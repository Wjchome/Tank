using System;
using System.Collections.Generic;
using Tankgame;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public enum GameMode
{
    opponent,
    friend
}

public class GameManager : SingletonMono<GameManager>
{
    [Header("预制体引用")]
    public GameObject bulletPrefab;
    public GameObject tankPrefab;
    
    [Header("工厂管理器")]
    public FactoryManager factoryManager;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeGameManager();
    }
    
    private void InitializeGameManager()
    {
        // 确保工厂管理器已初始化
        if (factoryManager == null)
            factoryManager = FindObjectOfType<FactoryManager>();
            
        // 设置预制体引用
        if (factoryManager != null && factoryManager.tankFactory != null)
            factoryManager.tankFactory.tankPrefab = tankPrefab;
            
        if (factoryManager != null && factoryManager.bulletFactory != null)
            factoryManager.bulletFactory.bulletPrefab = bulletPrefab;
    }
    
    /// <summary>
    /// 游戏开始时的初始化
    /// </summary>
    public void StartGame(GameStart gameStartData)
    {
        Debug.Log("GameManager: Starting game...");
        
        // 使用工厂管理器初始化游戏
        factoryManager.InitializeGame(gameStartData);
        
        // 其他游戏初始化逻辑...
    }
    
    /// <summary>
    /// 游戏结束时的清理
    /// </summary>
    public void EndGame()
    {
        Debug.Log("GameManager: Ending game...");
        
        // 清理所有游戏对象
        factoryManager.ClearAllObjects();
        
        // 其他游戏结束逻辑...
    }
    
    /// <summary>
    /// 获取游戏统计信息
    /// </summary>
    public GameStats GetGameStats()
    {
        return factoryManager.GetGameStats();
    }
    
    /// <summary>
    /// 打印调试信息
    /// </summary>
    [ContextMenu("Log Game Stats")]
    public void LogGameStats()
    {
        factoryManager.LogPoolStatus();
    }
}
