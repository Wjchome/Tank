using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using FixMath.NET;
using QuadTreeV3;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BulletController : MapEntity
{
    public Direction direction;

    public bool isPlayerBullet;

    public int moveIntervalFrame = 4;

    public long lastMoveTimeFrame;

    public FixVector2 dir;
    public FixRect myFixRect;
    public float moveSpeed;
    public Fix64 moveSpeedF => (Fix64)moveSpeed;

    public Vector2Int PosUp => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int PosUpRight => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int PosRight => new Vector2Int(Pos.x + 1, Pos.y);

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
        direction.ToVector2Int().Set(x, y);
        Pos = new Vector2Int(x, y);
        transform.DOKill();
        transform.DOMove(GetCenter(), moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);
    }

    bool ShouldCollide(Identity identity)
    {
        bool isPlayer = (identity == Identity.Myself || identity == Identity.OtherPlayer);

        return isPlayer == isPlayerBullet;
    }

    private Vector2 _smoothVelocity;
    private void Update()
    {
        UpdatePos();
    }

    public void UpdatePos()
    {
        transform.position = Vector2.SmoothDamp(transform.position, (Vector2)myFixRect.Center, ref _smoothVelocity, 0.1f);

    }
    public override void UpdateFrame()
    {
        if (!isShouldDestroy)
        {
            transform.DOKill();

            FixRect targetRect = new FixRect(myFixRect.X + (Fix64)dir.x * moveSpeedF,
                myFixRect.Y + (Fix64)dir.y * moveSpeedF,
                myFixRect.Width, myFixRect.Height);
            List<QuadTreeObject> get = QuadTreeV3.QuadTreeV3.Instance.Query(targetRect);

            bool isOk = true;
            for (int i = 0; i < get.Count; i++)
            {
                if (get[i].Target == gameObject)
                {
                    continue;
                }

                if (get[i].Target.CompareTag("Wall"))
                {
                    isDead = true;
                    isOk = false;
                }

                if (get[i].Target.CompareTag("BreakableWall"))
                {
                    isDead = true;
                    isOk = false;
                }

                if (get[i].Target.CompareTag("Tank"))
                {
                    if (!ShouldCollide(get[i].Target.GetComponent<TankController>().identity))
                    {
                        isDead = true;
                        isOk = false;
                    }
                }

                if (get[i].Target.CompareTag("Bullet"))
                {
                    if (isPlayerBullet != get[i].Target.GetComponent<BulletController>().isPlayerBullet)
                    {
                        isDead = true;
                        isOk = false;
                    }
                }
            }

            if (isOk)
            {
                myFixRect = targetRect;
            }

            QuadTreeV3.QuadTreeV3.Instance.UpdateObject(gameObject, myFixRect);
        }

        // if (!isShouldDestroy && NetworkManager.Instance.currentFrame - lastMoveTimeFrame > moveIntervalFrame)
        // {
        //     lastMoveTimeFrame = NetworkManager.Instance.currentFrame;
        //     bool isShouldMove = false;
        //
        //     Vector2Int newPos = Pos + dir;
        //
        //     // 检查2x2区域碰撞
        //     var tanks = MapManager.Instance.GetTankInArea(newPos.x, newPos.y, 2, 2);
        //
        //     foreach (var tank in tanks)
        //     {
        // if (tank != null && ShouldCollide(tank.identity))
        //         {
        //             isShouldMove = true;
        //         }
        //         else if (tank != null && !ShouldCollide(tank.identity))
        //         {
        //             if (tank is EnemyTankController enemy)
        //             {
        //                 enemy.DamageHP(damageNum, this.tank as PlayerTankController, DamageType.Bullet);
        //             }
        //             else if (tank is PlayerTankController player)
        //             {
        //                 player.DamageHP(damageNum);
        //             }
        //
        //             // 穿透逻辑：减少穿透次数而不是直接销毁
        //             penetrationCount--;
        //             if (penetrationCount <= 0)
        //             {
        //                 isShouldDestroy = true;
        //             }
        //         }
        //     }
        //
        //     foreach (var bullet in EntityManager.Instance.activeBullets)
        //     {
        //         if (bullet == this) continue;
        //         if (bullet.Pos == Pos && bullet.isPlayerBullet != isPlayerBullet)
        //         {
        //             // 穿透逻辑：减少穿透次数而不是直接销毁
        //             penetrationCount--;
        //             if (penetrationCount <= 0)
        //             {
        //                 isShouldDestroy = true;
        //             }
        //
        //             // 对方子弹也减少穿透次数
        //             bullet.penetrationCount--;
        //             if (bullet.penetrationCount <= 0)
        //             {
        //                 bullet.isShouldDestroy = true;
        //                 bullet.DestroyBullet();
        //             }
        //
        //
        //             break;
        //         }
        //     }
        //
        //
        //     if (MapManager.Instance.IsAreaBulletPassable(newPos.x, newPos.y, 2, 2))
        //     {
        //         isShouldMove = true;
        //     }
        //     else if (MapManager.Instance.IsHintHome(newPos.x, newPos.y, 2, 2))
        //     {
        //         GameStateManager.Instance.GameOver(false);
        //     }
        //     else
        //     {
        //         Vector2Int wallPos1, wallPos2;
        //
        //         if (dir == new Vector2Int(0, -1)) // 向下
        //         {
        //             wallPos1 = new Vector2Int(Pos.x, Pos.y - 1);
        //             wallPos2 = new Vector2Int(Pos.x + 1, Pos.y - 1);
        //         }
        //         else if (dir == new Vector2Int(0, 1)) // 向上
        //         {
        //             wallPos1 = new Vector2Int(Pos.x, Pos.y + 2);
        //             wallPos2 = new Vector2Int(Pos.x + 1, Pos.y + 2);
        //         }
        //         else if (dir == new Vector2Int(1, 0)) // 向右
        //         {
        //             wallPos1 = new Vector2Int(Pos.x + 2, Pos.y);
        //             wallPos2 = new Vector2Int(Pos.x + 2, Pos.y + 1);
        //         }
        //         else if (dir == new Vector2Int(-1, 0)) // 向左
        //         {
        //             wallPos1 = new Vector2Int(Pos.x - 1, Pos.y);
        //             wallPos2 = new Vector2Int(Pos.x - 1, Pos.y + 1);
        //         }
        //         else
        //         {
        //             return; // 无效方向
        //         }
        //
        //         // 破坏两个墙壁位置
        //         BreakWallIfPossible(wallPos1);
        //         BreakWallIfPossible(wallPos2);
        //
        //         isShouldDestroy = true;
        //     }
        //
        //     if (isShouldMove)
        //     {
        //         UpdatePosition(newPos.x, newPos.y);
        //     }
        //
        //     if (isShouldDestroy)
        //     {
        //         DestroyBullet();
        //     }
        // }
        //
        if (isDead)
        {
            Debug.Log(NetworkManager.Instance.currentFrame);
            if (NetworkManager.Instance.currentFrame >= deathDelayFrames)
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
        if (wallType == MapType.breakableWall
           )
        {
            MapManager.Instance.SetWallType(wallPos.x, wallPos.y, MapType.floor);
        }
        else if (tank is PlayerTankController player)
        {
            if (player.isBreakWall && wallType == MapType.wall)
            {
                MapManager.Instance.SetWallType(wallPos.x, wallPos.y, MapType.floor);
            }
        }
    }

    public void DestroyBullet()
    {
        if (isDead) return;
        isDead = true;
        Pos = new Vector2Int(-1, -1);
        Vector2 randomPos = new Vector2(Random.Range(0f, (float)dir.x), Random.Range(0f, (float)dir.y));
        transform.position += (Vector3)randomPos;
        animator.Play("SmallBoom");
        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
    }

    // 执行死亡后的逻辑
    void ExecuteDeathLogic()
    {
        EntityManager.Instance.BulletPool.ReturnObject(this);
    }

    private void OnDrawGizmos()
    {
        var bound = myFixRect.GetBoundingRect();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2((float)bound.X, (float)bound.Y),
            new Vector2((float)(bound.X + bound.Width), (float)bound.Y));
        Gizmos.DrawLine(new Vector2((float)(bound.X + bound.Width), (float)bound.Y),
            new Vector2((float)(bound.X + bound.Width), (float)(bound.Y + bound.Height)));
        Gizmos.DrawLine(new Vector2((float)(bound.X + bound.Width), (float)(bound.Y + bound.Height)),
            new Vector2((float)(bound.X), (float)(bound.Y + bound.Height)));
        Gizmos.DrawLine(new Vector2((float)(bound.X), (float)(bound.Y + bound.Height)),
            new Vector2((float)bound.X, (float)bound.Y));
    }
}