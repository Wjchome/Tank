using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tankgame;
using System.Threading.Tasks;

public class NetworkManager : SingletonMono<NetworkManager>
{
    private TcpClient tcpClient;//代表自己的连接
    private NetworkStream stream;//网络流
    private bool isConnected = false;
    
    public string playerID;//连接成功就赋值
    public long currentFrame = 0;//收到输入和空帧就赋值
    public RoomInfo currentRoom;//收到房间信息就赋值
    public bool isHost =>currentRoom?.HostId == playerID;
    public bool isGameing = false;

    public long seed;
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

    public string serverIP = "192.168.1.100";
    public int serverPort = 8080;
    private void Start()
    {
        ConnectToServer();
        
        
    }
    
    
    private async void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(serverIP, serverPort);
            float timeout = 5f; // 5秒超时
            if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(timeout))) == connectTask)
            {
                // 连接成功
                stream = tcpClient.GetStream();
                isConnected = true;
                Debug.Log($"Connected to frame sync server ({serverIP}:{serverPort})");
                // 启动独立线程读取消息
                receiveThread = new Thread(ReceiveMessagesThread);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            else
            {
                Debug.LogError($"Connect timeout! ({serverIP}:{serverPort})");
                // 这里可以弹窗提示用户
            }
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
            seed = message.GameStart.RandomSeed;
            isGameing = true;
            currentFrame = 0;
            OnGameStart?.Invoke(message.GameStart);
        }
        else if (message.ConnectSuccess != null)
        {
             playerID = message.ConnectSuccess.YourPlayerId;
            
           OnConnectSuccess?.Invoke(message.ConnectSuccess);
        }
        else if (message.RoomList != null)
        {
            OnRoomListReceived?.Invoke(message.RoomList.Rooms.ToList());
        }
        else if (message.RoomInfo != null)
        {
          
            
            currentRoom = message.RoomInfo;
         
            OnRoomInfoUpdate?.Invoke(currentRoom);
            
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

    void OnDestroy()
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

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
