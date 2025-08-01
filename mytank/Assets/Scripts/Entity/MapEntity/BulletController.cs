using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;
using Random =UnityEngine.Random;

public class BulletController :MapEntity
{

    public Direction direction;

    public bool isPlayerBullet;

    public int moveIntervalFrame = 4;
    
   public long lastMoveTimeFrame;

    public Vector2Int dir;

    
    // 坐标系统辅助方法

    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);

   
    public Animator animator;
    
    public bool isShouldDestroy = false;
    public float animTime = 0.3f;

    public int damageNum;

    public int penetrationCount;
    
    public bool isDead = false;
    public long deathDelayFrames;
    public void UpdatePosition(int x, int y)
    {
        direction.ToVector2Int().Set(x,y);
        Pos = new Vector2Int(x, y);
        transform.DOKill();
        transform.DOMove(GetCenter(), moveIntervalFrame*Constant.FrameInterval).SetEase(Ease.Linear);
    }

    bool ShouldCollide(Identity identity)
    {
        bool isPlayer=(identity == Identity.Myself||identity==Identity.OtherPlayer);
        
        return isPlayer==isPlayerBullet;
    }
    
    public override void UpdateFrame()
    {
        
        
        
        if (!isShouldDestroy&& NetworkManager.Instance.currentFrame - lastMoveTimeFrame > moveIntervalFrame)
        {
            lastMoveTimeFrame = NetworkManager.Instance.currentFrame;
            bool isShouldMove = false;
            
            Vector2Int newPos = Pos + dir;
            
            // 检查2x2区域碰撞
            var tanks = MapManager.Instance.GetTankInArea(newPos.x, newPos.y, 2, 2);

            foreach (var tank in tanks)
            {


                if (tank != null && ShouldCollide(tank.identity))
                {
                    isShouldMove = true;
                }
                else if (tank != null && !ShouldCollide(tank.identity))
                {
                    tank.DamageHP(damageNum, this.tank);
                    
                    // 穿透逻辑：减少穿透次数而不是直接销毁
                    penetrationCount--;
                    if (penetrationCount <= 0)
                    {
                        isShouldDestroy = true;
                    }
                }
            }

            foreach (var bullet in EntityManager.Instance.activeBullets)
            {
                if(bullet==this)continue;
                if (bullet.Pos == Pos && bullet.isPlayerBullet != isPlayerBullet)
                {
                    // 穿透逻辑：减少穿透次数而不是直接销毁
                    penetrationCount--;
                    if (penetrationCount <= 0)
                    {
                        isShouldDestroy = true;
                    }
                    
                    // 对方子弹也减少穿透次数
                    bullet.penetrationCount--;
                    if (bullet.penetrationCount <= 0)
                    {
                        bullet.isShouldDestroy = true;
                    }
                    
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
                GameStateManager.Instance.GameOver(false);
            }
            else
            {
                Vector2Int wallPos1, wallPos2;
    
                if (dir == new Vector2Int(0, -1)) // 向下
                {
                    wallPos1 = new Vector2Int(Pos.x, Pos.y - 1);
                    wallPos2 = new Vector2Int(Pos.x + 1, Pos.y - 1);
                }
                else if (dir == new Vector2Int(0, 1)) // 向上
                {
                    wallPos1 = new Vector2Int(Pos.x, Pos.y + 2);
                    wallPos2 = new Vector2Int(Pos.x + 1, Pos.y + 2);
                }
                else if (dir == new Vector2Int(1, 0)) // 向右
                {
                    wallPos1 = new Vector2Int(Pos.x + 2, Pos.y);
                    wallPos2 = new Vector2Int(Pos.x + 2, Pos.y + 1);
                }
                else if (dir == new Vector2Int(-1, 0)) // 向左
                {
                    wallPos1 = new Vector2Int(Pos.x - 1, Pos.y);
                    wallPos2 = new Vector2Int(Pos.x - 1, Pos.y + 1);
                }
                else
                {
                    return; // 无效方向
                }
    
                // 破坏两个墙壁位置
                BreakWallIfPossible(wallPos1);
                BreakWallIfPossible(wallPos2);
    
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

        if (isDead )
        {
           
            if (NetworkManager.Instance.currentFrame  >= deathDelayFrames)
            {
                // 执行死亡后的逻辑
                ExecuteDeathLogic();
            }
        }
        
    }
    
    protected virtual void BreakWallIfPossible(Vector2Int wallPos)
    {
        MapType wallType = MapManager.Instance.GetWallType(wallPos.x, wallPos.y);
    
        // 检查是否可以破坏墙壁
        if (wallType == MapType.breakableWall)
        {
            MapManager.Instance.SetWallType(wallPos.x, wallPos.y, MapType.floor);
        }
    }

    public void DestroyBullet()
    {
        if(isDead)return;
        isDead=true;
        Pos=new Vector2Int(-1, -1);
        Vector2 randomPos=new Vector2(Random.Range(0f,dir.x), Random.Range(0f,dir.y ));
        transform.position += (Vector3)randomPos;
        animator.Play("SmallBoom");
        deathDelayFrames=NetworkManager.Instance.currentFrame+(int)(animTime/Constant.FrameInterval);
  
    }
    
    // 执行死亡后的逻辑
    void ExecuteDeathLogic()
    {
        EntityManager.Instance.BulletPool.ReturnObject(this);
        
    }
    
    
}
