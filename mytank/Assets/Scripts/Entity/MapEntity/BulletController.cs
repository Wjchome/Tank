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


    public FixVector2 dir;
    public FixRect myFixRect;
    public float moveSpeed;
    public Fix64 moveSpeedF => (Fix64)moveSpeed;


    public Animator animator;

    public float animTime = 0.3f;

    public int damageNum;

    public int penetrationCount;

    public bool isDead = false;
    public long deathDelayFrames;


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
        transform.position =
            Vector2.SmoothDamp(transform.position, (Vector2)myFixRect.Center, ref _smoothVelocity, 0.1f);
    }

    public override void UpdateFrame()
    {
        if (!isDead)
        {
            FixRect targetRect = new FixRect(myFixRect.X + (Fix64)dir.x * moveSpeedF,
                myFixRect.Y + (Fix64)dir.y * moveSpeedF,
                myFixRect.Width, myFixRect.Height);
            List<QuadTreeObject> get = QuadTreeV3.QuadTreeV3.Instance.Query(targetRect);

            bool isCango = true;
            for (int i = 0; i < get.Count; i++)
            {
                if (get[i].Target == gameObject)
                {
                    continue;
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.Wall)))
                {
                    DestroyBullet();

                    isCango = false;
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.BreakableWall)))
                {
                    //删除 这个墙
                    DestroyBullet();

                    isCango = false;
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy)))
                {
                    if (isPlayerBullet)
                    {
                        BulletController _bulletController = get[i].Target.GetComponent<BulletController>();

                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();

                            isCango = false;

                            _bulletController.penetrationCount--;
                            if (_bulletController.penetrationCount <= 0)
                            {
                                DestroyBullet();
                            }
                        }
                    }
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.BulletFriend)))
                {
                    if (!isPlayerBullet)
                    {
                        BulletController _bulletController = get[i].Target.GetComponent<BulletController>();

                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();

                            isCango = false;

                            _bulletController.penetrationCount--;
                            if (_bulletController.penetrationCount <= 0)
                            {
                                DestroyBullet();
                            }
                        }
                    }
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankEnemy)))
                {
                    Debug.Log("qwe");
                    if (isPlayerBullet)
                    {
                        EnemyTankController enemy = get[i].Target.GetComponent<EnemyTankController>();
                        enemy.DamageHP(damageNum, this.tank as PlayerTankController, DamageType.Bullet);


                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();
                            isCango = false;
                        }
                    }
                }

                if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankFriend)))
                {
                    if (!isPlayerBullet)
                    {
                        PlayerTankController player = get[i].Target.GetComponent<PlayerTankController>();
                        player.DamageHP(damageNum);

                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();
                            isCango = false;
                        }
                    }
                }
            }

            if (isCango)
            {
                myFixRect = targetRect;
            }

            QuadTreeV3.QuadTreeV3.Instance.UpdateObject(gameObject, myFixRect);


            //     if (MapManager.Instance.IsHintHome(newPos.x, newPos.y, 2, 2))
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
        }
        else
        {
            if (NetworkManager.Instance.currentFrame >= deathDelayFrames)
            {
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
        if (isDead)
        {
            Debug.Log("赋值死亡多次");
            return;
        }

        Debug.Log("死亡 " + NetworkManager.Instance.currentFrame);
        isDead = true;

        animator.Play("SmallBoom");
        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
        QuadTreeV3.QuadTreeV3.Instance.RemoveObject(gameObject);
    }

    // 执行死亡后的逻辑
    void ExecuteDeathLogic()
    {
        Debug.Log("真死亡 " + NetworkManager.Instance.currentFrame);

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