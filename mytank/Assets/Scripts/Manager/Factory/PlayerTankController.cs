using System;
using System.Collections.Generic;
using DG.Tweening;
using Tankgame;
using UnityEngine;

public class PlayerTankController : TankController
{
    //击杀数量
    public int killNum = 0;

    //只有玩家有UI
    public PlayerPanelUI playerPanelUI;

    public Dictionary<FoodType, int> foodDict = new Dictionary<FoodType, int>();
    
    public bool isBoat = false;
    public long boatFrame = -1;
    public GameObject boatShow;

    public bool isInvincible = false;
    public long invincibleFrame = -1;
    public Vector3 scaleSize;


    public bool isShootTwice = false;
    public long shootTwiceFrame = -1;
    public bool isShootThree = false;
    public long secondShootFrame = -1;
    public Direction secondDir;
    public Vector2Int secondPos;
    public long threeShootFrame = -1;
    public Direction threeDir;
    public Vector2Int threePos;
    public GameObject shootTwiceShow;

    public bool isBreakWall = false;
    public long breakWallFrame = -1;
    public GameObject breakWallShow;


    public bool isEncourage = false;
    public long encourageFrame = -1;
    public GameObject encourageShow;

    public bool rearFire = false;
    public long rearFireFrame = -1;
    public GameObject rearFireShow;

    public bool isSpikeTrap = false;
    public int spikeTrapDurationFrame = 0;
    public GameObject spikeTrapShow;

    public GameObject shoeShow;

    public Animator tankAnimator2;


    public bool isSpeedKiller;
    public GameObject speedKillerShow;

    public bool isPenetrate;
    public GameObject penetrateShow;

    public bool isChainImplosion;
    public int chainImplosionDamage;
    public int chainImplosionRange;
    public GameObject chainImplosionShow;

    public bool isLegionoftheFallen;

    public override void UpdateFrame()
    {
        base.UpdateFrame();
        HandleBuff();
      
    }

    private void Update()
    {
        if (identity == Identity.Myself && !isDead)
        {
            HandleMovementInput();
            HandleShootInput();
        }
    }

    void HandleBuff()
    {
        if (isBoat)
        {
            if (NetworkManager.Instance.currentFrame >= boatFrame)
            {
                isBoat = false;
                boatShow.SetActive(false);
            }
        }

        if (isEncourage)
        {
            if (NetworkManager.Instance.currentFrame >= encourageFrame)
            {
                isEncourage = false;
                encourageShow.SetActive(false);
            }
        }

        if (isShootTwice)
        {
            if (NetworkManager.Instance.currentFrame >= shootTwiceFrame)
            {
                isShootTwice = false;
                shootTwiceShow.SetActive(false);
            }
        }

        if (threeShootFrame > 0 && NetworkManager.Instance.currentFrame >= threeShootFrame)
        {
            EntityManager.Instance.InitializeBullet(threeDir, this, threePos, bulletDamageNum);
            if (rearFire)
            {
                EntityManager.Instance.InitializeBullet(threeDir.Opposite(), this, threePos, bulletDamageNum);
            }

            threeShootFrame = -1; // 重置
        }

        if (secondShootFrame > 0 && NetworkManager.Instance.currentFrame >= secondShootFrame)
        {
            EntityManager.Instance.InitializeBullet(secondDir, this, secondPos, bulletDamageNum);
            if (rearFire)
            {
                EntityManager.Instance.InitializeBullet(secondDir.Opposite(), this, secondPos, bulletDamageNum);
            }

            secondShootFrame = -1; // 重置
        }

        if (rearFire)
        {
            if (NetworkManager.Instance.currentFrame >= rearFireFrame)
            {
                rearFire = false;
                rearFireShow.SetActive(false);
            }
        }

        if (isBreakWall)
        {
            if (NetworkManager.Instance.currentFrame >= breakWallFrame)
            {
                isBreakWall = false;
                breakWallShow.SetActive(false);
            }
        }

        if (isInvincible)
        {
            if (NetworkManager.Instance.currentFrame >= invincibleFrame)
            {
                transform.localScale = scaleSize;
                isInvincible = false;
            }
        }
    }

    int CurrentMoveIntervalFrame()
    {
        if (isSpeedKiller)
        {
            return (int)(moveIntervalFrame * 0.25f);
        }

        if (isEncourage)
        {
            return (int)(moveIntervalFrame * 0.5f);
        }

        return moveIntervalFrame;
    }

    int CurrentShootIntervalFrame()
    {
        if (isSpeedKiller)
        {
            return (int)(shootIntervalFrame * 0.25f);
        }

        if (isEncourage)
        {
            return (int)(shootIntervalFrame * 0.5f);
        }
        else
        {
            return shootIntervalFrame;
        }
    }

    void HandleMovementInput()
    {
        if (NetworkManager.Instance.currentFrame - lastMoveFrame > CurrentMoveIntervalFrame())
        {
//#if UNITY_STANDALONE_OSX
            if (Input.GetKey(KeyCode.W))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
//#elif UNITY_ANDROID

            if (GameUIManager.Instance.upButton.isPressed)
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (GameUIManager.Instance.downButton.isPressed)
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (GameUIManager.Instance.leftButton.isPressed)
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            else if (GameUIManager.Instance.rightButton.isPressed)
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            
//#endif
            

        }
    }

    void HandleShootInput()
    {
        if (NetworkManager.Instance.currentFrame - lastShootFrame > CurrentShootIntervalFrame())
        {
//#if UNITY_STANDALONE_OSX
            if (Input.GetKey(KeyCode.Space))
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
                lastShootFrame = NetworkManager.Instance.currentFrame;
            }
     //       #elif UNITY_ANDROID
            if (GameUIManager.Instance.shootButton.isPressed)
            {
                NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
                lastShootFrame = NetworkManager.Instance.currentFrame;
            }
//#endif
            
        }
    }


    protected override bool IsCanMoveTo(Vector2Int targetPos)
    {
        List<MapType> allowTypes = new List<MapType> { MapType.floor, MapType.ice, MapType.tree };
        List<MapType> allowTypes1 = new List<MapType> { MapType.floor, MapType.ice, MapType.tree, MapType.river };
        return (isBoat && MapManager.Instance.IsAreaWalkable(targetPos.x, targetPos.y, 2, 2, tankID, allowTypes1))
               || MapManager.Instance.IsAreaWalkable(targetPos.x, targetPos.y, 2, 2, tankID, allowTypes);
    }

    protected override void MoveTo(Vector2Int targetPos)
    {
        if (isSpikeTrap)
        {
            if (targetPos.x == Pos.x && targetPos.y == Pos.y + 1)
            {
                EntityManager.Instance.InitSpikeTrap(this, Pos, spikeTrapDurationFrame);
                EntityManager.Instance.InitSpikeTrap(this, PosRight, spikeTrapDurationFrame);
            }
            else if (targetPos.x == Pos.x && targetPos.y == Pos.y - 1)
            {
                EntityManager.Instance.InitSpikeTrap(this, PosUp, spikeTrapDurationFrame);
                EntityManager.Instance.InitSpikeTrap(this, PosUpRight, spikeTrapDurationFrame);
            }
            else if (targetPos.x == Pos.x + 1 && targetPos.y == Pos.y)
            {
                EntityManager.Instance.InitSpikeTrap(this, Pos, spikeTrapDurationFrame);
                EntityManager.Instance.InitSpikeTrap(this, PosUp, spikeTrapDurationFrame);
            }
            else if (targetPos.x == Pos.x - 1 && targetPos.y == Pos.y)
            {
                EntityManager.Instance.InitSpikeTrap(this, PosRight, spikeTrapDurationFrame);
                EntityManager.Instance.InitSpikeTrap(this, PosUpRight, spikeTrapDurationFrame);
            }
        }
        EntityManager.Instance. UpdateTankPos(this,Pos,targetPos);
        
        Pos = targetPos;
        Vector2 centerPos = GetCenter();
        isMoving = true;
        animator.Play("Tank" + animType);
        lastAnimStartFrame = NetworkManager.Instance.currentFrame;
        transform.DOMove(centerPos, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);
    }

    public override void Shoot()
    {
        EntityManager.Instance.InitializeBullet(tankDirection, this, Pos, bulletDamageNum);
        if (rearFire)
        {
            EntityManager.Instance.InitializeBullet(tankDirection.Opposite(), this, Pos, bulletDamageNum);
        }

        if (isShootThree)
        {
            int intervalFrame = 8;
            threeShootFrame = NetworkManager.Instance.currentFrame + intervalFrame;
            threeDir = tankDirection;
            threePos = Pos;
        }

        if (isShootTwice)
        {
            int intervalFrame = 4;
            secondShootFrame = NetworkManager.Instance.currentFrame + intervalFrame;
            secondDir = tankDirection;
            secondPos = Pos;
        }

        AudioManager.Instance.Play("Shoot");
    }




    public override void AddOrignalHP(int num)
    {
        bombAnimator.Play("Heal", 0, 0);
        orignalHP += num;
        playerPanelUI.UpdateHP(); // 更新血量显示
        tankEntityTankUI.UpdateHealthBar();
        
    }

    public override void AddHP(int num)
    {
        bombAnimator.Play("Heal", 0, 0);
        HP = Mathf.Min(HP + num, orignalHP);
        playerPanelUI.UpdateHP(); // 更新血量显示
        tankEntityTankUI.UpdateHealthBar();
        
      
        
   
    }

    public void DamageHP(int damage)
    {
        if (isInvincible) return;

        HP = Mathf.Max(HP- damage,0);
        DamageUIManager.Instance. ShowDamageWord(DamageType.Bullet,damage,transform);
        tankEntityTankUI.UpdateHealthBar();
        playerPanelUI.UpdateHP(); // 更新血量显示

     
        
    

        if (HP <= 0)
        {
            Dead();
        }
    }

    public void Dead()
    {
        if (isDead) return;
        isDead = true;
        animator.Play("BigBoom");
        AudioManager.Instance.Play("Bomb");
        CamController.Instance.ShakeDead();
        
        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
        Pos = new Vector2Int(-2, -2);
        
    
        

    }

    public void Kill(bool isSpecial)
    {
        killNum++;
        playerPanelUI.UpdateKillCount(1);
        

        
        if (isSpecial && identity == Identity.Myself)
        {
            FoodManager.Instance.AddChooseNum();
        }
    }

    protected override void ExecuteDeathLogic()
    {
        GameStateManager.Instance.GameOver(false);

        transform.position = new Vector2(-100, 0);
    }
}