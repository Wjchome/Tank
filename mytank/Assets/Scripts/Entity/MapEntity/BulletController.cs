using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using FixMath.NET;
using Physics2D;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BulletController : MapEntity
{
    public bool isPlayerBullet;


    public FixVector2 dir;
    public FixRect myFixRect => rigidBody2D.Body.Shape.GetBounds(rigidBody2D.Body.Position);
    public float moveSpeed;
    public Fix64 moveSpeedF => (Fix64)moveSpeed;

    public RigidBody2DComponent rigidBody2D;
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
            bool isCango = true;
            foreach (var enter in rigidBody2D.Enter)
            {
                if (enter.gameObject.CompareTag("Wall"))
                {
                    DestroyBullet();
                    isCango = false;
                }
                else if (enter.gameObject.CompareTag("BreakableWall"))
                {
                    PhysicsWorld2DComponent.Instance.World.RemoveBody(enter);
                    Destroy(enter.gameObject);
                    MapManager.Instance.CreateWallType(enter.Position, MapType.floor);

                    DestroyBullet();
                    isCango = false;
                }
                else if (enter.gameObject.CompareTag("Bullet"))
                {
                    BulletController _bulletController = enter.gameObject.GetComponent<BulletController>();

                    if (isPlayerBullet != _bulletController.isPlayerBullet)
                    {
                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();
                            isCango = false;

                            _bulletController.penetrationCount--;
                            if (_bulletController.penetrationCount <= 0)
                            {
                                _bulletController.DestroyBullet();
                            }
                        }
                    }
                }
                else if (enter.gameObject.CompareTag("Tank"))
                {
                    TankController tank = enter.gameObject.GetComponent<TankController>();
                
                    if (isPlayerBullet && tank is EnemyTankController enemy )
                    {
                        enemy .DamageHP(damageNum, this.tank as PlayerTankController, DamageType.Bullet);
                        
                        penetrationCount--;
                        if (penetrationCount <= 0)
                        {
                            DestroyBullet();
                            isCango = false;
                        }
                    }
                    else if (!isPlayerBullet && tank is PlayerTankController player)
                    {
                        player.DamageHP(damageNum);
                    }
                }
            }

            if (isCango)
            {
                rigidBody2D.Body.ApplyImpulse(dir*moveSpeedF);
            }
        }
        else
        {
            if (NetworkManager.Instance.currentFrame >= deathDelayFrames)
            {
                ExecuteDeathLogic();
            }
        }
    }



    public void DestroyBullet()
    {
        if (isDead)
        {
            return;
        }

        Debug.Log("死亡 " + NetworkManager.Instance.currentFrame);
        isDead = true;

        animator.Play("SmallBoom");
        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
        // QuadTreeV3.QuadTreeV3.Instance.RemoveObject(gameObject);
        PhysicsWorld2DComponent.Instance.World.RemoveBody(rigidBody2D.Body);
    }

    // 执行死亡后的逻辑
    void ExecuteDeathLogic()
    {
        Debug.Log("真死亡 " + NetworkManager.Instance.currentFrame);

        EntityManager.Instance.BulletPool.ReturnObject(this);
    }

    private void OnDrawGizmos()
    {
        var bound = myFixRect;
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