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
            
            // 计算移动位置
            Vector3 currentPos = transform.position;
            int currentX = Mathf.RoundToInt(currentPos.x);
            int currentY = Mathf.RoundToInt(currentPos.y);
            
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
            
            // 检查碰撞
            RaycastHit[] hits = Physics.RaycastAll(
                new Vector3(currentX, 0.5f, currentY),
                new Vector3(nextX - currentX, 0, nextY - currentY).normalized,
                1.0f
            );
            
            bool shouldDestroy = false;
            string hitType = "";
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    // 不可破坏墙
                    shouldDestroy = true;
                    hitType = "wall";
                    break;
                }
                else if (hit.collider.CompareTag("BreakableWall"))
                {
                    // 可破坏墙
                    shouldDestroy = true;
                    hitType = "breakable";
                    if (isLocal)
                    {
                        // 本地逻辑：破坏墙
                        Destroy(hit.collider.gameObject);
                    }
                    break;
                }
                else if (hit.collider.CompareTag("Tank"))
                {
                    // 坦克
                    TankController tank = hit.collider.GetComponent<TankController>();
                    if (tank.PlayerID != OwnerID) // 不会命中自己
                    {
                        shouldDestroy = true;
                        hitType = "tank";
                        
                        if (isLocal)
                        {
                            // 处理坦克被击中逻辑
                            HandleTankHit(tank);
                        }
                        break;
                    }
                }
            }
            
            if (shouldDestroy)
            {
                if (isLocal)
                {
                    // 发送子弹销毁消息
                    SendBulletDestroy(nextX, nextY, hitType);
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
    
    void SendBulletDestroy(int hitX, int hitZ, string hitType)
    {
        var destroyData = new BulletDestroyData
        {
            ID = BulletID,
            PlayerID = OwnerID,
            HitX = hitX,
            HitY = hitZ // 注意：在网络传输中使用Y
        };
        
        NetworkManager.Instance.SendMessage("bullet_destroy", destroyData);
        
        // 从GameManager中移除子弹记录
        GameManager.Instance.bullets.Remove(BulletID);
    }
}
