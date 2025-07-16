using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletController : MonoBehaviour
{
    public string BulletID { get; private set; }
    public string Direction { get; private set; }
    public string OwnerID { get; private set; }
    
    private bool isLocal; // 是否由本地客户端创建
    private float moveSpeed = 5f; // 子弹移动速度
    private float moveInterval = 0.2f; // 子弹移动间隔
    private float lastMoveTime;
    
    public Vector2Int Pos=>new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    public void Initialize(string id, string direction, string ownerID, bool local = false)
    {
        BulletID = id;
        Direction = direction;
        OwnerID = ownerID;
        isLocal = local;
        
        lastMoveTime = Time.time;
        StartCoroutine(MoveRoutine());
    }
    
    IEnumerator MoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);
            
            
            int currentX = Pos.x;
            int currentY = Pos.y;
            
            int nextX = currentX;
            int nextY = currentY;
            
            switch (Direction)
            {
                case "up":
                    nextY += 1;
                    break;
                case "down":
                    nextY -= 1;
                    break;
                case "left":
                    nextX -= 1;
                    break;
                case "right":
                    nextX += 1;
                    break;
            }
            
           
            
            bool shouldDestroy = false;
            int nextWallType = MapManager.Instance.GetWallType(nextX, nextY);
            bool isHit = false;
            TankController hitTank = null;
            foreach (var kv in GameManager.Instance.tanks)
            {
                var tank = kv.Value;
                if (tank.PlayerID != OwnerID)
                {
                    if (tank.Pos.x == nextX && tank.Pos.y == nextY)
                    {
                        isHit = true;
                        hitTank = tank;
                        break;
                    }
                }
            }

            if (isHit)
            {
                // 那个坦克受伤
                if (hitTank != null && isLocal)
                {
                    HandleTankHit(hitTank);
                }
                shouldDestroy = true;
            }
            else if (nextWallType == 0)
            {
                //移动
                transform.position = new Vector2(nextX, nextY);
            }
            else if (nextWallType == 1)
            {
                //销毁自己
                shouldDestroy = true;
            }
            else if (nextWallType == 2)
            {
                // 销毁自己和那个可破坏墙
                if (isLocal)
                {
                    var wallData = new WallDestroyData { X = nextX, Y = nextY };
                    NetworkManager.Instance.SendMessage("wall_destroy", wallData);
                }
                MapManager.Instance.SetWallType(nextX, nextY, 0);
                shouldDestroy = true;
                
            }
            
       
            
            if (shouldDestroy)
            {
                if (isLocal)
                {
                    // 发送子弹销毁消息
                    SendBulletDestroy(nextX, nextY);
                }
                
                Destroy(gameObject);
                yield break;
            }
            
            // 没有碰撞，继续移动
            transform.position = new Vector3(nextX, nextY);
        }
    }
    
    void HandleTankHit(TankController tank)
    {
        // 计算新的HP
        int newHP = tank.HP - 1;
        
        // 发送玩家受击消息
        var hitData = new PlayerHitData
        {
            PlayerID = tank.PlayerID,
            HP = newHP,
            HitByID = OwnerID
        };
        
        NetworkManager.Instance.SendMessage("player_hit", hitData);
        
        // 本地更新坦克HP
        tank.TakeDamage(newHP);
    }
    
    void SendBulletDestroy(int hitX, int hitY)
    {
        var destroyData = new BulletDestroyData
        {
            ID = BulletID,
            PlayerID = OwnerID,
            HitX = hitX,
            HitY = hitY // 注意：在网络传输中使用Y
        };
        
        NetworkManager.Instance.SendMessage("bullet_destroy", destroyData);
        
        // 从GameManager中移除子弹记录
        GameManager.Instance.bullets.Remove(BulletID);
    }
}
