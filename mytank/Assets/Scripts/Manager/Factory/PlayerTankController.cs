using System;
using System.Collections.Generic;
using DG.Tweening;
using FixMath.NET;
using QuadTreeV3;
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
    public FixVector2 secondDir;
    public FixRect secondPos;
    public long threeShootFrame = -1;
    public FixVector2 threeDir;
    public FixRect threePos;
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
                EntityManager.Instance.InitializeBullet(-threeDir, this, threePos, bulletDamageNum);
            }

            threeShootFrame = -1; // 重置
        }

        if (secondShootFrame > 0 && NetworkManager.Instance.currentFrame >= secondShootFrame)
        {
            EntityManager.Instance.InitializeBullet(secondDir, this, secondPos, bulletDamageNum);
            if (rearFire)
            {
                EntityManager.Instance.InitializeBullet(-secondDir, this, secondPos, bulletDamageNum);
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

    public override void Update()
    {
        base.Update();
        if (identity == Identity.Myself && !isDead)
        {
            HandleMovementInput();
            HandleShootInput();
        }
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
        Vector2Int moveDir = Vector2Int.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.y += 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDir.y -= 1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x -= 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1;
        }

        if (moveDir != Vector2Int.zero)
        {
            var inputType = moveDir.ToDirection().ToInputType();
            NetworkManager.Instance.SendPlayerInput(inputType);
        }

        // 触屏问题
        // if (GameUIManager.Instance.upButton.isPressed)
        // {
        //     NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
        //     lastMoveFrame = NetworkManager.Instance.currentFrame;
        // }
        // else if (GameUIManager.Instance.downButton.isPressed)
        // {
        //     NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
        //     lastMoveFrame = NetworkManager.Instance.currentFrame;
        // }
        // else if (GameUIManager.Instance.leftButton.isPressed)
        // {
        //     NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
        //     lastMoveFrame = NetworkManager.Instance.currentFrame;
        // }
        // else if (GameUIManager.Instance.rightButton.isPressed)
        // {
        //     NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
        //     lastMoveFrame = NetworkManager.Instance.currentFrame;
        // }
    }

    void HandleShootInput()
    {
        if (NetworkManager.Instance.currentFrame - lastShootFrame > CurrentShootIntervalFrame())
        {
            Vector2 targetPos = Vector2.zero;
            bool shouldShoot = false;

            // 键盘输入：使用鼠标位置作为开火目标
            if (Input.GetMouseButton(0))
            {
                // 将鼠标世界坐标转换为Fix64坐标
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                targetPos = mouseWorldPos;
                shouldShoot = true;
            }

            // // 触屏输入：使用触摸位置作为开火目标
            // if (GameUIManager.Instance.shootButton.isPressed)
            // {
            //     // 使用触摸位置或按钮位置
            //     if (Input.touchCount > 0)
            //     {
            //         Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            //         touchWorldPos.z = 0;
            //         targetPos = touchWorldPos;
            //     }
            //     else
            //     {
            //         // 如果没有触摸，使用鼠标位置
            //         Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //         mouseWorldPos.z = 0;
            //         targetPos = mouseWorldPos;
            //     }
            //
            //     shouldShoot = true;
            // }

            if (shouldShoot)
            {
                // 将Vector2转换为Fix64，然后获取原始值（rawValue）
                Fix64 targetXFix = (Fix64)targetPos.x;
                Fix64 targetYFix = (Fix64)targetPos.y;

                // 获取Fix64的原始值（rawValue），用于网络传输
                long targetX = targetXFix.RawValue;
                long targetY = targetYFix.RawValue;

                NetworkManager.Instance.SendShootRequest(targetX, targetY);
                lastShootFrame = NetworkManager.Instance.currentFrame;
            }
        }
    }


    // protected void MoveTo(Vector2Int targetPos)
    // {
    //     if (isSpikeTrap)
    //     {
    //         if (targetPos.x == Pos.x && targetPos.y == Pos.y + 1)
    //         {
    //             EntityManager.Instance.InitSpikeTrap(this, Pos, spikeTrapDurationFrame);
    //             EntityManager.Instance.InitSpikeTrap(this, PosRight, spikeTrapDurationFrame);
    //         }
    //         else if (targetPos.x == Pos.x && targetPos.y == Pos.y - 1)
    //         {
    //             EntityManager.Instance.InitSpikeTrap(this, PosUp, spikeTrapDurationFrame);
    //             EntityManager.Instance.InitSpikeTrap(this, PosUpRight, spikeTrapDurationFrame);
    //         }
    //         else if (targetPos.x == Pos.x + 1 && targetPos.y == Pos.y)
    //         {
    //             EntityManager.Instance.InitSpikeTrap(this, Pos, spikeTrapDurationFrame);
    //             EntityManager.Instance.InitSpikeTrap(this, PosUp, spikeTrapDurationFrame);
    //         }
    //         else if (targetPos.x == Pos.x - 1 && targetPos.y == Pos.y)
    //         {
    //             EntityManager.Instance.InitSpikeTrap(this, PosRight, spikeTrapDurationFrame);
    //             EntityManager.Instance.InitSpikeTrap(this, PosUpRight, spikeTrapDurationFrame);
    //         }
    //     }
    //
    //     EntityManager.Instance.UpdateTankPos(this, Pos, targetPos);
    //
    //     myFixRect.X = (Fix64)(targetPos.x - MapManager.Instance.gridSize / 2);
    //     myFixRect.Y = (Fix64)(targetPos.y - MapManager.Instance.gridSize / 2);
    //     QuadTreeV3.QuadTreeV3.Instance.UpdateObject(gameObject, myFixRect);
    //
    //     Pos = targetPos;
    //     Vector2 centerPos = GetCenter();
    //     isMoving = true;
    //     animator.Play("Tank" + animType);
    //     lastAnimStartFrame = NetworkManager.Instance.currentFrame;
    //     transform.DOMove(centerPos, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);
    // }

 

    public void ShootAt(long targetX, long targetY)
    {
        
        Fix64 targetXFix = Fix64.FromRaw(targetX);
        Fix64 targetYFix = Fix64.FromRaw(targetY);
        
        FixVector2 targetPos = new FixVector2(targetXFix, targetYFix);
        // 计算从坦克中心到目标位置的方向向量
        FixVector2 directionVec = targetPos - myFixRect.Center;

        // 如果方向向量为零，使用
        if (directionVec.Magnitude() == Fix64.Zero)
        {
            directionVec = FixVector2.Up;
        }
        

        // 使用计算出的方向开火
        FixRect currentRect = myFixRect.ScaleCenter((Fix64)0.5f);
        EntityManager.Instance.InitializeBullet(directionVec, this, currentRect, bulletDamageNum);

        // 处理后向开火
        if (rearFire)
        {
            EntityManager.Instance.InitializeBullet(-directionVec, this, currentRect, bulletDamageNum);
        }

        // 处理三次开火
        if (isShootThree)
        {
            int intervalFrame = 8;
            threeShootFrame = NetworkManager.Instance.currentFrame + intervalFrame;
            threeDir = directionVec;
            threePos = currentRect;
        }

        // 处理二次开火
        if (isShootTwice)
        {
            int intervalFrame = 4;
            secondShootFrame = NetworkManager.Instance.currentFrame + intervalFrame;
            secondDir = directionVec;
            secondPos = currentRect;
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

        HP = Mathf.Max(HP - damage, 0);
        DamageUIManager.Instance.ShowDamageWord(DamageType.Bullet, damage, transform);
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
        QuadTreeV3.QuadTreeV3.Instance.RemoveObject(gameObject);
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
        gameObject.SetActive(false);
        transform.position = new Vector2(-100, 0);
    }
}