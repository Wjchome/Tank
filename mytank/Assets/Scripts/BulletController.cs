using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Random =UnityEngine.Random;

public class BulletController : MonoBehaviour
{
    public string BulletID { get; private set; }
    public Direction Direction { get; private set; }
    public string OwnerID { get; private set; }
    
    private bool isLocal; // 是否由本地客户端创建

    public float moveInterval = 0.3f;
    
    private float lastMoveTime;

    private Vector2Int dir;
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
    public void Initialize(string id, Direction direction, string ownerID, bool local = false)
    {
        BulletID = id;
        Direction = direction;
        OwnerID = ownerID;
        isLocal = local;
        // 设置子弹朝向
        SetBulletRotation();
        
        // 根据坦克位置计算子弹初始位置
        TankController ownerTank = GameStateManager.Instance.playerTanks[ownerID];
        if (ownerTank != null)
        {
            Pos = ownerTank.Pos;
            
            // 设置子弹中心位置
            transform.position = GetCenter();
        }
    }
    
    private void SetBulletRotation()
    {
        Vector3 rotation = Vector3.zero;
        switch (Direction)
        {
            case Direction.Up: 
                rotation = new Vector3(0, 0, 0);
                dir = Direction.ToVector2Int();  
                break;
            case Direction.Down: 
                rotation = new Vector3(0, 0, 180);  
                dir = Direction.ToVector2Int(); 
                break;
            case Direction.Left: 
                rotation = new Vector3(0, 0, 90);  
                dir = Direction.ToVector2Int(); 
                break;
            case Direction.Right: 
                rotation = new Vector3(0, 0, -90);  
                dir = Direction.ToVector2Int(); 
                break;
        }
        transform.rotation = Quaternion.Euler(rotation);
    }
    
  
    public void UpdatePosition(int x, int y)
    {
        Pos = new Vector2Int(x, y);
        transform.DOKill();
        transform.DOMove(GetCenter(), moveDuration).SetEase(Ease.Linear);
    }

    private void Update()
    {
        if (isShouldDestroy) return;
       
        if (Time.time - lastMoveTime > moveInterval)
        {
            lastMoveTime = Time.time;
            bool isShouldMove = false;
            
            Vector2Int newPos = Pos + dir;
            
            // 检查2x2区域碰撞
            var tank = MapManager.Instance.GetTankInArea(newPos.x, newPos.y, 2, 2);
            
            if (tank != null && tank.PlayerID == OwnerID)
            {
                isShouldMove=true;
            }
            else if (tank != null && tank.PlayerID != OwnerID)
            {
                tank.DamageHP(1,OwnerID);
                isShouldDestroy = true;
            }
            if (MapManager.Instance.IsAreaBulletPassable(newPos.x, newPos.y, 2, 2))
            {
                isShouldMove=true;
                

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
        Vector2 randomPos=new Vector2(Random.Range(0f,dir.x), Random.Range(0f,dir.y ));
        transform.position += (Vector3)randomPos;
        animator.Play("SmallBoom");
        Destroy(gameObject,animTime);
    }
}
