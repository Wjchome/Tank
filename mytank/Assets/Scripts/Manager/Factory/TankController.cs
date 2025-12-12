using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using FixMath.NET;
using QuadTreeV3;
using UnityEngine.Serialization;


public enum Identity
{
    Myself,
    OtherPlayer,
    Enemy
}

//给碰撞用的
public enum QuadTreeLayerType : Int32
{
    Wall,
    BreakableWall,
    River,
    Ice,
    TankFriend,
    TankEnemy,
    BulletFriend,
    BulletEnemy
}

public abstract class TankController : MonoBehaviour
{
    public Vector2Int Pos;
    public int shootIntervalFrame;
    public int HP;
    public int orignalHP;

    public string tankID;

    public Direction tankDirection;

    public FixRect myFixRect;
    public float moveSpeed;
    public Fix64 moveSpeedF => (Fix64)moveSpeed;
    public Fix64 rotationF = Fix64.Zero;


    // 一些计时器
    public long lastShootFrame;

    // 玩家自选名字
    public string playerName;

    // 玩家自选颜色
    public Color playerColor;

    // 身份
    public Identity identity;

    // 控制动画
    public bool isMoving = false;

    //死亡动画时间
    public float animTime = 0.5f;

    //是否死亡
    public bool isDead;

    //本地动画机
    public Animator animator;
    public Animator bombAnimator;
    public TankEntityTankUI tankEntityTankUI;

    public SpriteRenderer spriteRenderer;

    public System.Random random = new System.Random();

    public int animType = 1;

    public int bulletDamageNum;

    public long deathDelayFrames;


    public virtual void UpdateFrame()
    {
        CheckMovementState();

        CheckDeadState();
    }

    void CheckMovementState()
    {
        if (isDead) return;
        if (isMoving)
        {
            isMoving = false;
            animator.Play("Idle" + animType);
        }
        else
        {
            animator.Play("Idle" + animType);
        }
    }

    void CheckDeadState()
    {
        if (isDead)
        {
            if (NetworkManager.Instance.currentFrame >= deathDelayFrames)
            {
                // 执行死亡后的逻辑
                ExecuteDeathLogic();
            }
        }
    }


    Vector2 _smoothVelocity;

    public virtual void Update()
    {
        UpdatePos();
    }

    public void UpdatePos()
    {
        transform.position =
            Vector2.SmoothDamp(transform.position, (Vector2)myFixRect.Center, ref _smoothVelocity, 0.1f);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, (float)rotationF),
            5 * Time.deltaTime);
    }


    //是否撞到墙
    public bool MoveBy(Direction direction)
    {
        bool canMove = false;
        var (dxy, dr) = direction.ToFixVector2();

        List<QuadTreeObject> cur = QuadTreeV3.QuadTreeV3.Instance.Query(myFixRect);
        Fix64 moveMul = Fix64.One;
        foreach (var item in cur)
        {
            if (item.Target == gameObject)
            {
                continue;
            }

            if (item.Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.Ice) ))
            {
                moveMul = new Fix64(2);
                break;
            }
        }

        FixVector2 moveVec = dxy * (moveSpeedF * moveMul);
        transform.DOKill();
        FixRect targetRect = new FixRect(myFixRect.X + moveVec.x, myFixRect.Y + moveVec.y,
            myFixRect.Width, myFixRect.Height);
        List<QuadTreeObject> get = QuadTreeV3.QuadTreeV3.Instance.Query(targetRect);
        bool isOk = true;
        
        for (int i = 0; i < get.Count; i++)
        {
            if (get[i].Target == gameObject)
            {
                continue;
            }


            if (get[i].Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.Wall) |
                                        QuadTreeLayer.GetLayer((int)QuadTreeLayerType.BreakableWall) |
                                        QuadTreeLayer.GetLayer((int)QuadTreeLayerType.River) |
                                        QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankEnemy) |
                                        QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankFriend)))
            {
                canMove = true;
                FixRect other = get[i].Bounds;
                FixVector2 vec1 =
                    new FixVector2(targetRect.CenterX - other.CenterX, targetRect.CenterY - other.CenterY);
                if (Fix64.Abs(vec1.x) < Fix64.Abs(vec1.y))
                {
                    if (vec1.y > Fix64.Zero)
                    {
                        moveVec.y = Fix64.Max(moveVec.y, Fix64.Zero);
                    }
                    else
                    {
                        moveVec.y = Fix64.Min(moveVec.y, Fix64.Zero);
                    }
                }
                else
                {
                    if (vec1.x > Fix64.Zero)
                    {
                        moveVec.x = Fix64.Max(moveVec.x, Fix64.Zero);
                    }
                    else
                    {
                        moveVec.x = Fix64.Min(moveVec.x, Fix64.Zero);
                    }
                }
            }
        }

        if (moveVec != FixVector2.Zero)
        {
            myFixRect = new FixRect(myFixRect.X + moveVec.x, myFixRect.Y + moveVec.y,
                myFixRect.Width, myFixRect.Height);
        }

        rotationF = dr;
        tankDirection = direction;

        QuadTreeV3.QuadTreeV3.Instance.UpdateObject(gameObject, myFixRect);

        isMoving = true;
        animator.Play("Tank" + animType);

        return canMove;
    }

    public abstract void Shoot();


    public abstract void AddOrignalHP(int num);

    public abstract void AddHP(int num);


    protected abstract void ExecuteDeathLogic();

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