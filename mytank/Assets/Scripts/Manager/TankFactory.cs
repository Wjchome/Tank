using UnityEngine;
using Tankgame;

public class TankFactory : SingletonMono<TankFactory>
{
    [Header("坦克预制体")]
    public GameObject tankPrefab;
    
    [Header("对象池配置")]
    public int tankPoolSize = 20;
    
    private const string TANK_POOL_TAG = "Tank";
    
    protected override void Awake()
    {
        base.Awake();
        InitializeTankPool();
    }
    
    private void InitializeTankPool()
    {
        // 确保ObjectPoolManager已经初始化
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager not found!");
            return;
        }
        
        // 配置坦克池
        ObjectPoolManager.PoolInfo tankPoolInfo = new ObjectPoolManager.PoolInfo
        {
            tag = TANK_POOL_TAG,
            prefab = tankPrefab,
            size = tankPoolSize
        };
        
        // 添加到ObjectPoolManager的配置中
        ObjectPoolManager.Instance.poolInfos = new ObjectPoolManager.PoolInfo[] { tankPoolInfo };
    }
    
    /// <summary>
    /// 创建玩家坦克
    /// </summary>
    public TankController CreatePlayerTank(string playerID, string playerName, int x, int y, Color color, int dataIndex)
    {
        GameObject tankObj = ObjectPoolManager.Instance.SpawnFromPool(TANK_POOL_TAG, Vector3.zero, Quaternion.identity);
        if (tankObj == null)
        {
            Debug.LogError("Failed to spawn tank from pool!");
            return null;
        }
        
        TankController tankController = tankObj.GetComponent<TankController>();
        if (tankController == null)
        {
            Debug.LogError("TankController component not found on tank prefab!");
            return null;
        }
        
        // 初始化坦克
        tankController.Initialize(playerID, playerName, x, y, color, true, dataIndex);
        
        return tankController;
    }
    
    /// <summary>
    /// 创建AI坦克
    /// </summary>
    public TankController CreateAITank(string playerID, int x, int y, int dataIndex)
    {
        GameObject tankObj = ObjectPoolManager.Instance.SpawnFromPool(TANK_POOL_TAG, Vector3.zero, Quaternion.identity);
        if (tankObj == null)
        {
            Debug.LogError("Failed to spawn AI tank from pool!");
            return null;
        }
        
        TankController tankController = tankObj.GetComponent<TankController>();
        if (tankController == null)
        {
            Debug.LogError("TankController component not found on tank prefab!");
            return null;
        }
        
        // 为AI坦克生成随机颜色
        Color aiColor = GetRandomAIColor();
        string aiName = $"AI_{playerID}";
        
        // 初始化AI坦克
        tankController.Initialize(playerID, aiName, x, y, aiColor, false, dataIndex);
        
        return tankController;
    }
    
    /// <summary>
    /// 回收坦克到对象池
    /// </summary>
    public void RecycleTank(TankController tank)
    {
        if (tank == null) return;
        
        // 清理坦克状态
        CleanupTank(tank);
        
        // 回收到对象池
        ObjectPoolManager.Instance.ReturnToPool(tank.gameObject);
    }
    
    /// <summary>
    /// 延迟回收坦克（用于死亡动画等）
    /// </summary>
    public void RecycleTankDelayed(TankController tank, float delay)
    {
        if (tank == null) return;
        
        // 清理坦克状态
        CleanupTank(tank);
        
        // 延迟回收到对象池
        ObjectPoolManager.Instance.ReturnToPool(tank.gameObject, delay);
    }
    
    /// <summary>
    /// 清理坦克状态
    /// </summary>
    private void CleanupTank(TankController tank)
    {
        // 从GameStateManager中移除
        if (GameStateManager.Instance != null && GameStateManager.Instance.playerTanks.ContainsKey(tank.PlayerID))
        {
            GameStateManager.Instance.playerTanks.Remove(tank.PlayerID);
        }
        
        // 清理UI
        if (tank.playerPanelUI != null)
        {
            Destroy(tank.playerPanelUI.gameObject);
            tank.playerPanelUI = null;
        }
        
        // 停止所有动画
        tank.transform.DOKill();
        
        // 重置坦克状态
        tank.isDead = false;
        tank.killNum = 0;
        tank.isMoving = false;
        tank.lastMoveTime = 0;
        tank.lastShootTime = 0;
        tank.lastAnimStartTime = 0;
    }
    
    /// <summary>
    /// 获取随机AI颜色
    /// </summary>
    private Color GetRandomAIColor()
    {
        Color[] aiColors = new Color[]
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f), // 橙色
            new Color(0.5f, 0f, 1f), // 紫色
        };
        
        return aiColors[Random.Range(0, aiColors.Length)];
    }
    
    /// <summary>
    /// 批量创建坦克（用于游戏开始）
    /// </summary>
    public void CreateTanksForGame(GameStart gameStartData)
    {
        // 创建玩家坦克
        foreach (var playerInfo in gameStartData.Players)
        {
            TankController tank = CreatePlayerTank(
                playerInfo.PlayerId,
                playerInfo.PlayerName,
                playerInfo.X,
                playerInfo.Y,
                new Color(playerInfo.ColorR, playerInfo.ColorG, playerInfo.ColorB),
                playerInfo.DataIndex
            );
        }
        
        // 创建AI坦克（如果有的话）
        if (gameStartData.Enemies != null)
        {
            foreach (var enemyInfo in gameStartData.Enemies)
            {
                TankController aiTank = CreateAITank(
                    enemyInfo.PlayerId,
                    enemyInfo.X,
                    enemyInfo.Y,
                    enemyInfo.DataIndex
                );
            }
        }
    }
    
    /// <summary>
    /// 清理所有坦克
    /// </summary>
    public void ClearAllTanks()
    {
        if (GameStateManager.Instance != null)
        {
            var tanks = new System.Collections.Generic.List<TankController>(GameStateManager.Instance.playerTanks.Values);
            foreach (var tank in tanks)
            {
                RecycleTank(tank);
            }
        }
    }
} 