using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tankgame;
using DG.Tweening;
using UnityEditor.Tilemaps;
using UnityEngine.Serialization;


public enum Identity
{
    Myself,
    OtherPlayer,
    Enemy
}

public class TankController : MonoBehaviour
{

    public TankData currentData;


    public string tankID;
    public Direction tankDirection;

    public Vector2Int Pos;
    // 坐标系统辅助方法
    public Vector2Int GetTopLeft() => new Vector2Int(Pos.x, Pos.y + 1);
    public Vector2Int GetTopRight() => new Vector2Int(Pos.x + 1, Pos.y + 1);
    public Vector2Int GetBottomLeft() => Pos; // 左下角就是Pos
    public Vector2Int GetBottomRight() => new Vector2Int(Pos.x + 1, Pos.y);
    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);
    //击杀数量
    public int killNum=0;
 
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
    //只有玩家有UI
    public PlayerPanelUI playerPanelUI;
    //是否死亡
    public bool isDead;
    
    //本地动画机
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    public System.Random random = new System.Random();

    public int animType = 1;


    
    public bool isBoat = false;
    public long boatFrame = -1;
    
    public bool isInvincible = false;
    public long invincibleFrame=0;

    
    
    
    public bool isShootTwice = false;
    public long shootTwiceFrame = -1;
    
    public long secondShootFrame = -1;
    public Direction secondDir;
    public Vector2Int secondPos;
    
    public bool isBreakWall = false;
    public long breakWallFrame = -1;

    private Vector3 size;
    
    
    public bool isEncourage = false;
    public long encourageFrame = -1;
    
    public bool rearFire=false;
    
    
    private void Awake()
    {
        size=transform.localScale;
    }

     public void UpdateFrame()
    {
        if (invincibleFrame > NetworkManager.Instance.currentFrame)
        {
            transform.localScale =size* 1.5f;
        }
        else
        {
            transform.localScale =size;

        }

        HandleBuff();   
        CheckMovementState();
        if (identity==Identity.Myself && !isDead)
        {
        

            HandleMovementInput();
            HandleShootInput();
        }
        if (identity==Identity.Enemy&&!isDead&&NetworkManager.Instance.currentFrame>=EnemyManager.Instance.pauseEndFrame)
        {
            AiControls();
        }
        
      
    }

    void HandleBuff()
    {
        if (isBoat)
        {
            if (NetworkManager.Instance.currentFrame >= boatFrame)
            {
                isBoat = false;
            }
        }
        if (isEncourage)
        {
            if (NetworkManager.Instance.currentFrame >= encourageFrame)
            {
                isEncourage = false;
            }
        }

        if (isShootTwice)
        {
            
            if (NetworkManager.Instance.currentFrame >= shootTwiceFrame)
            {
                isShootTwice = false;
            }
        }
        if (secondShootFrame > 0 && NetworkManager.Instance.currentFrame >= secondShootFrame)
        {
            BulletFactory.Instance.Initialize(secondDir, this,secondPos);
            if ( rearFire)
            {
                BulletFactory.Instance.Initialize(secondDir.Opposite(), this,secondPos);
            
            }
            secondShootFrame = -1; // 重置
        }

        if (isBreakWall)
        {
            if (NetworkManager.Instance.currentFrame >= breakWallFrame)
            {
                isBreakWall = false;
            }
        }

        if (isInvincible)
        {
            if (NetworkManager.Instance.currentFrame >=invincibleFrame)
            {
                transform.localScale =size;
                isInvincible = false;

            }
        }
        
      
    }
    

    int CurrentMoveIntervalFrame()
    {
        if (isEncourage)
        {
            return (int)(currentData.moveIntervalFrame * 0.7f);
        }
        else
        {

            return currentData.moveIntervalFrame;
        }
    }
    int CurrentShootIntervalFrame()
    {
        if (isEncourage)
        {
            return (int)(currentData.shootIntervalFrame * 0.7f);
        }
        else
        {

            return currentData.shootIntervalFrame;
        }
    }
    void HandleMovementInput()
    {
        if (NetworkManager.Instance.currentFrame - lastMoveFrame >CurrentMoveIntervalFrame())
        {
            if (Input.GetKey(KeyCode.W))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
                lastMoveFrame = NetworkManager.Instance.currentFrame ;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
                lastMoveFrame = NetworkManager.Instance.currentFrame ;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
                lastMoveFrame =NetworkManager.Instance.currentFrame ;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
                lastMoveFrame = NetworkManager.Instance.currentFrame ;
            }
        }
    }

    void HandleShootInput()
    {
        if (NetworkManager.Instance.currentFrame- lastShootFrame > CurrentShootIntervalFrame())
        {
            if (Input.GetKey(KeyCode.Space))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
                lastShootFrame=NetworkManager.Instance.currentFrame ;
            }
        }
    }

    void MoveTo( Vector2Int targetPos)
    {
        
            Pos = targetPos;
            
            // 计算坦克中心位置
            Vector2 centerPos = GetCenter();
            
            isMoving = true;
            animator.Play("Tank"+ animType);
            lastAnimStartFrame = NetworkManager.Instance.currentFrame ;
            transform.DOMove(centerPos, currentData.moveIntervalFrame*Constant.FrameInterval).SetEase(Ease.Linear);
            Landmine a=MapManager.Instance.HasTankInLanemine(Pos.x,Pos.y);
            if (a != null)
            {
                a.Trigger();
            }

            if (identity != Identity.Enemy)
            {
                HealingGarden b=MapManager.Instance.HasTankInGarden(Pos.x,Pos.y);
                if (b != null)
                {
                    b.PickUp(this);
                }
            }
        

    }

    bool isCanMoveTo( Vector2Int targetPos )
    {
        return (isBoat && MapManager.Instance.IsAreaWalkable1(targetPos.x, targetPos.y, 2, 2, tankID))
               || MapManager.Instance.IsAreaWalkable(targetPos.x, targetPos.y, 2, 2, tankID);
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
        bool underice=MapManager.Instance.HasTankInIce(Pos.x,Pos.y);
        if (underice)
        {
            Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
            if (isCanMoveTo(targetPos))
            {
                MoveTo( targetPos);
                canMove = true;
                
            }
           
            targetPos = Pos + new Vector2Int(dx, dy);
            if (isCanMoveTo(targetPos))
            {
                MoveTo( targetPos);
                canMove = true;
                
            }
           
          
        }
        else
        {
            Vector2Int targetPos = Pos + new Vector2Int(dx, dy);
        
            if (isCanMoveTo(targetPos))
            {
                MoveTo( targetPos);
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
        transform.DORotate(rotation, currentData.moveIntervalFrame*Constant.FrameInterval).SetEase(Ease.OutQuad);
        return canMove;
        
    }
  
    public void Shoot()
    {
        BulletFactory.Instance.Initialize(tankDirection,this,Pos);
        if ( rearFire)
        {
            BulletFactory.Instance.Initialize(tankDirection.Opposite(),this,Pos);
            
        }
        if (isShootTwice)
        {
            int intervalFrame = 4;
            secondShootFrame = NetworkManager.Instance.currentFrame + intervalFrame;
            secondDir=tankDirection;
            secondPos = Pos;
        }
    }

    void CheckMovementState()
    {
        if(isDead)return;
        if (isMoving)
        {
            
            
            if (NetworkManager.Instance.currentFrame - lastAnimStartFrame > currentData.moveIntervalFrame)
            {
                isMoving = false;
                animator.Play("Idle" +animType);
            }
        }
        else
        {
            animator.Play("Idle"+animType);
        }
    }

    public void AddHP(int num)
    {
        currentData.HP += num;
        playerPanelUI?.UpdateUI(this); // 更新血量显示
        
    }
    
    public void DamageHP(int damage,TankController attacker)
    {
        if(invincibleFrame>=NetworkManager.Instance.currentFrame)return;
        
        
        currentData.HP -= damage;
        playerPanelUI?.UpdateUI(this); // 更新血量显示
        
        if (currentData.HP <= 0)
        {
            Dead(attacker);
        }
    }

    public void Dead(TankController attacker)
    {
        isDead = true;
        Pos=new Vector2Int(-2,-2);

        GetComponent<SpriteRenderer>().material.color = Color.white;
        animator.Play("BigBoom");
            
       
        DOVirtual.DelayedCall(animTime, () => 
        {
            TankFactory.Instance.TankPool.ReturnObject(this);
            // 如果是敌人，通知EnemyManager
            if (identity==Identity.Enemy)
            {
                EnemyManager.Instance.RemoveEnemy(this);
            }
            else
            {
                PlayerManager.Instance.RemovePlayer(this);
            }

        });
        if (attacker != null)
        {
            
            attacker.Kill(GetComponent<Special>()!=null);
        }
        
        
    }



    public void Kill(bool isSpecial)
    {
        killNum++;
        playerPanelUI?.UpdateUI(this); // 更新血量显示
        if (isSpecial&&identity==Identity.Myself)
        {
            FoodManager.Instance.ShowPanels(this,random);
        }
    }



    void AiControls()
    {
       
    
        
        if (NetworkManager.Instance.currentFrame - lastMoveFrame > currentData.moveIntervalFrame)
        {
            if (!MoveBy(tankDirection))
            {
            
                int a = random.Next(0, 4);
                
                tankDirection = (Direction)a;
                
                // 使用帧时间更新
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else
            {
                // 移动成功
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
        }

        if (NetworkManager.Instance.currentFrame - lastShootFrame > currentData.shootIntervalFrame)
        {
            Shoot();
            lastShootFrame = NetworkManager.Instance.currentFrame;
        }
    }
    
    
    
}

