using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using FixMath.NET;
using Physics2D;
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
    public Transform body;
    public Transform barrel;
    
    public int shootIntervalFrame;
    public int HP;
    public int orignalHP;

    public string tankID;

    public Direction tankDirection;

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
    
    public RigidBody2DComponent rigidBody2D;

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
     
        //   transform.position = (Vector2)rigidBody2D.Body.Position;
        // Vector2.SmoothDamp(transform.position, (Vector2)rigidBody2D.Body.Position, ref _smoothVelocity, 0.1f);
        
        transform.position =Vector2.Lerp( transform.position , (Vector2)rigidBody2D.Body.Position,Time.deltaTime);
           
        body.rotation = Quaternion.Slerp(body.rotation, Quaternion.Euler(0f, 0f, (float)rotationF),
            5 * Time.deltaTime);
        
    }


    public void  MoveBy(Direction direction)
    {
        var (dxy, dr) = direction.ToFixVector2();
        
        Fix64 moveMul = Fix64.One;
        foreach (var item in rigidBody2D.Stay)
        {
            if (item.gameObject.CompareTag("Ice"))
            {
                moveMul = new Fix64(2);
                break;
            }
        }
        rigidBody2D.Body.ApplyForce(dxy * (moveSpeedF * moveMul));
        if (rigidBody2D.Body.Shape is BoxShape2D boxShape2D)
        {
            boxShape2D.Rotation = dr * Fix64.Deg2Rad;
        }
        
        rotationF = dr;
        tankDirection = direction;
            

        isMoving = true;
        animator.Play("Tank" + animType);

    }

    public abstract void AddOrignalHP(int num);

    public abstract void AddHP(int num);


    protected abstract void ExecuteDeathLogic();


}