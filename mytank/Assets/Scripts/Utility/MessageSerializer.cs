using System;
using System.IO;
using UnityEngine;
using Tankgame;
using Google.Protobuf;

public static class MessageSerializer
{
    // 序列化客户端消息为protobuf
    public static byte[] SerializeClientMessage(ClientMessage message)
    {
        try
        {
            byte[] protobufData = message.ToByteArray();
            
            // 添加长度前缀（4字节，大端序）
            byte[] lengthBytes = new byte[4];
            int length = protobufData.Length;
            lengthBytes[0] = (byte)((length >> 24) & 0xFF);
            lengthBytes[1] = (byte)((length >> 16) & 0xFF);
            lengthBytes[2] = (byte)((length >> 8) & 0xFF);
            lengthBytes[3] = (byte)(length & 0xFF);
            
            byte[] result = new byte[4 + protobufData.Length];
            Array.Copy(lengthBytes, 0, result, 0, 4);
            Array.Copy(protobufData, 0, result, 4, protobufData.Length);
            
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to serialize client message: {e.Message}");
            return null;
        }
    }

    // 反序列化服务器消息
    public static ServerMessage DeserializeServerMessage(byte[] data)
    {
        try
        {
            return ServerMessage.Parser.ParseFrom(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to deserialize server message: {e.Message}");
            return null;
        }
    }
}
