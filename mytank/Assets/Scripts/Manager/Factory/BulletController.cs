using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;
using Random =UnityEngine.Random;

public class BulletController : MonoBehaviour
{

    public Direction direction;
     public string ownerID;

    public bool isPlayerBullet;

    public float moveInterval = 0.3f;
    
    public float lastMoveTime;

    public Vector2Int dir;
    public Vector2Int Pos; // 子弹左下角坐标（2x2占地）
    
    // 坐标系统辅助方法
    public Vector2Int GetTopLeft() => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int GetTopRight() => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int GetBottomLeft() => Pos; // 左下角就是Pos
    public Vector2Int GetBottomRight() => new Vector2Int(Pos.x + 1, Pos.y);
    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);

   
    public float moveDuration = 0.3f; // DOTween动画时长
    public Animator animator;
    
    public bool isShouldDestroy = false;
    public float animTime = 0.3f;
   
    
  
    public void UpdatePosition(int x, int y)
    {
        direction.ToVector2Int().Set(x,y);
        Pos = new Vector2Int(x, y);
        transform.DOKill();
        transform.DOMove(GetCenter(), moveDuration).SetEase(Ease.Linear);
    }

    private void Update()
    {
        if (isShouldDestroy) return;
       
        // 使用帧数进行时间判断，确保所有客户端同步
        long currentFrame = NetworkManager.Instance.currentFrame;
        float frameTime = currentFrame * 0.05f; // 每帧0.05秒
        
        if (frameTime - lastMoveTime > moveInterval)
        {
            lastMoveTime = frameTime;
            bool isShouldMove = false;
            
            Vector2Int newPos = Pos + dir;
            
            // 检查2x2区域碰撞
            var tank = MapManager.Instance.GetTankInArea(newPos.x, newPos.y, 2, 2);
            
            
            if (tank != null && tank.isPlayer== isPlayerBullet)
            {
                isShouldMove=true;
            }
            else if (tank != null && tank.isPlayer!= isPlayerBullet)
            {
                tank.DamageHP(1,ownerID);
                isShouldDestroy = true;
            }

            foreach (var bullet in BulletFactory.Instance.allBullets)
            {
                if(bullet==this)continue;
                if (bullet.Pos == Pos && bullet.isPlayerBullet != isPlayerBullet)
                {
                    isShouldDestroy=true;
                    bullet.isShouldDestroy = true;
                    bullet.DestroyBullet();
                    break;
                }
            }
      
            
            if (MapManager.Instance.IsAreaBulletPassable(newPos.x, newPos.y, 2, 2))
            {
                isShouldMove=true;
                

            }
            else if (MapManager.Instance.IsHintHome(newPos.x, newPos.y, 2, 2))
            {
                PlayerManager.Instance.GameFail();
            }
            else
            {
                if (dir == new Vector2Int(0, -1))
                {
                    if (MapManager.Instance.GetWallType(Pos.x, Pos.y - 1) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x, Pos.y - 1, MapType.floor);
                    }

                    if (MapManager.Instance.GetWallType(Pos.x+1, Pos.y - 1) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x+1, Pos.y - 1, MapType.floor);
                    }
                }
                else if (dir == new Vector2Int(0, 1))
                {
                    if (MapManager.Instance.GetWallType(Pos.x, Pos.y +2) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x, Pos.y +2, MapType.floor);
                    }

                    if (MapManager.Instance.GetWallType(Pos.x+1, Pos.y +2) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x+1, Pos.y +2, MapType.floor);
                    }
                }
                else if (dir == new Vector2Int(1, 0))
                {
                    if (MapManager.Instance.GetWallType(Pos.x+2, Pos.y ) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x+2, Pos.y , MapType.floor);
                    }

                    if (MapManager.Instance.GetWallType(Pos.x+2, Pos.y +1) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x+2, Pos.y +1, MapType.floor);
                    }
                }
                else if (dir == new Vector2Int(-1, 0))
                {
                    if (MapManager.Instance.GetWallType(Pos.x-1, Pos.y ) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x-1, Pos.y , MapType.floor);
                    }

                    if (MapManager.Instance.GetWallType(Pos.x-1, Pos.y +1) == MapType.breakableWall)
                    {
                        MapManager.Instance.SetWallType(Pos.x+-1, Pos.y +1, MapType.floor);
                    }
                }
                isShouldDestroy = true;
            }
            if (isShouldMove)
            {
                UpdatePosition(newPos.x, newPos.y);
            
            }
            
            if (isShouldDestroy)
            {
                DestroyBullet();
            }
        }

        
        
    }

    public void DestroyBullet()
    {
        Pos=new Vector2Int(-1, -1);
        Vector2 randomPos=new Vector2(Random.Range(0f,dir.x), Random.Range(0f,dir.y ));
        transform.position += (Vector3)randomPos;
        animator.Play("SmallBoom");
        DOVirtual.DelayedCall(animTime, () => 
        {
            BulletFactory.Instance.BulletPool.ReturnObject(this);
        });
    }
    
    
}
