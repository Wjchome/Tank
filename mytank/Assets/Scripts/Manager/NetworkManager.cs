using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Tankgame;

public class NetworkManager : SingletonMono<NetworkManager>
{
    private TcpClient tcpClient;//代表自己的连接
    private NetworkStream stream;//网络流
    private bool isConnected = false;
    
    public string playerID;
    public long currentFrame = 0;


    public List<RoomInfo> availableRooms = new List<RoomInfo>();
    public RoomInfo currentRoom;//自己所在的房间
    public bool isHost =>currentRoom?.HostId == playerID;

    // 事件定义
    public event Action<FrameInputs> OnFrameInputs;
    public event Action<EmptyFrame> OnEmptyFrame;
    public event Action<ConnectSuccess> OnConnectSuccess;
    public event Action<List<RoomInfo>> OnRoomListReceived;
    public event Action<RoomInfo> OnRoomInfoUpdate;
    public event Action<GameStart> OnGameStart;
    
    private Queue<ServerMessage> messageQueue = new Queue<ServerMessage>();
    private object queueLock = new object();
    private Thread receiveThread;
    private bool shouldStopThread = false;

    private void Start()
    {
        OnConnectSuccess += SetPlayerID;
        ConnectToServer();
    }
    
    
    void ConnectToServer()
    {
        try
        {
            // 检查是否是ParrelSync克隆的实例
            string projectName = Application.productName;
            string clientId = "";
            
            // 如果是克隆实例，添加唯一标识
            if (projectName.Contains("Clone"))
            {
                // 从项目名中提取克隆编号
                string[] parts = projectName.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out int cloneNumber))
                {
                    clientId = $"Clone_{cloneNumber}";
                    Debug.Log($"Clone instance detected: {clientId}");
                }
            }
            
            tcpClient = new TcpClient("localhost", 8080);
            stream = tcpClient.GetStream();
            isConnected = true;
            
            Debug.Log($"Connected to frame sync server (Client: {clientId})");

            // 启动独立线程读取消息
            receiveThread = new Thread(ReceiveMessagesThread);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect: " + e.Message);
        }
    }

    
    
    //每帧处理一个服务端传输来的消息
    void Update()
    {
        // 在主线程处理消息队列
        ServerMessage msg = null;
        lock (queueLock)
        {
            if (messageQueue.Count > 0)
            {
                msg = messageQueue.Dequeue();
            }
        }

        if (msg != null)
        {
            ProcessServerMessage(msg);
        }
    }

    // 处理服务器消息
    void ProcessServerMessage(ServerMessage message)
    {
        LogMessage("[Server]", message);

        if (message.FrameInputs != null)
        {
            currentFrame = message.FrameInputs.FrameNumber;
            OnFrameInputs?.Invoke(message.FrameInputs);
        }
        else if (message.EmptyFrame != null)
        {
            currentFrame = message.EmptyFrame.FrameNumber;
            OnEmptyFrame?.Invoke(message.EmptyFrame);
        }
        else if (message.GameStart != null)
        {
            OnGameStart?.Invoke(message.GameStart);
        }
        else if (message.ConnectSuccess != null)
        {
           OnConnectSuccess?.Invoke(message.ConnectSuccess);
        }
        else if (message.RoomList != null)
        {
            availableRooms = new List<RoomInfo>(message.RoomList.Rooms);
            
            
            
            OnRoomListReceived?.Invoke(availableRooms);
            Debug.Log($"Received {availableRooms.Count} available rooms");
        }
        else if (message.RoomInfo != null)
        {
          
            
            currentRoom = message.RoomInfo;
            if (currentRoom.Status == "error")
            {
                Debug.LogError($"Room error: {message.RoomInfo.RoomName}");
                // 错误处理：恢复按钮状态
                OnRoomInfoUpdate?.Invoke(null);
            }
            else
            {
                // 成功响应：更新房间信息
                currentRoom = message.RoomInfo;
                Debug.Log($"Setting isHost to: {isHost} (my ID: {playerID}, host ID: {currentRoom.HostId})");
                Debug.Log($"Invoking OnRoomInfoUpdate event...");
                OnRoomInfoUpdate?.Invoke(currentRoom);
                Debug.Log($"Room info updated: {currentRoom.RoomName} ({currentRoom.PlayerIds.Count}/{currentRoom.MaxPlayers})");
            }
        }
        else
        {
            Debug.LogWarning("Received unknown message type");
        }
    }

    // 用线程读取消息
    void ReceiveMessagesThread()
    {
        while (isConnected && tcpClient.Connected && !shouldStopThread)
        {
            try
            {
                // 读取消息长度前缀（4字节）
                byte[] lengthBytes = new byte[4];
                int bytesRead = stream.Read(lengthBytes, 0, 4);
                if (bytesRead != 4)
                {
                    Debug.LogError("Failed to read message length");
                    break;
                }

                // 解析消息长度（大端序）
                int messageLength = (lengthBytes[0] << 24) | (lengthBytes[1] << 16) |
                                   (lengthBytes[2] << 8) | lengthBytes[3];

                // 读取消息内容
                byte[] messageBytes = new byte[messageLength];
                bytesRead = 0;
                while (bytesRead < messageLength)
                {
                    int read = stream.Read(messageBytes, bytesRead, messageLength - bytesRead);
                    if (read == 0)
                    {
                        Debug.LogError("Connection closed by server");
                        break;
                    }
                    bytesRead += read;
                }

                if (bytesRead == messageLength)
                {
                    // 反序列化消息
                    ServerMessage message = MessageSerializer.DeserializeServerMessage(messageBytes);
                    if (message != null)
                    {
                        lock (queueLock)
                        {
                            messageQueue.Enqueue(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Receive error: {e.Message}");
                break;
            }
        }

        Debug.Log("Receive thread ended");
    }

    #region ClientRequest

    

    // 请求房间列表
    public void RequestRoomList()
    {
        var msg = new ClientMessage { RoomListRequest = new RoomListRequest() };
        SendMessage(msg);
    }
    // 请求创建房间
    public void CreateRoom(string roomName, int maxPlayers, string playerName, int colorR, int colorG, int colorB)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot create room: not connected");
            return;
        }

        var message = new ClientMessage()
        {
            CreateRoomRequest = new CreateRoomRequest()
            {
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                PlayerName = playerName,
                ColorR = colorR,
                ColorG = colorG,
                ColorB = colorB,
            }
        };
        SendMessage(message);
    }
    
    // 请求加入房间
    public void JoinRoom(string roomId, string playerName, int colorR, int colorG, int colorB)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot join room: not connected");
            return;
        }

        var message = new ClientMessage
        {
            JoinRoomRequest = new JoinRoomRequest
            {
                RoomId = roomId,
                PlayerName = playerName,
                ColorR = colorR,
                ColorG = colorG,
                ColorB = colorB,
            }
        };
        SendMessage(message);
    }
    
    // 请求离开房间
    public void LeaveRoom(string roomId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot leave room: not connected");
            return;
        }

        var message = new ClientMessage
        {
            LeaveRoomRequest = new LeaveRoomRequest
            {
                RoomId = roomId
            }
        };
        SendMessage(message);
    }

    // 请求踢人
    public void KickPlayerRequest(string roomId, string targetPlayerId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot kick player: not connected");
            return;
        }

        var message = new ClientMessage
        {
            KickPlayerRequest = new KickPlayerRequest
            {
                RoomId = roomId,
                TargetPlayerId = targetPlayerId
            }
        };
        SendMessage(message);
    }
    
    // 发送玩家输入
    public void SendPlayerInput(InputType inputType)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot send input: not connected");
            return;
        }

        var message = new ClientMessage
        {
            PlayerInput = new PlayerInput
            {
                PlayerId = playerID,
                InputType = inputType,
                FrameNumber = currentFrame,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };
        SendMessage(message);
    }
    // 请求开始游戏

    public void GameStartRequest(string roomId, int level = 0)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot send input: not connected");
            return;
        }

        var message = new ClientMessage { GameStartRequest = new GameStartRequest()
        {
            RoomId = roomId,
            Level = level
        }};
        SendMessage(message);
    }
    
    #endregion

    // 发送消息到服务器
    public void SendMessage(ClientMessage message)
    {
        if (!isConnected || stream == null)
        {
            Debug.LogWarning("Cannot send message: not connected");
            return;
        }

        try
        {
            byte[] data = MessageSerializer.SerializeClientMessage(message);
            if (data != null)
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
                LogMessage("[Client]", message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Send error: {e.Message}");
        }
    }
    
    // 打印消息内容（调试用）
    private void LogMessage(string prefix, object message)
    {
        if (message != null)
        {
            Debug.Log($"{prefix}: {message}");
        }
    }
    
    public void Disconnect()
    {
        shouldStopThread = true;
        isConnected = false;
        stream?.Close();
        tcpClient?.Close();
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(1000); // 等待线程结束，最多1秒
        }
    }
    
    void SetPlayerID(ConnectSuccess connectSuccess)
    {
        playerID = connectSuccess.YourPlayerId;
    }

    void OnDestroy()
    {
        OnConnectSuccess -= SetPlayerID;
        shouldStopThread = true;
        isConnected = false;
        stream?.Close();
        tcpClient?.Close();
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(1000); // 等待线程结束，最多1秒
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
