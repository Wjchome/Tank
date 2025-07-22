using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletFactory : SingletonMono<BulletFactory>
{
    public BulletController bulletPrefab; // 改为 TankController 类型
    public ObjectPool<BulletController> BulletPool { get; private set; }

    public List<BulletController> allBullets { get; private set; }

    private void Awake()
    {
        BulletPool = new ObjectPool<BulletController>(
            prefab: bulletPrefab,
            onSpawn: CreateBullet,
            onDespawn: KillBullet
        );
        allBullets = new List<BulletController>();
    }

    private void CreateBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.isShouldDestroy=false;
        bullet.lastMoveTime = 0f;
        bullet.animator.Play("Idle",0,0);
        allBullets.Add(bullet);
        
    }

    private void KillBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(false);
        allBullets.Remove(bullet);
        
    }

        
    public void Initialize(Direction direction, string ownerID, bool isPlayerBullet )
    {
        BulletController bullet = BulletPool.GetObject();
        bullet.direction = direction;
        bullet.ownerID = ownerID;
        bullet.isPlayerBullet=isPlayerBullet;
        // 设置子弹朝向
        Vector3 rotation = Vector3.zero;
        bullet.dir = direction.ToVector2Int();  
        
        switch (direction)
        {
            case Direction.Up: 
                rotation = new Vector3(0, 0, 0);
                break;
            case Direction.Down: 
                rotation = new Vector3(0, 0, 180);  
                break;
            case Direction.Left: 
                rotation = new Vector3(0, 0, 90);  
                break;
            case Direction.Right: 
                rotation = new Vector3(0, 0, -90);  
                break;
        }
        bullet.transform.rotation = Quaternion.Euler(rotation);
        
        // 根据坦克位置计算子弹初始位置
        TankController ownerTank = GameStateManager.Instance.allTanks[ownerID];
       
        bullet.Pos = ownerTank.Pos;
            
            // 设置子弹中心位置
            bullet.transform.position = bullet.GetCenter();
     
    }
    
    public void ClearAllBullets()
    {
        // 创建副本，避免遍历时修改集合
        var bullets = new List<BulletController>(allBullets);
        foreach (var bullet in bullets)
        {
            BulletPool.ReturnObject(bullet);
        }
        allBullets.Clear();
    }
  
}