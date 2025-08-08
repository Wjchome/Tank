using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using UnityEngine.Serialization;


public enum Identity
{
    Myself,
    OtherPlayer,
    Enemy
}

public abstract class TankController : MonoBehaviour
{
    public int moveIntervalFrame ;
    public int shootIntervalFrame ;
    public int HP;
    public int orignalHP;

    public string tankID;
    public Direction tankDirection;

    public Vector2Int Pos;

    // 坐标系统辅助方法
    public Vector2Int PosUp => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int PosUpRight => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int PosRight => new Vector2Int(Pos.x + 1, Pos.y);

    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);

   

    //一些计时器
    public long lastMoveFrame;
    public long lastShootFrame;
    public long lastAnimStartFrame;

    // 玩家自选名字
    public string playerName;

    //玩家自选颜色
    public Color playerColor;

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
            if (NetworkManager.Instance.currentFrame - lastAnimStartFrame > moveIntervalFrame)
            {
                isMoving = false;
                animator.Play("Idle" + animType);
            }
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






    protected abstract void MoveTo(Vector2Int targetPos);

    protected abstract bool IsCanMoveTo(Vector2Int targetPos);
    // 帧同步调用
    public bool MoveBy(Direction direction)
    {
        bool canMove = false;
        int dx = 0, dy = 0;
        switch (direction)
        {
            case Direction.Up:
                dx = 0;
                dy = 1;
                break;
            case Direction.Down:
                dx = 0;
                dy = -1;
                break;
            case Direction.Left:
                dx = -1;
                dy = 0;
                break;
            case Direction.Right:
                dx = 1;
                dy = 0;
                break;
        }

        // 停止当前动画，防止插值冲突
        transform.DOKill();
        bool underice = MapManager.Instance.HasTankInMapTypes(Pos.x, Pos.y,new List<MapType> { MapType.ice });
        if (underice)
        {
            Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
            if (IsCanMoveTo(targetPos))
            {
                MoveTo(targetPos);
                canMove = true;
            }

            targetPos = Pos + new Vector2Int(dx, dy);
            if (IsCanMoveTo(targetPos))
            {
                MoveTo(targetPos);
                canMove = true;
            }
        }
        else
        {
            Vector2Int targetPos = Pos + new Vector2Int(dx, dy);

            if (IsCanMoveTo(targetPos))
            {
                MoveTo(targetPos);
                canMove = true;
            }
        }

        tankDirection = direction;
        Vector3 rotation = Vector3.zero;
        switch (direction)
        {
            case Direction.Up: rotation = new Vector3(0, 0, 0); break;
            case Direction.Down: rotation = new Vector3(0, 0, 180); break;
            case Direction.Left: rotation = new Vector3(0, 0, 90); break;
            case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        }

        // 旋转也用DOTween
        transform.DORotate(rotation, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.OutQuad);
        return canMove;
    }

    public abstract void Shoot();

    

    public abstract void AddOrignalHP(int num);

    public abstract void AddHP(int num);



    protected abstract void ExecuteDeathLogic();




}