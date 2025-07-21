using System;
using System.Collections.Generic;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random; // 新增



public class TankController : MonoBehaviour
{

    public TankData currentData;


    public string PlayerID;
    public Direction TankDirection;

    public Vector2Int Pos;
    // 坐标系统辅助方法
    public Vector2Int GetTopLeft() => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int GetTopRight() => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int GetBottomLeft() => Pos; // 左下角就是Pos
    public Vector2Int GetBottomRight() => new Vector2Int(Pos.x + 1, Pos.y);
    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);
    //击杀数量
    public int killNum=0;
    
    //是否是本机玩家
    public bool IsLocalPlayer;
    //一些计时器
    public float lastMoveTime;
    public float lastShootTime;
    public float lastAnimStartTime;
    
    // 玩家自选名字
    public string playerName;
    //玩家自选颜色
    public Color playerColor;
    //是否是玩家 ，false代表是敌人ai
    public bool isPlayer;
    
    // 控制动画
    public bool isMoving = false;
    //动画速度
    public float animSpeed = 0.8f;
    //死亡动画时间
    public float animTime = 0.5f;
    //只有玩家有UI
    public PlayerPanelUI playerPanelUI;
    //是否死亡
    public bool isDead;
    
    //本地动画机
    public Animator animator;
    

    void Update()
    {
        // 检查移动状态
        CheckMovementState();
        if (IsLocalPlayer && !isDead)
        {
        

            HandleMovementInput();
            HandleShootInput();
        }
        if (!isPlayer)
        {
            AiControls();
        }
    }

    void HandleMovementInput()
    {
        if (Time.time - lastMoveTime > currentData.moveInterval)
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
        if (Time.time - lastShootTime > currentData.shootInterval)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
                lastShootTime=Time.time;
            }
        }
    }

    // 帧同步推进调用
    public bool MoveBy( Direction direction)
    {
        bool canMove = false;
        int dx = 0, dy = 0;
        switch (direction)
        {
            case Direction.Up:
                dx = 0;dy = 1;
                break;
            case Direction.Down:
                dx = 0;dy = -1;
                break;
            case Direction.Left:
                dx = -1;dy = 0;
                break;
            case Direction.Right:
                dx = 1;dy = 0;
                break;
        }
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
            lastAnimStartTime = Time.time;
            transform.DOMove(centerPos, currentData.moveInterval).SetEase(Ease.Linear);
            canMove = true;
        }
        
        TankDirection = direction;
        Vector3 rotation = Vector3.zero;
        switch (direction)
        {
            case Direction.Up: rotation = new Vector3(0, 0, 0); break;
            case Direction.Down: rotation = new Vector3(0, 0, 180); break;
            case Direction.Left: rotation = new Vector3(0, 0, 90); break;
            case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        }
        // 旋转也用DOTween
        transform.DORotate(rotation, currentData.moveInterval).SetEase(Ease.OutQuad);
        return canMove;
    }
  
    public void Shoot()
    {
        BulletFactory.Instance.Initialize(TankDirection,PlayerID,isPlayer);
       
    }

    void CheckMovementState()
    {
        if (isMoving)
        {
                animator.speed = animSpeed;
            
            if (Time.time - lastAnimStartTime > currentData.moveInterval)
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

    public void DamageHP(int damage,string attackerID)
    {
        currentData.HP -= damage;
        playerPanelUI?.UpdateUI(this); // 更新血量显示
        
        if (currentData.HP <= 0)
        {
            isDead = true;
            Pos=Vector2Int.zero;
            animator.speed = 1;
            animator.Play("BigBoom");
            
            // 如果是敌人，通知EnemyManager
            if (!isPlayer)
            {
                EnemyManager.Instance.RemoveEnemy(this);
            }
            
            TankFactory.Instance.TankPool.ReturnObject(this);
            GameStateManager.Instance.allTanks[attackerID].Kill();
        }
    }

    public void Kill()
    {
        killNum++;
        playerPanelUI?.UpdateUI(this); // 更新血量显示
        
    }



    void AiControls()
    {
        // 使用帧数进行时间判断，确保所有客户端同步
        long currentFrame = NetworkManager.Instance.currentFrame;
        float frameTime = currentFrame * 0.05f; // 每帧0.05秒
        
        if (frameTime - lastMoveTime > currentData.moveInterval)
        {
            if (!MoveBy(TankDirection))
            {
                // 使用帧数和PlayerID生成确定性随机数
                int seed = (int)(currentFrame * 1000 + PlayerID.GetHashCode());
                Random.InitState(seed);
                int a = Random.Range(0, 4);
                
                TankDirection = (Direction)a;
                
                // 使用帧时间更新
                lastMoveTime = frameTime;
            }
            else
            {
                // 移动成功
                lastMoveTime = frameTime;
            }
        }

        if (frameTime - lastShootTime > currentData.shootInterval)
        {
            Shoot();
            lastShootTime = frameTime;
        }
    }
    
    
    
}

