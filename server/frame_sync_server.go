package main

import (
	"bufio"
	"crypto/rand"
	"encoding/binary"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"log"
	"net"
	"sync"
	"sync/atomic"
	"time"

	myproto "github.com/WjcHome/gohello/proto" // 使用模块路径导入proto包
	"google.golang.org/protobuf/proto"
)

const (
	FRAME_INTERVAL = 50 * time.Millisecond // 20帧每秒
)

// 全局客户端计数器
var clientCounter int64 = 0

// 生成唯一客户端ID
func generateClientID() string {
	// 使用原子操作递增计数器
	counter := atomic.AddInt64(&clientCounter, 1)

	// 生成随机字节
	randomBytes := make([]byte, 8)
	rand.Read(randomBytes)
	randomHex := hex.EncodeToString(randomBytes)

	// 组合时间戳、计数器和随机数
	timestamp := time.Now().UnixNano()
	return fmt.Sprintf("client_%d_%d_%s", timestamp, counter, randomHex[:8])
}

// 服务器信息结构
type ServerInfo struct {
	ServerIP   string `json:"serverIP"`
	GamePort   int    `json:"gamePort"`
	ServerName string `json:"serverName"`
	Version    string `json:"version"`
}

type Client struct {
	ID         string
	Conn       net.Conn
	RoomID     string
	IsHost     bool   // 是否是房主
	Name       string // 玩家名字
	ColorR     int32  // 颜色R分量
	ColorG     int32  // 颜色G分量
	ColorB     int32  // 颜色B分量
	PlayerRole int32  // 角色选择
}

type Room struct {
	ID              string
	Name            string
	HostID          string
	Clients         map[string]*Client
	FrameDataBuffer []*myproto.FrameData // 帧数据缓冲区（包含移动、开火、选择食物）
	FrameNumber     int64
	Status          string // "waiting", "playing"
	MaxPlayers      int32
	Mutex           sync.Mutex
}

type Server struct {
	Rooms map[string]*Room
	Mutex sync.Mutex
	// 新增字段
	broadcastPort int
	serverInfo    ServerInfo
	stopBroadcast chan bool
}

// 服务器唯一
func NewServer() *Server {
	return &Server{
		Rooms:         make(map[string]*Room),
		broadcastPort: 9999,
		serverInfo: ServerInfo{
			ServerName: "Tank Game Server",
			Version:    "1.0",
			GamePort:   8080,
		},
		stopBroadcast: make(chan bool),
	}
}

func (s *Server) Start() {
	// 启动UDP广播
	//	go s.startBroadcast()

	// 启动定期清理任务
	go s.cleanupEmptyRooms()

	ln, err := net.Listen("tcp", ":8088")
	if err != nil {
		log.Fatal(err)
	}
	defer ln.Close()
	//fmt.Println("Frame Sync Relay Server with Room Management started on :8080")
	//fmt.Println("UDP broadcast started on :9999")

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

// 新增：启动UDP广播
func (s *Server) startBroadcast() {
	// 获取本机IP
	localIP := s.getLocalIP()
	if localIP == "" {
		log.Println("Failed to get local IP")
		return
	}

	s.serverInfo.ServerIP = localIP

	// 创建UDP连接
	conn, err := net.DialUDP("udp", nil, &net.UDPAddr{
		IP:   net.IPv4(255, 255, 255, 255),
		Port: s.broadcastPort,
	})
	if err != nil {
		log.Println("Failed to create UDP broadcast connection:", err)
		return
	}
	defer conn.Close()

	ticker := time.NewTicker(2 * time.Second) // 每2秒广播一次
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			s.broadcastServerInfo(conn)
		case <-s.stopBroadcast:
			return
		}
	}
}

// 新增：广播服务器信息
func (s *Server) broadcastServerInfo(conn *net.UDPConn) {
	data, err := json.Marshal(s.serverInfo)
	if err != nil {
		log.Println("Failed to marshal server info:", err)
		return
	}

	_, err = conn.Write(data)
	if err != nil {
		log.Println("Failed to broadcast server info:", err)
	} else {
		log.Printf("Broadcasting server info: %s:%d", s.serverInfo.ServerIP, s.serverInfo.GamePort)
	}
}

// 新增：获取本机IP
func (s *Server) getLocalIP() string {
	addrs, err := net.InterfaceAddrs()
	if err != nil {
		return ""
	}

	for _, addr := range addrs {
		if ipnet, ok := addr.(*net.IPNet); ok && !ipnet.IP.IsLoopback() {
			if ipnet.IP.To4() != nil {
				return ipnet.IP.String()
			}
		}
	}
	return ""
}

func (s *Server) handleClient(conn net.Conn) {
	defer conn.Close()

	// 禁用Nagle算法，减少网络延迟
	if tcpConn, ok := conn.(*net.TCPConn); ok {
		tcpConn.SetNoDelay(true)
	}

	clientID := generateClientID()
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
		case *myproto.ClientMessage_FrameData:
			s.handleFrameData(client, data.FrameData)
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
		default:
			log.Printf("Unknown message type from client %s", client.ID)
		}
	}

	// 客户端断开连接
	s.handleClientDisconnect(client)
}

func (s *Server) handleFrameData(client *Client, frameData *myproto.FrameData) {
	if client.RoomID == "" {
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		return
	}

	// 确保player_id正确
	if frameData.PlayerId == "" {
		frameData.PlayerId = client.ID
	}

	room.Mutex.Lock()
	// 将客户端的帧数据添加到房间的缓冲区
	room.FrameDataBuffer = append(room.FrameDataBuffer, frameData)
	room.Mutex.Unlock()
}

func (s *Server) handleCreateRoom(client *Client, req *myproto.CreateRoomRequest) {
	roomID := fmt.Sprintf("room_%d", time.Now().UnixNano())
	roomName := req.RoomName
	if roomName == "" {
		roomName = fmt.Sprintf("Room %s", roomID[:8])
	}
	maxPlayers := req.MaxPlayers

	// 保存玩家信息
	client.Name = req.PlayerName
	client.ColorR = req.ColorR
	client.ColorG = req.ColorG
	client.ColorB = req.ColorB
	client.PlayerRole = req.PlayerRole

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
	client.PlayerRole = req.PlayerRole

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
	fmt.Printf("Game over request from client %s in room %s\n", client.ID, client.RoomID)

	if client.RoomID == "" {
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		fmt.Printf("Room %s not found for game over request\n", client.RoomID)
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

	// 游戏结束后房间状态保持playing，等待玩家重新开始或离开
	room.Mutex.Unlock()

	// 如果房间空了，删除房间
	if len(room.Clients) == 0 {
		s.Mutex.Lock()
		delete(s.Rooms, room.ID)
		s.Mutex.Unlock()
		fmt.Printf("Room %s deleted (empty after game over)\n", room.ID)
	} else {
		// 广播房间信息更新给剩余玩家
		s.broadcastRoomInfo(room)
	}

	// 发送房间列表给离开的客户端
	s.sendRoomList(client.Conn)
	fmt.Printf("Client %s game over and left room %s\n", client.ID, req.RoomId)
}

func (s *Server) handleClientDisconnect(client *Client) {
	fmt.Printf("Client %s disconnected\n", client.ID)

	if client.RoomID == "" {
		fmt.Printf("Client %s was not in any room\n", client.ID)
		return
	}

	s.Mutex.Lock()
	room, exists := s.Rooms[client.RoomID]
	s.Mutex.Unlock()

	if !exists {
		fmt.Printf("Room %s not found for disconnected client %s\n", client.RoomID, client.ID)
		return
	}

	room.Mutex.Lock()
	delete(room.Clients, client.ID)
	client.RoomID = ""
	client.IsHost = false

	// 如果房主离开，选择新的房主
	if room.HostID == client.ID && len(room.Clients) > 0 {
		for _, c := range room.Clients {
			c.IsHost = true
			room.HostID = c.ID
			fmt.Printf("New host selected: %s in room %s\n", c.ID, room.ID)
			break
		}
	}

	// 客户端断开时保持房间状态不变，只有房主转移

	// 如果房间空了，删除房间
	if len(room.Clients) == 0 {
		room.Mutex.Unlock()
		s.Mutex.Lock()
		delete(s.Rooms, room.ID)
		s.Mutex.Unlock()
		fmt.Printf("Room %s deleted (empty after disconnect)\n", room.ID)
		return
	}

	room.Mutex.Unlock()

	// 广播房间信息更新
	s.broadcastRoomInfo(room)
	fmt.Printf("Client %s disconnected from room %s, %d players remaining\n", client.ID, room.ID, len(room.Clients))
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
			PlayerRole: c.PlayerRole,
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

	// 延迟启动房间帧循环，确保GameStart消息先到达客户端
	go func() {
		time.Sleep(100 * time.Millisecond) // 等待100ms确保GameStart消息到达
		room.frameLoop()
	}()

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
				PlayerRole: c.PlayerRole,
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

// 定期清理空房间
func (s *Server) cleanupEmptyRooms() {
	ticker := time.NewTicker(30 * time.Second) // 每30秒检查一次
	defer ticker.Stop()

	for range ticker.C {
		s.Mutex.Lock()
		roomsToDelete := make([]string, 0)

		for roomID, room := range s.Rooms {
			room.Mutex.Lock()
			if len(room.Clients) == 0 {
				roomsToDelete = append(roomsToDelete, roomID)
				fmt.Printf("Marking room %s for deletion (empty)\n", roomID)
			}
			room.Mutex.Unlock()
		}

		// 删除空房间
		for _, roomID := range roomsToDelete {
			delete(s.Rooms, roomID)
			fmt.Printf("Room %s deleted by cleanup task\n", roomID)
		}
		s.Mutex.Unlock()

		if len(roomsToDelete) > 0 {
			fmt.Printf("Cleanup: deleted %d empty rooms\n", len(roomsToDelete))
		}
	}
}

func (room *Room) frameLoop() {
	ticker := time.NewTicker(FRAME_INTERVAL)
	defer ticker.Stop()

	fmt.Printf("Frame loop started for room %s\n", room.ID)

	for range ticker.C {
		room.Mutex.Lock()
		frameDatas := room.FrameDataBuffer
		room.FrameDataBuffer = make([]*myproto.FrameData, 0)
		room.FrameNumber++
		clients := make([]*Client, 0, len(room.Clients))
		for _, c := range room.Clients {
			clients = append(clients, c)
		}
		clientCount := len(clients)
		room.Mutex.Unlock()

		// 如果房间没有客户端，停止帧循环
		if clientCount == 0 {
			fmt.Printf("Room %s has no clients, stopping frame loop\n", room.ID)
			return
		}

		// 房间状态检查：只有在没有客户端时才停止循环
		// 游戏结束后房间状态保持playing，等待玩家重新开始

		// 每帧都发送ServerFrame
		currentTime := time.Now().UnixNano()
		serverFrame := &myproto.ServerFrame{
			TimeStamp:  currentTime,
			FrameDatas: frameDatas,
		}
		serverMsg := &myproto.ServerMessage{
			Data: &myproto.ServerMessage_ServerFrame{
				ServerFrame: serverFrame,
			},
		}
		data, err := proto.Marshal(serverMsg)
		if err != nil {
			log.Println("Marshal error:", err)
			continue
		}
		lengthBytes := make([]byte, 4)
		binary.BigEndian.PutUint32(lengthBytes, uint32(len(data)))

		// 发送给所有客户端，如果发送失败则记录错误
		for _, client := range clients {
			_, err := client.Conn.Write(lengthBytes)
			if err != nil {
				log.Printf("Failed to send frame to client %s: %v\n", client.ID, err)
				continue
			}
			_, err = client.Conn.Write(data)
			if err != nil {
				log.Printf("Failed to send frame data to client %s: %v\n", client.ID, err)
			}
		}
	}
}

func main() {
	server := NewServer()
	server.Start()
}
