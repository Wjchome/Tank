using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading;

public class NetworkManager : SingletonMono<NetworkManager>
{

    
    private TcpClient tcpClient;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    private bool isConnected = false;
    
    public string playerID;
    public event Action<string, object> OnMessageReceived;
    
    private Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();
    private object queueLock = new object();
    private Thread receiveThread;

    
    void Start()
    {
        ConnectToServer();
    }
    
    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient("localhost", 8080);
            stream = tcpClient.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
            isConnected = true;
            
            Debug.Log("Connected to server");
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
        // 每帧只处理一条消息（如需每帧处理多条可用 while）
        NetworkMessage msg = null;
        lock (queueLock)
        {
            if (messageQueue.Count > 0)
            {
                msg = messageQueue.Dequeue();
            }
        }
        if (msg != null)
        {
            OnMessageReceived?.Invoke(msg.type, msg.data);
        }
    }

    // 用线程读取消息
    void ReceiveMessagesThread()
    {
        while (isConnected && tcpClient.Connected)
        {
            string line = null;
            try
            {
                line = reader.ReadLine();
            }
            catch (Exception e)
            {
                Debug.LogError("Receive error: " + e.Message);
                break;
            }

            if (!string.IsNullOrEmpty(line))
            {
                Debug.Log("[Receive] " + line);
                //MessageManager.Instance.SetReceiveTxt("Receive:\n" + line);
                NetworkMessage message = null;
                try
                {
                    message = JsonConvert.DeserializeObject<NetworkMessage>(line);
                }
                catch (Exception e)
                {
                    Debug.LogError("Json parse error: " + e.Message);
                }
                if (message != null)
                {
                    lock (queueLock)
                    {
                        messageQueue.Enqueue(message);
                    }
                }
            }
        }
    }
    public void SendMessage(string type, object data)
    {
        if (!isConnected || writer == null) return;
        
        try
        {
            var message = new NetworkMessage { type = type, data = data };
            string json = JsonConvert.SerializeObject(message);
            
            writer.WriteLine(json); // 使用WriteLine自动添加换行符
            Debug.Log("Sending: " + json);
            
            MessageManager.Instance.SetSendTxt("Sending: \n"+json);
            
        }
        catch (Exception e)
        {
            Debug.LogError("Send error: " + e.Message);
        }
    }
    
    void OnDestroy()
    {
        isConnected = false;
        
        reader?.Close();
        writer?.Close();
        stream?.Close();
        tcpClient?.Close();
    }
}

[System.Serializable]
public class NetworkMessage
{
    public string type;
    public object data;
}
