package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"log"
	"net"
	"sync"
	"time"
)

type Server struct {
	clients   map[string]*Client
	gameRooms map[string]*GameRoom
	mutex     sync.RWMutex
}

type Client struct {
	ID       string
	Conn     net.Conn
	RoomID   string
	PlayerID string
}

type GameRoom struct {
	ID      string
	Players map[string]*Player
	State   string // "waiting", "playing", "ended"
	mutex   sync.RWMutex
}

type Player struct {
	ID        string `json:"id"`
	X         int    `json:"x"`
	Y         int    `json:"y"`
	Direction string `json:"direction"` // "up", "down", "left", "right"
	HP        int    `json:"hp"`
	LastMove  int64  `json:"lastMove"`
	LastShoot int64  `json:"lastShoot"`
}

type Message struct {
	Type string      `json:"type"`
	Data interface{} `json:"data"`
}

type PlayerMoveData struct {
	PlayerID  string `json:"playerID"`
	X         int    `json:"x"`
	Y         int    `json:"y"`
	Direction string `json:"direction"`
	Timestamp int64  `json:"timestamp"`
}

type BulletCreateData struct {
	ID        string `json:"id"`
	PlayerID  string `json:"playerID"`
	X         int    `json:"x"`
	Y         int    `json:"y"`
	Direction string `json:"direction"`
	Timestamp int64  `json:"timestamp"`
}

type BulletDestroyData struct {
	ID       string `json:"id"`
	PlayerID string `json:"playerID"`
	HitX     int    `json:"hitX"`
	HitY     int    `json:"hitY"`
}

type PlayerHitData struct {
	PlayerID string `json:"playerID"`
	HP       int    `json:"hp"`
	HitByID  string `json:"hitByID"`
}

type GameStateData struct {
	Players map[string]*Player `json:"players"`
	State   string             `json:"state"`
}

func NewServer() *Server {
	return &Server{
		clients:   make(map[string]*Client),
		gameRooms: make(map[string]*GameRoom),
	}
}

func (s *Server) Start() {
	//监听端口
	listener, err := net.Listen("tcp", ":8080")
	if err != nil {
		log.Fatal("Failed to start server:", err)
	}
	defer listener.Close()

	fmt.Println("Tank Battle Server started on :8080")

	for {
		conn, err := listener.Accept()
		if err != nil {
			log.Println("Failed to accept connection:", err)
			continue
		}

		go s.handleClient(conn)
	}
}

func (s *Server) handleClient(conn net.Conn) {
	defer conn.Close()
	//给clientID赋值 和时间相关 转化成string
	clientID := fmt.Sprintf("client_%d", time.Now().UnixNano())
	client := &Client{ //赋值两个参数
		ID:   clientID,
		Conn: conn,
	}

	s.mutex.Lock()
	s.clients[clientID] = client //哈希
	s.mutex.Unlock()

	// 自动匹配到房间
	roomID := s.findOrCreateRoom()
	s.joinRoom(client, roomID)

	// 使用bufio.Scanner来处理粘包问题
	scanner := bufio.NewScanner(conn)
	for scanner.Scan() {
		line := scanner.Text()
		if line == "" {
			continue
		}

		var msg Message
		if err := json.Unmarshal([]byte(line), &msg); err != nil {
			log.Printf("Failed to unmarshal message: %v", err)
			continue
		}

		s.handleMessage(client, msg)
	}

	if err := scanner.Err(); err != nil {
		log.Printf("Scanner error: %v", err)
	}

	s.removeClient(clientID)
}

// 获得房间ID
func (s *Server) findOrCreateRoom() string {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	// 寻找等待中的房间 可用房间
	for _, room := range s.gameRooms {
		room.mutex.RLock()
		if room.State == "waiting" && len(room.Players) < 2 {
			room.mutex.RUnlock()
			return room.ID
		}
		room.mutex.RUnlock()
	}

	// 没有等待中的房间，则创建新房间
	roomID := fmt.Sprintf("room_%d", time.Now().UnixNano())
	room := &GameRoom{
		ID:      roomID,
		Players: make(map[string]*Player),
		State:   "waiting",
	}
	//房间哈希
	s.gameRooms[roomID] = room
	return roomID
}

func (s *Server) joinRoom(client *Client, roomID string) {
	s.mutex.Lock()
	room := s.gameRooms[roomID]
	s.mutex.Unlock()

	room.mutex.Lock()
	defer room.mutex.Unlock()

	client.RoomID = roomID
	playerID := fmt.Sprintf("player_%d", len(room.Players)+1) //1 或者 2
	client.PlayerID = playerID

	// 设置初始位置
	var x, y int
	if len(room.Players) == 0 {
		x, y = 1, 1 // 玩家1在左上
	} else {
		x, y = 18, 18 // 玩家2在右下
	}

	player := &Player{
		ID:        playerID,
		X:         x,
		Y:         y,
		Direction: "up",
		HP:        3,
		LastMove:  0,
		LastShoot: 0,
	}

	room.Players[playerID] = player

	// 发送玩家ID给客户端
	s.sendToClient(client, Message{
		Type: "player_assigned",
		Data: map[string]string{"playerID": playerID},
	})

	// 如果房间满了，开始游戏
	if len(room.Players) == 1 {
		room.State = "playing"
		go func() {
			time.Sleep(100 * time.Millisecond)
			s.broadcastToRoom(roomID, Message{
				Type: "game_start",
				Data: GameStateData{
					Players: room.Players,
					State:   room.State,
				},
			})
		}()
	}
}

func (s *Server) handleMessage(client *Client, msg Message) {
	switch msg.Type {
	case "player_move":
		s.handlePlayerMove(client, msg.Data)
	case "bullet_create":
		s.handleBulletCreate(client, msg.Data)
	case "bullet_destroy":
		s.handleBulletDestroy(client, msg.Data)
	case "player_hit":
		s.handlePlayerHit(client, msg.Data)
	}
}

func (s *Server) handlePlayerMove(client *Client, data interface{}) {
	var moveData PlayerMoveData
	jsonData, _ := json.Marshal(data)
	json.Unmarshal(jsonData, &moveData)

	// 简单验证：检查是否是该客户端的玩家
	if moveData.PlayerID != client.PlayerID {
		return
	}

	// 更新服务器记录
	s.mutex.RLock()
	room := s.gameRooms[client.RoomID]
	s.mutex.RUnlock()

	room.mutex.Lock()
	if player, exists := room.Players[moveData.PlayerID]; exists {
		player.X = moveData.X
		player.Y = moveData.Y
		player.Direction = moveData.Direction
		player.LastMove = moveData.Timestamp
	}
	room.mutex.Unlock()

	// 转发给房间内其他玩家
	s.broadcastToRoomExcept(client.RoomID, client.ID, Message{
		Type: "player_move",
		Data: moveData,
	})
}

func (s *Server) handleBulletCreate(client *Client, data interface{}) {
	var bulletData BulletCreateData
	jsonData, _ := json.Marshal(data)
	json.Unmarshal(jsonData, &bulletData)

	if bulletData.PlayerID != client.PlayerID {
		return
	}

	// 转发给房间内其他玩家
	s.broadcastToRoomExcept(client.RoomID, client.ID, Message{
		Type: "bullet_create",
		Data: bulletData,
	})
}

func (s *Server) handleBulletDestroy(client *Client, data interface{}) {
	var destroyData BulletDestroyData
	jsonData, _ := json.Marshal(data)
	json.Unmarshal(jsonData, &destroyData)

	if destroyData.PlayerID != client.PlayerID {
		return
	}

	s.broadcastToRoomExcept(client.RoomID, client.ID, Message{
		Type: "bullet_destroy",
		Data: destroyData,
	})
}

func (s *Server) handlePlayerHit(client *Client, data interface{}) {
	var hitData PlayerHitData
	jsonData, _ := json.Marshal(data)
	json.Unmarshal(jsonData, &hitData)

	s.mutex.RLock()
	room := s.gameRooms[client.RoomID]
	s.mutex.RUnlock()

	room.mutex.Lock()
	if player, exists := room.Players[hitData.PlayerID]; exists {
		player.HP = hitData.HP
		if player.HP <= 0 {
			room.State = "ended"
		}
	}
	room.mutex.Unlock()

	// 广播给房间内所有玩家
	s.broadcastToRoom(client.RoomID, Message{
		Type: "player_hit",
		Data: hitData,
	})

	// 如果游戏结束
	if room.State == "ended" {
		s.broadcastToRoom(client.RoomID, Message{
			Type: "game_end",
			Data: map[string]string{"winner": hitData.HitByID},
		})
	}
}

func (s *Server) sendToClient(client *Client, msg Message) {
	jsonData, err := json.Marshal(msg)
	if err != nil {
		log.Printf("Failed to marshal message: %v", err)
		return
	}

	// 添加换行符作为消息分隔符
	message := string(jsonData) + "\n"

	_, err = client.Conn.Write([]byte(message))
	if err != nil {
		log.Printf("Failed to send message to client %s: %v", client.ID, err)
	}

}

func (s *Server) broadcastToRoom(roomID string, msg Message) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	for _, client := range s.clients {
		if client.RoomID == roomID {
			s.sendToClient(client, msg)

		}
	}
}

func (s *Server) broadcastToRoomExcept(roomID, exceptClientID string, msg Message) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	for clientID, client := range s.clients {
		if client.RoomID == roomID && clientID != exceptClientID {
			s.sendToClient(client, msg)
		}
	}
}

func (s *Server) removeClient(clientID string) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	if client, exists := s.clients[clientID]; exists {
		if client.RoomID != "" {
			// 通知房间内其他玩家
			s.broadcastToRoomExcept(client.RoomID, clientID, Message{
				Type: "player_disconnected",
				Data: map[string]string{"playerID": client.PlayerID},
			})
		}
		delete(s.clients, clientID)
	}
}

func main() {
	server := NewServer() //返回 *Server
	server.Start()
}
