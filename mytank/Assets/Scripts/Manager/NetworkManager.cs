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
    private TcpClient tcpClient;
    private NetworkStream stream;
    private bool isConnected = false;

    public string playerID;
    public long currentFrame = 0;

    // 事件定义
    public event Action<FrameInputs> OnFrameInputs;
    public event Action<GameStart> OnGameStart;
    
    private Queue<ServerMessage> messageQueue = new Queue<ServerMessage>();
    private object queueLock = new object();
    private Thread receiveThread;
    private bool shouldStopThread = false;

    void Start()
    {
        //NetworkManager.Instance.OnFrameInputs += OnFrameInputs;
        NetworkManager.Instance.OnGameStart += OnGameStartFun;
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient("localhost", 8080);
            stream = tcpClient.GetStream();
            isConnected = true;

            Debug.Log("Connected to frame sync server");

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
        MessageSerializer.LogMessage("[Server]", message);

        if (message.FrameInputs != null)
        {
            currentFrame = message.FrameInputs.FrameNumber;
            OnFrameInputs?.Invoke(message.FrameInputs);
        }
        else if (message.GameStart != null)
        {
            OnGameStart?.Invoke(message.GameStart);
        }
        else if (message.ConnectSuccess!= null) {
            playerID= message.ConnectSuccess.YourPlayerId;
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

    // 发送玩家输入
    public void SendPlayerInput(InputType inputType)
    {
       
        if (!isConnected)
        {
            Debug.LogWarning("Cannot send input: not connected");
            return;
        }

        var message = MessageSerializer.CreatePlayerInputMessage(playerID, inputType, currentFrame);
        SendMessage(message);
    }

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
                MessageSerializer.LogMessage("[Client]", message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Send error: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnGameStart -= OnGameStart;
        }
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

    void OnGameStartFun(GameStart gameStart)
    {
        Debug.Log($"Game started! Room: {gameStart.RoomId}, Players: {string.Join(",", gameStart.PlayerIds)}");
        foreach (var playerId in gameStart.PlayerIds)
        {
            if (!GameStateManager.Instance. playerTanks.ContainsKey(playerId))
            {
                GameStateManager.Instance.CreatePlayerTank(playerId);
                
            }
        }
    }
}
