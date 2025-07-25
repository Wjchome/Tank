package main

import (
	"bufio"
	"encoding/binary"
	"fmt"
	"log"
	"net"
	"sync"
	"time"

	myproto "github.com/WjcHome/gohello/proto" // 替换为你的proto包路径
	"google.golang.org/protobuf/proto"
)

const (
	FRAME_INTERVAL   = 25 * time.Millisecond // 20帧每秒
	MAX_ROOM_PLAYERS = 2
)

type Client struct {
	ID     string
	Conn   net.Conn
	RoomID string
	IsHost bool   // 是否是房主
	Name   string // 玩家名字
	ColorR int32  // 颜色R分量
	ColorG int32  // 颜色G分量
	ColorB int32  // 颜色B分量
}

type Room struct {
	ID               string
	Name             string
	HostID           string
	Clients          map[string]*Client
	InputBuffer      []*myproto.PlayerInput
	ChooseFoodBuffer []*myproto.ChooseFoodRequest // 新增：奖励选择缓冲区
	FrameNumber      int64
	Status           string // "waiting", "playing", "ended"
	MaxPlayers       int32
	Mutex            sync.Mutex
}

type Server struct {
	Rooms map[string]*Room
	Mutex sync.Mutex
}

// 服务器唯一
func NewServer() *Server {
	return &Server{
		Rooms: make(map[string]*Room),
	}
}

func (s *Server) Start() {
	ln, err := net.Listen("tcp", ":8080")
	if err != nil {
		log.Fatal(err)
	}
	defer ln.Close()
	fmt.Println("Frame Sync Relay Server with Room Management started on :8080")

	for {
		conn, err := ln.Accept()
		if err != nil {
			log.Println("Accept error:", err)
			continue
		}
		//监测到客户端连接，执行逻辑
		go s.handleClient(conn)
	}
}

func (s *Server) handleClient(conn net.Conn) {
	defer conn.Close()
	clientID := fmt.Sprintf("client_%d", time.Now().UnixNano())
	client := &Client{ID: clientID, Conn: conn}

	// 发送连接成功消息
	connectMsg := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_ConnectSuccess{
			ConnectSuccess: &myproto.ConnectSuccess{
				YourPlayerId: clientID,
			},
		},
	}
	s.sendMessage(conn, connectMsg)

	reader := bufio.NewReader(conn)
	for {
		lengthBytes := make([]byte, 4)
		_, err := reader.Read(lengthBytes)
		if err != nil {
			log.Println("Read length error:", err)
			break
		}
		length := binary.BigEndian.Uint32(lengthBytes)
		data := make([]byte, length)
		_, err = reader.Read(data)
		if err != nil {
			log.Println("Read data error:", err)
			break
		}
		var clientMsg myproto.ClientMessage
		if err := proto.Unmarshal(data, &clientMsg); err != nil {
			log.Println("Unmarshal error:", err)
			continue
		}

		// 处理不同类型的客户端消息
		switch data := clientMsg.Data.(type) {
		case *myproto.ClientMessage_PlayerInput:
			s.handlePlayerInput(client, data.PlayerInput)
		case *myproto.ClientMessage_CreateRoomRequest:
			s.handleCreateRoom(client, data.CreateRoomRequest)
		case *myproto.ClientMessage_JoinRoomRequest:
			s.handleJoinRoom(client, data.JoinRoomRequest)
		case *myproto.ClientMessage_RoomListRequest:
			s.sendRoomList(client.Conn)
		case *myproto.ClientMessage_GameStartRequest:
			s.startGame(s.Rooms[client.RoomID], data.GameStartRequest.Level)
		case *myproto.ClientMessage_LeaveRoomRequest:
			s.handleLeaveRoomRequest(client, data.LeaveRoomRequest)
		case *myproto.ClientMessage_KickPlayerRequest:
			s.handleKickPlayerRequest(client, data.KickPlayerRequest)
		case *myproto.ClientMessage_GameOverRequest:
			s.handleGameOverRequest(client, data.GameOverRequest)
		case *myproto.ClientMessage_ChooseFoodRequest:
			s.handleChooseFood(client, data.ChooseFoodRequest)
		default:
			log.Printf("Unknown message type from client %s", client.ID)
		}
	}

	// 客户端断开连接
	s.handleClientDisconnect(client)
}

func (s *Server) handlePlayerInput(client *Client, input *myproto.PlayerInput) {
	if client.RoomID == "" {
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		return
	}

	room.Mutex.Lock()
	//将客户端的输入添加到房间的输入缓冲区
	room.InputBuffer = append(room.InputBuffer, input)
	room.Mutex.Unlock()
}

func (s *Server) handleCreateRoom(client *Client, req *myproto.CreateRoomRequest) {
	roomID := fmt.Sprintf("room_%d", time.Now().UnixNano())
	roomName := req.RoomName
	if roomName == "" {
		roomName = fmt.Sprintf("Room %s", roomID[:8])
	}
	maxPlayers := req.MaxPlayers
	if maxPlayers <= 0 {
		maxPlayers = MAX_ROOM_PLAYERS
	}

	// 保存玩家信息
	client.Name = req.PlayerName
	client.ColorR = req.ColorR
	client.ColorG = req.ColorG
	client.ColorB = req.ColorB

	room := &Room{
		ID:         roomID,
		Name:       roomName,
		HostID:     client.ID,
		Clients:    make(map[string]*Client),
		Status:     "waiting",
		MaxPlayers: maxPlayers,
	}

	s.Mutex.Lock()
	s.Rooms[roomID] = room
	s.Mutex.Unlock()

	// 将客户端加入房间
	client.RoomID = roomID
	client.IsHost = true
	room.Clients[client.ID] = client

	// 直接返回RoomInfo
	s.sendRoomInfo(client.Conn, room, "")

	fmt.Printf("Client %s created room %s (%s)\n", client.ID, roomID, roomName)
}

func (s *Server) handleJoinRoom(client *Client, req *myproto.JoinRoomRequest) {
	fmt.Println("join")
	s.Mutex.Lock()
	room, exists := s.Rooms[req.RoomId]
	s.Mutex.Unlock()

	if !exists {
		// 返回空RoomInfo或带错误信息
		s.sendRoomInfo(client.Conn, nil, "Room not found")
		return
	}

	room.Mutex.Lock()

	if room.Status != "waiting" {
		room.Mutex.Unlock()
		s.sendRoomInfo(client.Conn, room, "Room is not available")
		return
	}

	if int32(len(room.Clients)) >= room.MaxPlayers {
		room.Mutex.Unlock()
		s.sendRoomInfo(client.Conn, room, "Room is full")
		return
	}

	// 保存玩家信息
	client.Name = req.PlayerName
	client.ColorR = req.ColorR
	client.ColorG = req.ColorG
	client.ColorB = req.ColorB

	// 加入房间
	client.RoomID = room.ID
	room.Clients[client.ID] = client
	room.Mutex.Unlock() // 释放房间锁，避免死锁

	fmt.Println("send")
	// 广播给房间内所有客户端
	for _, c := range room.Clients {
		s.sendRoomInfo(c.Conn, room, "")
	}

	fmt.Printf("Client %s joined room %s\n", client.ID, room.ID)
}

func (s *Server) handleLeaveRoomRequest(client *Client, req *myproto.LeaveRoomRequest) {
	fmt.Println("leave room request")

	if client.RoomID == "" {
		// 客户端不在任何房间中
		s.sendRoomInfo(client.Conn, nil, "Not in any room")
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		// 房间不存在
		s.sendRoomInfo(client.Conn, nil, "Room not found")
		return
	}

	room.Mutex.Lock()

	// 从房间中移除客户端
	delete(room.Clients, client.ID)
	client.RoomID = ""
	client.IsHost = false

	// 如果房主离开，选择新的房主
	if room.HostID == client.ID && len(room.Clients) > 0 {
		for _, c := range room.Clients {
			c.IsHost = true
			room.HostID = c.ID
			break
		}
	}

	room.Mutex.Unlock()

	// 如果房间空了，删除房间
	if len(room.Clients) == 0 {
		s.Mutex.Lock()
		delete(s.Rooms, room.ID)
		s.Mutex.Unlock()
		fmt.Printf("Room %s deleted (empty)\n", room.ID)
	} else {
		// 广播房间信息更新给剩余玩家
		s.broadcastRoomInfo(room)
	}

	// 发送房间列表给离开的客户端，表示已成功离开房间
	s.sendRoomList(client.Conn)

	fmt.Printf("Client %s left room %s\n", client.ID, req.RoomId)
}

func (s *Server) handleKickPlayerRequest(client *Client, req *myproto.KickPlayerRequest) {
	fmt.Printf("Kick player request from %s to kick %s\n", client.ID, req.TargetPlayerId)

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	if !exists {
		fmt.Printf("Room %s not found\n", client.RoomID)
		return
	}
	s.Mutex.Unlock()

	room.Mutex.Lock()

	// 检查目标玩家是否存在
	targetClient := room.Clients[req.TargetPlayerId]

	// 从房间中移除目标玩家
	delete(room.Clients, req.TargetPlayerId)
	targetClient.RoomID = ""
	targetClient.IsHost = false

	room.Mutex.Unlock()

	// 发送房间列表给被踢的玩家
	s.sendRoomList(targetClient.Conn)

	// 广播房间信息更新给剩余玩家
	s.broadcastRoomInfo(room)

	fmt.Printf("Player %s was kicked from room %s by host %s\n", req.TargetPlayerId, room.ID, client.ID)
}
func (s *Server) handleGameOverRequest(client *Client, req *myproto.GameOverRequest) {
	s.Mutex.Lock()
	room := s.Rooms[client.RoomID]
	s.Mutex.Unlock()
	room.Mutex.Lock()
	// 从房间中移除客户端
	delete(room.Clients, client.ID)
	client.RoomID = ""
	client.IsHost = false
	room.Mutex.Unlock()
	// 发送房间列表给离开的客户端，表示已成功离开房间
	s.sendRoomList(client.Conn)

}

func (s *Server) handleClientDisconnect(client *Client) {
	if client.RoomID == "" {
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		return
	}

	room.Mutex.Lock()
	delete(room.Clients, client.ID)

	// 如果房主离开，选择新的房主
	if client.IsHost && len(room.Clients) > 0 {
		for _, c := range room.Clients {
			c.IsHost = true
			room.HostID = c.ID
			break
		}
	}

	// 如果房间空了，删除房间
	if len(room.Clients) == 0 {
		room.Mutex.Unlock()
		s.Mutex.Lock()
		delete(s.Rooms, room.ID)
		s.Mutex.Unlock()
		fmt.Printf("Room %s deleted (empty)\n", room.ID)
		return
	}

	room.Mutex.Unlock()

	// 广播房间信息更新
	s.broadcastRoomInfo(room)
	fmt.Printf("Client %s left room %s\n", client.ID, room.ID)
}

func (s *Server) startGame(room *Room, level int32) {
	room.Status = "playing"

	// 生成随机种子
	randomSeed := time.Now().UnixNano()

	// 收集玩家信息
	playerInfos := make([]*myproto.PlayerInfo, 0, len(room.Clients))
	for _, c := range room.Clients {
		playerInfos = append(playerInfos, &myproto.PlayerInfo{
			PlayerId:   c.ID,
			PlayerName: c.Name,
			ColorR:     c.ColorR,
			ColorG:     c.ColorG,
			ColorB:     c.ColorB,
		})
	}

	// 发送游戏开始消息
	gameStart := &myproto.GameStart{
		RoomId:      room.ID,
		PlayerInfos: playerInfos,
		RandomSeed:  randomSeed,
		Level:       level,
	}
	serverMsg := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_GameStart{
			GameStart: gameStart,
		},
	}

	// 广播给房间内所有客户端
	for _, c := range room.Clients {
		s.sendMessage(c.Conn, serverMsg)
	}

	// 启动房间帧循环
	go room.frameLoop()

	fmt.Printf("Game started in room %s with %d players\n", room.ID, len(room.Clients))
}

func (s *Server) sendRoomList(conn net.Conn) {
	s.Mutex.Lock()
	rooms := make([]*myproto.RoomInfo, 0, len(s.Rooms))
	for _, room := range s.Rooms {
		room.Mutex.Lock()
		playerIDs := make([]string, 0, len(room.Clients))
		playerInfos := make([]*myproto.PlayerInfo, 0, len(room.Clients))
		var hostName string
		for _, c := range room.Clients {
			playerIDs = append(playerIDs, c.ID)
			playerInfos = append(playerInfos, &myproto.PlayerInfo{
				PlayerId:   c.ID,
				PlayerName: c.Name,
				ColorR:     c.ColorR,
				ColorG:     c.ColorG,
				ColorB:     c.ColorB,
			})
			// 获取房主名字
			if c.ID == room.HostID {
				hostName = c.Name
			}
		}
		roomInfo := &myproto.RoomInfo{
			RoomId:      room.ID,
			PlayerIds:   playerIDs,
			Status:      room.Status,
			HostId:      room.HostID,
			MaxPlayers:  room.MaxPlayers,
			RoomName:    room.Name,
			PlayerInfos: playerInfos, // 添加玩家详细信息
			HostName:    hostName,    // 添加房主名字
		}
		rooms = append(rooms, roomInfo)
		room.Mutex.Unlock()
	}
	s.Mutex.Unlock()

	roomList := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_RoomList{
			RoomList: &myproto.RoomList{
				Rooms: rooms,
			},
		},
	}
	s.sendMessage(conn, roomList)
}

func (s *Server) broadcastRoomInfo(room *Room) {
	room.Mutex.Lock()
	playerIDs := make([]string, 0, len(room.Clients))
	playerInfos := make([]*myproto.PlayerInfo, 0, len(room.Clients))
	var hostName string
	for _, c := range room.Clients {
		playerIDs = append(playerIDs, c.ID)
		playerInfos = append(playerInfos, &myproto.PlayerInfo{
			PlayerId:   c.ID,
			PlayerName: c.Name,
			ColorR:     c.ColorR,
			ColorG:     c.ColorG,
			ColorB:     c.ColorB,
		})
		// 获取房主名字
		if c.ID == room.HostID {
			hostName = c.Name
		}
	}
	roomInfo := &myproto.RoomInfo{
		RoomId:      room.ID,
		PlayerIds:   playerIDs,
		Status:      room.Status,
		HostId:      room.HostID,
		MaxPlayers:  room.MaxPlayers,
		RoomName:    room.Name,
		PlayerInfos: playerInfos, // 添加玩家详细信息
		HostName:    hostName,    // 添加房主名字
	}
	room.Mutex.Unlock()

	serverMsg := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_RoomInfo{
			RoomInfo: roomInfo,
		},
	}

	// 广播给房间内所有客户端
	for _, c := range room.Clients {
		s.sendMessage(c.Conn, serverMsg)
	}
}

// 发送给客户端的消息
func (s *Server) sendMessage(conn net.Conn, msg *myproto.ServerMessage) {
	data, err := proto.Marshal(msg)
	if err != nil {
		log.Println("Marshal error:", err)
		return
	}
	lengthBytes := make([]byte, 4)
	binary.BigEndian.PutUint32(lengthBytes, uint32(len(data)))
	conn.Write(lengthBytes)
	conn.Write(data)
}

// 发送RoomInfo（Room对象或错误信息）
func (s *Server) sendRoomInfo(conn net.Conn, room *Room, errMsg string) {
	var roomInfo *myproto.RoomInfo
	if room != nil {
		room.Mutex.Lock()
		playerIDs := make([]string, 0, len(room.Clients))
		playerInfos := make([]*myproto.PlayerInfo, 0, len(room.Clients))
		var hostName string
		for _, c := range room.Clients {
			playerIDs = append(playerIDs, c.ID)
			playerInfos = append(playerInfos, &myproto.PlayerInfo{
				PlayerId:   c.ID,
				PlayerName: c.Name,
				ColorR:     c.ColorR,
				ColorG:     c.ColorG,
				ColorB:     c.ColorB,
			})
			// 获取房主名字
			if c.ID == room.HostID {
				hostName = c.Name
			}
		}
		roomInfo = &myproto.RoomInfo{
			RoomId:      room.ID,
			PlayerIds:   playerIDs,
			Status:      room.Status,
			HostId:      room.HostID,
			MaxPlayers:  room.MaxPlayers,
			RoomName:    room.Name,
			PlayerInfos: playerInfos, // 添加玩家详细信息
			HostName:    hostName,    // 添加房主名字
		}
		room.Mutex.Unlock()
		if errMsg != "" {
			roomInfo.Status = "error"
			roomInfo.RoomName = errMsg
		}
	} else {
		roomInfo = &myproto.RoomInfo{
			RoomId:   "",
			Status:   "error",
			RoomName: errMsg,
		}
	}
	serverMsg := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_RoomInfo{
			RoomInfo: roomInfo,
		},
	}
	s.sendMessage(conn, serverMsg)
}

func (s *Server) handleChooseFood(client *Client, req *myproto.ChooseFoodRequest) {
	if client.RoomID == "" {
		return
	}
	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()
	if !exists {
		return
	}
	room.Mutex.Lock()
	room.ChooseFoodBuffer = append(room.ChooseFoodBuffer, req)
	room.Mutex.Unlock()
}

func (room *Room) frameLoop() {
	ticker := time.NewTicker(FRAME_INTERVAL)
	defer ticker.Stop()
	// startTime := time.Now() // 已不再使用，移除

	for range ticker.C {
		room.Mutex.Lock()
		inputs := room.InputBuffer
		chooseFoods := room.ChooseFoodBuffer
		frameNum := room.FrameNumber
		room.InputBuffer = make([]*myproto.PlayerInput, 0)
		room.ChooseFoodBuffer = make([]*myproto.ChooseFoodRequest, 0)
		room.FrameNumber++
		clients := make([]*Client, 0, len(room.Clients))
		for _, c := range room.Clients {
			clients = append(clients, c)
		}
		room.Mutex.Unlock()

		if len(clients) == 0 {
			continue
		}

		// 每帧都发送FrameMessage
		frameMsg := &myproto.FrameMessage{
			FrameNumber:        frameNum,
			Inputs:             inputs,
			ChooseFoodRequests: chooseFoods,
		}
		serverMsg := &myproto.ServerMessage{
			Data: &myproto.ServerMessage_FrameMessage{
				FrameMessage: frameMsg,
			},
		}
		data, err := proto.Marshal(serverMsg)
		if err != nil {
			log.Println("Marshal error:", err)
			continue
		}
		lengthBytes := make([]byte, 4)
		binary.BigEndian.PutUint32(lengthBytes, uint32(len(data)))
		for _, client := range clients {
			client.Conn.Write(lengthBytes)
			client.Conn.Write(data)
		}
	}
}

func main() {
	server := NewServer()
	server.Start()
}
