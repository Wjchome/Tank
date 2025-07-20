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
    public string Direction;
    public Vector2Int Pos; // 权威格子坐标
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
    public void Initialize(string playerID, int initialHP = 3)
    {
        PlayerID = playerID;
        HP = initialHP;
        Direction ="up";
        isLocalPlayer = playerID == NetworkManager.Instance.playerID;
        Pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
     
        
        // 确保animator已赋值
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    public void Initialize(string playerID, int x, int y, int initialHP = 3)
    {
        PlayerID = playerID;
        HP = initialHP;
        Direction = "up";
        isLocalPlayer = playerID == NetworkManager.Instance.playerID;
        Pos = new Vector2Int(x, y);
        
        // 设置位置
        transform.position = new Vector2(x, y);
        
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
    public void MoveBy(int dx, int dy, string direction)
    {
        
        // 停止当前动画，防止插值冲突
        transform.DOKill();
        Vector2Int  targetPos=Pos + new Vector2Int(dx, dy);
        if (MapManager.Instance.IsWalkable( targetPos.x,  targetPos.y))
        {
            Pos=targetPos;
            isMoving = true;
            animStartTime=Time.time;
            transform.DOMove((Vector2)targetPos, moveDuration).SetEase(Ease.Linear);
        }
        Direction = direction;
        Vector3 rotation = Vector3.zero;
        switch (direction)
        {
            case "up": rotation = new Vector3(0, 0, 0); break;
            case "down": rotation = new Vector3(0, 0, 180); break;
            case "left": rotation = new Vector3(0, 0, 90); break;
            case "right": rotation = new Vector3(0, 0, -90); break;
        }
        // 旋转也用DOTween
        transform.DORotate(rotation, moveDuration * 0.5f).SetEase(Ease.OutQuad);
    }
  
    public void Shoot()
    {
        GameObject bullet=Instantiate(GameManager.Instance.bulletPrefab, transform.position, transform.rotation);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.Initialize("",Direction,PlayerID);
        
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

