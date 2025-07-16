using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class NetworkManager : SingletonMono<NetworkManager>
{

    
    private TcpClient tcpClient;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    private bool isConnected = false;
    
    public string playerID;
    public event Action<string, object> OnMessageReceived;
    

    
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
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            isConnected = true;
            
            Debug.Log("Connected to server");
            StartCoroutine(ReceiveMessages());
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect: " + e.Message);
        }
    }
    
    IEnumerator ReceiveMessages()
    {
        byte[] buffer = new byte[4096];
        
        while (isConnected && tcpClient.Connected)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        Debug.Log("Received: " + line);
                        
                        var message = JsonConvert.DeserializeObject<NetworkMessage>(line);
                        OnMessageReceived?.Invoke(message.type, message.data);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Receive error: " + e.Message);
                break;
            }
            
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    public void SendMessage(string type, object data)
    {
        if (!isConnected || writer == null) return;
        
        try
        {
            var message = new NetworkMessage { type = type, data = data };
            string json = JsonConvert.SerializeObject(message);
            
            Debug.Log("Sending: " + json);
            writer.WriteLine(json); // 使用WriteLine自动添加换行符
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
