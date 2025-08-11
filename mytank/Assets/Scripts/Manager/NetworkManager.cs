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
using Unity.VisualScripting;
using System.Net;
using System.Text;
using Newtonsoft.Json;

// 服务器信息结构
[System.Serializable]
public class ServerInfo
{
    public string serverIP;
    public int gamePort;
    public string serverName;
    public string version;
}

public class NetworkManager : SingletonMono<NetworkManager>
{
    private TcpClient tcpClient; //代表自己的连接
    private NetworkStream stream; //网络流
    private bool isConnected = false;

    public string playerID; //连接成功就赋值
    public long currentFrame = 0; //收到输入和空帧就赋值
    public RoomInfo currentRoom; //收到房间信息就赋值
    public bool isHost => currentRoom?.HostId == playerID;
    public bool isGameing = false;

    public long seed;


    private Queue<ServerMessage> messageQueue = new Queue<ServerMessage>();
    private object queueLock = new object();
    private Thread receiveThread;
    private bool shouldStopThread = false;

    // 新增字段
    private UdpClient discoveryClient;
    private Thread discoveryThread;
    private bool shouldStopDiscovery = false;
    private ServerInfo discoveredServer;
    private bool serverDiscovered = false;
    private bool serverInfoUpdated = false; // 新增：标记服务器信息是否更新

    // 默认连接信息（如果没发现服务器时使用）
    public string serverIP = "127.0.0.1";
    public int serverPort = 8080;

    public PlayerTankController myTank;

    private void Start()
    {
        Application.targetFrameRate = 60;
      //  StartDiscovery();
        // 延迟连接，等待发现服务器
        StartCoroutine(DelayedConnect());
        
        style.fontSize = 30;
        style.normal.textColor = Color.white;
    }

    // 新增：延迟连接
    private IEnumerator DelayedConnect()
    {
        yield return new WaitForSeconds(2f); // 等待2秒发现服务器

        if (serverDiscovered && discoveredServer != null)
        {
            serverIP = discoveredServer.serverIP;
            serverPort = discoveredServer.gamePort;
            Debug.Log($"Discovered server: {serverIP}:{serverPort}");
        }
        else
        {
            Debug.Log("No server discovered, using default IP");
        }

        ConnectToServer();
    }

    // 新增：启动服务器发现
    private void StartDiscovery()
    {
        try
        {
            discoveryClient = new UdpClient(9999);
            discoveryThread = new Thread(DiscoveryThread);
            discoveryThread.IsBackground = true;
            discoveryThread.Start();
            Debug.Log("Started server discovery on port 9999");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start discovery: {e.Message}");
        }
    }

    // 新增：发现线程
    private void DiscoveryThread()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 9999);
        while (!shouldStopDiscovery)
        {
            try
            {
                byte[] data = discoveryClient.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data);

                ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(json);

                // 直接更新变量，在Update中处理
                lock (queueLock)
                {
                    discoveredServer = serverInfo;
                    serverDiscovered = true;
                    serverInfoUpdated = true;
                }

                // 找到服务器后停止UDP监听
                Debug.Log("Server found, stopping UDP discovery");
                shouldStopDiscovery = true;
                break;
            }
            catch (Exception e)
            {
                if (!shouldStopDiscovery)
                {
                    Debug.LogError($"Discovery error: {e.Message}");
                }
            }
        }
    }


    private async void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient();

            // 禁用Nagle算法，减少网络延迟
            tcpClient.NoDelay = true;
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

    private float fps;
    private GUIStyle style = new GUIStyle();
    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 200, 50), $"FPS: {fps.ToString("F1")}", style);
    }
    void Update()
    {
        fps = 1.0f / Time.deltaTime;
        // 处理服务器发现结果
        if (serverInfoUpdated)
        {
            lock (queueLock)
            {
                serverInfoUpdated = false;
                if (discoveredServer != null)
                {
                    Debug.Log(
                        $"Server discovered: {discoveredServer.serverName} at {discoveredServer.serverIP}:{discoveredServer.gamePort}");
                }
            }
        }

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
        if (message.FrameMessage != null)
        {
            currentFrame = message.FrameMessage.FrameNumber;
            GameStateManager.Instance.OnFrameInputs(message.FrameMessage.Inputs.ToList());
            GameStateManager.Instance.OnFoodsRequest(message.FrameMessage.ChooseFoodRequests.ToList());
            EnemyManager.Instance.UpdateFrame(); //生成敌人
            FoodManager.Instance.UpdateFrame(); //选择道具
            PlayerManager.Instance.UpdateFrame(); //
            EntityManager.Instance.UpdateFrame(); //实体推进
            MapManager.Instance.UpdateFrame(); //有关道具
        }
        else if (message.GameStart != null)
        {
            seed = message.GameStart.RandomSeed;
            isGameing = true;
            currentFrame = 0;


            LevelManager.Instance.GameStart(message.GameStart.Level);
            MapManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
            EnemyManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
            PlayerManager.Instance.OnGameStart(message.GameStart.PlayerInfos.ToList());
            CamController.Instance.Change(MapManager.Instance.mapWidth / 2);
            
            RoomManager.Instance.OnGameStartRoom();//关闭UI
            GameUIManager.Instance.GameStart();//复原打开游戏UI
            FoodManager.Instance.GameStart();//重新设置食物啥的
        }
        else if (message.ConnectSuccess != null)
        {
            playerID = message.ConnectSuccess.YourPlayerId;
        }
        else if (message.RoomList != null)
        {
            currentRoom = null;
            RoomManager.Instance.OnRoomListReceived(message.RoomList.Rooms.ToList());
        }
        else if (message.RoomInfo != null)
        {
            if (isGameing)
            {
                GameStateManager.Instance.GameOver(false);
            }
            else
            {
                currentRoom = message.RoomInfo;

                RoomManager.Instance.OnRoomInfoUpdate(message.RoomInfo);
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
    public void CreateRoom(string roomName, int maxPlayers, string playerName, int colorR, int colorG, int colorB,
        PlayerRole playerRole)
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
                PlayerRole = (int)playerRole
            }
        };
        SendMessage(message);
    }

    // 请求加入房间
    public void JoinRoom(string roomId, string playerName, int colorR, int colorG, int colorB, PlayerRole playerRole)
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
                PlayerRole = (int)playerRole
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

        var message = new ClientMessage
        {
            GameStartRequest = new GameStartRequest()
            {
                RoomId = roomId,
                Level = level
            }
        };
        SendMessage(message);
    }


    public void GameOverRequest(string roomId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot send input: not connected");
            return;
        }

        var message = new ClientMessage
        {
            GameOverRequest = new GameOverRequest()
            {
                RoomId = roomId,
            }
        };
        SendMessage(message);
    }


    public void FoodChooseRequest(FoodType foodType)
    {
        var message = new ClientMessage
        {
            ChooseFoodRequest = new ChooseFoodRequest()
            {
                PlayerId = playerID,
                FoodId = (int)foodType
            }
        };
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
                //     LogMessage("[Client]", message);
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
        // 停止发现线程
        shouldStopDiscovery = true;
        discoveryClient?.Close();
        if (discoveryThread != null && discoveryThread.IsAlive)
        {
            discoveryThread.Join(1000);
        }

        // 原有的清理代码
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