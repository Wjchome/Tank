using System;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using UnityEngine.Serialization; // 新增

public class TankController : MonoBehaviour
{
    

    public float moveInterval = 0.05f;
    public float lastMoveTime;
    public float shootInterval = 0.2f;
    public float lastShootTime;
    public float moveDuration = 0.05f; // DOTween动画时长
    public string PlayerID { get; private set; }
    public int HP;
    public Direction Direction;
    public Vector2Int Pos; // 坦克左下角坐标（2x2占地）
    // 坐标系统辅助方法
    public Vector2Int GetTopLeft() => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int GetTopRight() => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int GetBottomLeft() => Pos; // 左下角就是Pos
    public Vector2Int GetBottomRight() => new Vector2Int(Pos.x + 1, Pos.y);
    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);
    
  
    public Animator animator;

    public bool isLocalPlayer;
    public float animStartTime;
    
    // 玩家信息
    public string playerName;
    public Color playerColor;
    
    // 动画控制相关
    public bool isMoving = false;

    public float animSpeed = 0.8f;

    public float animTime = 0.5f;
   
    
    public void Initialize(string playerID, int x, int y, int initialHP = 3)
    {
        PlayerID = playerID;
        HP = initialHP;
        Direction = Direction.Up;
        isLocalPlayer = playerID == NetworkManager.Instance.playerID;
        Pos = new Vector2Int(x, y); // 左下角坐标
        
        // 设置坦克中心位置
        transform.position = GetCenter();
        
        // 确保animator已赋值
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 检查移动状态
        CheckMovementState();
        if (!isLocalPlayer || HP <= 0) return;
        HandleMovementInput();
        HandleShootInput();
        
        
    }

    void HandleMovementInput()
    {
        if (Time.time - lastMoveTime > moveInterval)
        {
            if (Input.GetKey(KeyCode.W))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
                lastMoveTime = Time.time;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
                lastMoveTime = Time.time;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
                lastMoveTime = Time.time;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
                lastMoveTime = Time.time;
            }
        }
    }

    void HandleShootInput()
    {
        if (Time.time - lastShootTime > shootInterval)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
                lastShootTime=Time.time;
            }
        }
    }

    // 帧同步推进调用
    public void MoveBy(int dx, int dy, Direction direction)
    {
        // 停止当前动画，防止插值冲突
        transform.DOKill();
        
        Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
        
        // 检查2x2区域是否可通行
        if (MapManager.Instance.IsAreaWalkable(targetPos.x, targetPos.y, 2, 2,PlayerID))
        {
            Pos = targetPos;
            
            // 计算坦克中心位置
            Vector2 centerPos = GetCenter();
            
            isMoving = true;
            animStartTime = Time.time;
            transform.DOMove(centerPos, moveDuration).SetEase(Ease.Linear);
        }
        
        Direction = direction;
        Vector3 rotation = Vector3.zero;
        switch (direction)
        {
            case Direction.Up: rotation = new Vector3(0, 0, 0); break;
            case Direction.Down: rotation = new Vector3(0, 0, 180); break;
            case Direction.Left: rotation = new Vector3(0, 0, 90); break;
            case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        }
        // 旋转也用DOTween
        transform.DORotate(rotation, moveDuration * 0.5f).SetEase(Ease.OutQuad);
    }
  
    public void Shoot()
    {
        GameObject bullet = Instantiate(GameManager.Instance.bulletPrefab, transform.position, transform.rotation);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.Initialize("", Direction, PlayerID);
        
       
    }

    void CheckMovementState()
    {
        if (isMoving)
        {
                animator.speed = animSpeed;
            
            if (Time.time - animStartTime > moveDuration)
            {
                isMoving = false;
                animator.speed = 0;
            }
        }
        else
        {
            animator.speed = 0;
        }
    }

    public void SetHP(int newHP)
    {
        HP = newHP;
        //UpdateUI(); // 更新血量显示
        
        if (HP <= 0)
        {
            Pos=Vector2Int.zero;
            animator.speed = 1;
            animator.Play("BigBoom");
            Destroy(gameObject, animTime);
        }
    }
}

