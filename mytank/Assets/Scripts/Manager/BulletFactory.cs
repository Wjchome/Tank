using UnityEngine;
using Tankgame;
using System.Collections.Generic;
using DG.Tweening;

public class BulletFactory : SingletonMono<BulletFactory>
{
    [Header("子弹预制体")]
    public GameObject bulletPrefab;
    
    [Header("对象池配置")]
    public int bulletPoolSize = 10;
    
    private const string BULLET_POOL_TAG = "Bullet";
    
    // 活跃子弹列表
    private List<BulletController> activeBullets = new List<BulletController>();
    
    protected  void Awake()
    {
        InitializeBulletPool();
    }
    
    private void InitializeBulletPool()
    {
        
        
        // 配置子弹池
        ObjectPoolManager.PoolInfo bulletPoolInfo = new ObjectPoolManager.PoolInfo
        {
            tag = BULLET_POOL_TAG,
            prefab = bulletPrefab,
            size = bulletPoolSize
        };
        
        // 添加到ObjectPoolManager的配置中
        ObjectPoolManager.Instance.poolInfos = new ObjectPoolManager.PoolInfo[] { bulletPoolInfo };
    }
    
    /// <summary>
    /// 创建子弹
    /// </summary>
    public BulletController CreateBullet( Direction direction, string ownerID, bool isPlayerBullet)
    {
        GameObject bulletObj = ObjectPoolManager.Instance.SpawnFromPool(BULLET_POOL_TAG, Vector3.zero, Quaternion.identity);
        if (bulletObj == null)
        {
            Debug.LogError("Failed to spawn bullet from pool!");
            return null;
        }
        
        BulletController bulletController = bulletObj.GetComponent<BulletController>();
        if (bulletController == null)
        {
            Debug.LogError("BulletController component not found on bullet prefab!");
            return null;
        }
        
        // 初始化子弹
        bulletController.Initialize("", direction, ownerID, isPlayerBullet);
        
        // 添加到活跃子弹列表
        activeBullets.Add(bulletController);
        
        return bulletController;
    }
    
    /// <summary>
    /// 回收子弹到对象池
    /// </summary>
    public void RecycleBullet(BulletController bullet)
    {
        if (bullet == null) return;
        
        // 从活跃子弹列表中移除
        activeBullets.Remove(bullet);
        
        // 清理子弹状态
        CleanupBullet(bullet);
        
        // 回收到对象池
        ObjectPoolManager.Instance.ReturnToPool(bullet.gameObject);
    }
    
    /// <summary>
    /// 延迟回收子弹（用于爆炸动画等）
    /// </summary>
    public void RecycleBulletDelayed(BulletController bullet, float delay)
    {
        if (bullet == null) return;
        
        // 从活跃子弹列表中移除
        activeBullets.Remove(bullet);
        
        // 清理子弹状态
        CleanupBullet(bullet);
        
        // 延迟回收到对象池
        ObjectPoolManager.Instance.ReturnToPool(bullet.gameObject, delay);
    }
    
    /// <summary>
    /// 清理子弹状态
    /// </summary>
    private void CleanupBullet(BulletController bullet)
    {
        // 停止所有动画
        bullet.transform.DOKill();
        
        // 重置子弹状态
        bullet.isShouldDestroy = false;
        bullet.lastMoveTime = 0;
        bullet.Pos = Vector2Int.zero;
    }
    
 
    
    /// <summary>
    /// 清理所有子弹
    /// </summary>
    public void ClearAllBullets()
    {
        var bullets = new List<BulletController>(activeBullets);
        foreach (var bullet in bullets)
        {
            RecycleBullet(bullet);
        }
        activeBullets.Clear();
    }
    
    /// <summary>
    /// 根据所有者ID获取子弹
    /// </summary>
    public List<BulletController> GetBulletsByOwner(string ownerID)
    {
        List<BulletController> ownerBullets = new List<BulletController>();
        foreach (var bullet in activeBullets)
        {
            if (bullet.OwnerID == ownerID)
            {
                ownerBullets.Add(bullet);
            }
        }
        return ownerBullets;
    }
    
    /// <summary>
    /// 根据位置获取子弹
    /// </summary>
    public List<BulletController> GetBulletsAtPosition(Vector2Int position)
    {
        List<BulletController> bulletsAtPos = new List<BulletController>();
        foreach (var bullet in activeBullets)
        {
            if (bullet.Pos == position)
            {
                bulletsAtPos.Add(bullet);
            }
        }
        return bulletsAtPos;
    }
    
   
    
  
    
    /// <summary>
    /// 根据ID查找子弹
    /// </summary>
    private BulletController FindBulletById(string bulletId)
    {
        foreach (var bullet in activeBullets)
        {
            if (bullet.BulletID == bulletId)
            {
                return bullet;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取活跃子弹数量
    /// </summary>
    public int GetActiveBulletCount()
    {
        return activeBullets.Count;
    }
    
    /// <summary>
    /// 检查子弹池状态
    /// </summary>
    public void LogPoolStatus()
    {
        int poolSize = ObjectPoolManager.Instance.GetPoolSize(BULLET_POOL_TAG);
        Debug.Log($"Bullet Pool Status - Active: {activeBullets.Count}, Pool: {poolSize}");
    }
} 