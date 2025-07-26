using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletFactory : SingletonMono<BulletFactory>
{
    public BulletController bulletPrefab; // 改为 TankController 类型
    public ObjectPool<BulletController> BulletPool { get; private set; }

    public List<BulletController> activeBullets;

    private void Awake()
    {
        BulletPool = new ObjectPool<BulletController>(
            prefab: bulletPrefab,
            onSpawn: CreateBullet,
            onDespawn: KillBullet
        );
        activeBullets = new List<BulletController>();
    }

    private void CreateBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.isShouldDestroy=false;
        bullet.lastMoveTime = 0f;
        bullet.animator.Play("Idle",0,0);
        activeBullets.Add(bullet);
        
    }

    private void KillBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(false);
        activeBullets.Remove(bullet);
        
    }

        
    public void Initialize(Direction direction, TankController tank,Vector2Int pos )
    {
        BulletController bullet = BulletPool.GetObject();
        bullet.direction = direction;
        bullet.ownerTank=tank;
        if (tank.identity == Identity.Myself || tank.identity == Identity.OtherPlayer)
            bullet.isPlayerBullet = true;
        else
        {
            bullet.isPlayerBullet = false;
        }
        // 设置子弹朝向
        Vector3 rotation = Vector3.zero;
        bullet.dir = direction.ToVector2Int();
        bullet.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
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
        
     
       
        bullet.Pos = pos;
            
            // 设置子弹中心位置
            bullet.transform.position = bullet.GetCenter();
     bullet.isCanBreakWall=tank.isBreakWall;
    }
    
    public void ClearAllBullets()
    {
        // 创建副本，避免遍历时修改集合
        var bullets = new List<BulletController>(activeBullets);
        foreach (var bullet in bullets)
        {
            BulletPool.ReturnObject(bullet);
        }
     
    }
  
}