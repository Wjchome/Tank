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
	FRAME_INTERVAL   = 50 * time.Millisecond // 20帧每秒
	MAX_ROOM_PLAYERS = 2
)

type Client struct {
	ID     string
	Conn   net.Conn
	RoomID string
}

type Room struct {
	ID          string
	Clients     map[string]*Client
	InputBuffer []*myproto.PlayerInput
	FrameNumber int64
	Mutex       sync.Mutex
}

type Server struct {
	Rooms map[string]*Room
	Mutex sync.Mutex
}

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
	fmt.Println("Frame Sync Relay Server with Room started on :8080")

	for {
		conn, err := ln.Accept()
		if err != nil {
			log.Println("Accept error:", err)
			continue
		}
		go s.handleClient(conn)
	}
}

func (s *Server) handleClient(conn net.Conn) {
	defer conn.Close()
	clientID := fmt.Sprintf("client_%d", time.Now().UnixNano())
	client := &Client{ID: clientID, Conn: conn}

	connectMsg := &myproto.ServerMessage{
		Data: &myproto.ServerMessage_ConnectSuccess{
			ConnectSuccess: &myproto.ConnectSuccess{
				YourPlayerId: clientID,
			},
		},
	}
	data, _ := proto.Marshal(connectMsg)
	lengthBytes := make([]byte, 4)
	binary.BigEndian.PutUint32(lengthBytes, uint32(len(data)))
	conn.Write(lengthBytes)
	conn.Write(data)
	// 分配房间
	room := s.findOrCreateRoom()
	client.RoomID = room.ID

	var (
		playerIDs            []string
		roomFull             bool
		shouldStartFrameLoop bool
		clientsCopy          []*Client
	)

	room.Mutex.Lock()
	room.Clients[clientID] = client
	playerIDs = make([]string, 0, len(room.Clients))
	for _, c := range room.Clients {
		playerIDs = append(playerIDs, c.ID)
	}
	roomFull = len(room.Clients) == MAX_ROOM_PLAYERS
	shouldStartFrameLoop = len(room.Clients) == 1
	clientsCopy = make([]*Client, 0, len(room.Clients))
	for _, c := range room.Clients {
		clientsCopy = append(clientsCopy, c)
	}
	room.Mutex.Unlock()

	fmt.Printf("Client %s joined room %s\n", clientID, room.ID)

	// 启动房间帧循环（只在第一个玩家加入时）
	if shouldStartFrameLoop {
		go room.frameLoop()
	}

	// 房间满员时广播 GameStart
	if roomFull {
		randomSeed := time.Now().UnixNano() // 新增：生成随机种子
		gameStart := &myproto.GameStart{
			RoomId:     room.ID,
			PlayerIds:  playerIDs,
			RandomSeed: randomSeed, // 新增
		}
		serverMsg := &myproto.ServerMessage{
			Data: &myproto.ServerMessage_GameStart{
				GameStart: gameStart,
			},
		}
		data, err := proto.Marshal(serverMsg)
		if err != nil {
			log.Println("Marshal error:", err)
			return
		}
		lengthBytes := make([]byte, 4)
		binary.BigEndian.PutUint32(lengthBytes, uint32(len(data)))

		for _, c := range clientsCopy {
			c.Conn.Write(lengthBytes)
			c.Conn.Write(data)
		}
	}

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
		if pi, ok := clientMsg.Data.(*myproto.ClientMessage_PlayerInput); ok {
			room.Mutex.Lock()
			room.InputBuffer = append(room.InputBuffer, pi.PlayerInput)
			room.Mutex.Unlock()
		}
	}

	// 客户端断开
	room.Mutex.Lock()
	delete(room.Clients, clientID)
	room.Mutex.Unlock()
	fmt.Printf("Client %s left room %s\n", clientID, room.ID)
}

func (s *Server) findOrCreateRoom() *Room {
	s.Mutex.Lock()
	defer s.Mutex.Unlock()
	for _, room := range s.Rooms {
		room.Mutex.Lock()
		if len(room.Clients) < MAX_ROOM_PLAYERS {
			room.Mutex.Unlock()
			return room
		}
		room.Mutex.Unlock()
	}
	// 没有可用房间，新建一个
	roomID := fmt.Sprintf("room_%d", time.Now().UnixNano())
	room := &Room{
		ID:          roomID,
		Clients:     make(map[string]*Client),
		InputBuffer: make([]*myproto.PlayerInput, 0),
		FrameNumber: 0,
	}
	s.Rooms[roomID] = room
	return room
}

func (room *Room) frameLoop() {
	ticker := time.NewTicker(FRAME_INTERVAL)
	defer ticker.Stop()
	for range ticker.C {
		room.Mutex.Lock()
		inputs := room.InputBuffer
		room.InputBuffer = make([]*myproto.PlayerInput, 0)
		room.FrameNumber++
		clients := make([]*Client, 0, len(room.Clients))
		for _, c := range room.Clients {
			clients = append(clients, c)
		}
		room.Mutex.Unlock()

		if len(inputs) == 0 || len(clients) == 0 {
			continue
		}

		frameInputs := &myproto.FrameInputs{
			FrameNumber: room.FrameNumber,
			Inputs:      inputs,
		}
		serverMsg := &myproto.ServerMessage{
			Data: &myproto.ServerMessage_FrameInputs{
				FrameInputs: frameInputs,
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
