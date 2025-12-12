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

public abstract class TankController : MonoBehaviour
{
    public int moveIntervalFrame;
    public int shootIntervalFrame;
    public int HP;
    public int orignalHP;

    public string tankID;

    public Direction tankDirection;

    public Vector2Int Pos;

    public FixRect myFixRect;
    public float moveSpeed;
    public Fix64 moveSpeedF => (Fix64)moveSpeed;
    public Fix64 rotationF = Fix64.Zero;


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
    protected abstract void MoveTo(FixVector2 targetPos);

    protected abstract bool IsCanMoveTo(Vector2Int targetPos);

    Vector2 _targetPos;
    Vector2 _smoothVelocity;

    public virtual void Update()
    {
        UpdatePos();
    }

    public void UpdatePos()
    {
        var centerX = (float)(myFixRect.X + myFixRect.Width / new Fix64(2));
        var centerY = (float)(myFixRect.Y + myFixRect.Height / new Fix64(2));
        _targetPos = new Vector2(centerX, centerY);
        transform.position = Vector2.SmoothDamp(transform.position, _targetPos, ref _smoothVelocity, 0.1f);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, (float)rotationF),
            5 * Time.deltaTime);
    }

    // 帧同步调用
    public bool MoveBy(Direction direction)
    {
        bool canMove = false;
        var (dxy,dr) = direction.ToFixVector2();
        
        // 停止当前动画，防止插值冲突
        transform.DOKill();
        // FixVector2 targetPos = PosF + new FixVector2((Fix64)dx, (Fix64)dy)*moveSpeedF;
        FixRect targetRect = new FixRect(myFixRect.X + dxy.x * moveSpeedF, myFixRect.Y + dxy.y * moveSpeedF,
            myFixRect.Width, myFixRect.Height);
        List<QuadTreeObject> get = QuadTreeV3.QuadTreeV3.Instance.Query(targetRect);
        bool isOk = true;
        for (int i = 0; i < get.Count; i++)
        {
            if (get[i].Target == gameObject)
            {
                continue;
            }

            if (get[i].Target.CompareTag("Wall") || get[i].Target.CompareTag("BreakableWall") ||
                get[i].Target.CompareTag("Tank"))
            {
                FixRect other = get[i].Bounds;
                //分离物体
                isOk = false;
            }
        }

        if (isOk)
        {
            myFixRect = targetRect;
        }

        rotationF = dr;
        tankDirection = direction;

        QuadTreeV3.QuadTreeV3.Instance.UpdateObject(gameObject, myFixRect);


        // transform.position = new Vector2((float)(myFixRect.X + myFixRect.Width / new Fix64(2)),
        //     (float)(myFixRect.Y + myFixRect.Height / new Fix64(2)));
        return canMove;
        // bool underice = MapManager.Instance.HasTankInMapTypes(Pos.x, Pos.y,new List<MapType> { MapType.ice });
        // if (underice)
        // {
        //     Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
        //     if (IsCanMoveTo(targetPos))
        //     {
        //         MoveTo(targetPos);
        //         canMove = true;
        //     }
        //
        //     targetPos = Pos + new Vector2Int(dx, dy);
        //     if (IsCanMoveTo(targetPos))
        //     {
        //         MoveTo(targetPos);
        //         canMove = true;
        //     }
        // }
        // else
        // {
        // Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
        //
        // if (IsCanMoveTo(targetPos))
        // {
        //     MoveTo(targetPos);
        //     canMove = true;
        // }
        // //}
        //
        //
        //
        // tankDirection = direction;
        // Vector3 rotation = Vector3.zero;
        // switch (direction)
        // {
        //     case Direction.Up: rotation = new Vector3(0, 0, 0); break;
        //     case Direction.Down: rotation = new Vector3(0, 0, 180); break;
        //     case Direction.Left: rotation = new Vector3(0, 0, 90); break;
        //     case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        // }
        //
        // // 旋转也用DOTween
        // transform.DORotate(rotation, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.OutQuad);
        // return canMove;
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