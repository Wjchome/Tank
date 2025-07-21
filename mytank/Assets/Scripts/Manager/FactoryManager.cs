using UnityEngine;
using Tankgame;

public class FactoryManager : SingletonMono<FactoryManager>
{
    [Header("工厂引用")]
    public TankFactory tankFactory;
    public BulletFactory bulletFactory;
    public ObjectPoolManager objectPoolManager;
    
 
    
    /// <summary>
    /// 游戏开始时创建所有对象
    /// </summary>
    public void InitializeGame(GameStart gameStartData)
    {
        // 清理之前的对象
        ClearAllObjects();
        
        // 创建坦克
        tankFactory.CreateTanksForGame(gameStartData);
        
    }
    
    /// <summary>
    /// 清理所有游戏对象
    /// </summary>
    public void ClearAllObjects()
    {
        tankFactory.ClearAllTanks();
        bulletFactory.ClearAllBullets();
        
    }
    
    /// <summary>
    /// 获取游戏状态统计
    /// </summary>
    public GameStats GetGameStats()
    {
        return new GameStats
        {
            activeTanks = GameStateManager.Instance?.playerTanks?.Count ?? 0,
            activeBullets = bulletFactory.GetActiveBulletCount(),
            tankPoolSize = objectPoolManager.GetPoolSize("Tank"),
            bulletPoolSize = objectPoolManager.GetPoolSize("Bullet")
        };
    }
    
    /// <summary>
    /// 打印池状态
    /// </summary>
    public void LogPoolStatus()
    {
        var stats = GetGameStats();
        Debug.Log($"Game Stats - Tanks: {stats.activeTanks}, Bullets: {stats.activeBullets}");
        Debug.Log($"Pool Status - Tank Pool: {stats.tankPoolSize}, Bullet Pool: {stats.bulletPoolSize}");
    }
    
    /// <summary>
    /// 预加载对象池（可选，用于优化）
    /// </summary>
    public void PreloadPools()
    {
        // 这里可以添加预加载逻辑
        Debug.Log("Pools preloaded");
    }
}

// 游戏状态统计
[System.Serializable]
public class GameStats
{
    public int activeTanks;
    public int activeBullets;
    public int tankPoolSize;
    public int bulletPoolSize;
} 