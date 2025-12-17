using DG.Tweening;
using UnityEngine;

/// <summary>
/// 全能炮台
/// </summary>
public class AlmightyTurret : MapEntity
{
    [Header("Shooting Settings")] public int shootIntervalFrame;
    private long lastShootFrame;
    private Direction currentDirection = Direction.Up;
    public int shootDamageNum;

    [Header("Healing Settings")] public int genateIntervalFrame;
    public long lastGenateFrame;
    public bool isCanHealing = false;
    public int healingNum;

    [Header("Combat Settings")] public int damageNum;
    public bool isDead = false;
    public long deadDelayTime;

    public Animator animator;

    public GameObject healGO;

    public override void UpdateFrame()
    {
        if (isDead)
        {
            HandleDeath();
            return;
        }

        HandleShooting();
        HandleHealing();
        HandleEnemyDetection();
    }

    private void HandleDeath()
    {
        if (NetworkManager.Instance.currentFrame >= deadDelayTime)
        {
            Destroy();
        }
    }

    private void HandleShooting()
    {
        if (NetworkManager.Instance.currentFrame - lastShootFrame >= shootIntervalFrame)
        {
            Shoot();
            RotateDirection();
            lastShootFrame = NetworkManager.Instance.currentFrame;
        }
    }

    private void HandleHealing()
    {
        // 检查是否可以开始治疗
        if (NetworkManager.Instance.currentFrame - lastGenateFrame > genateIntervalFrame)
        {
            if (!isCanHealing)
            {
                StartHealing();
            }
        }

        // 执行治疗
        if (isCanHealing)
        {
            // var tanks = MapManager.Instance.GetPlayerTankInArea(Pos.x, Pos.y, 1, 1);
            // if (tanks != null)
            // {
            //     foreach (var _tank in tanks)
            //     {
            //         PickUp(_tank);
            //     }
            // }
        }
    }

    private void HandleEnemyDetection()
    {
        // var enemies = MapManager.Instance.GetEnemyTankInArea(Pos.x, Pos.y, 1, 1);
        // if (enemies != null && enemies.Count > 0)
        // {
        //     Trigger();
        // }
    }

    private void StartHealing()
    {
        isCanHealing = true;
        healGO.SetActive(true);
        lastGenateFrame = NetworkManager.Instance.currentFrame;
    }

    private void RotateDirection()
    {
        currentDirection = (Direction)(((int)currentDirection + 1) % 4);

        Vector3 rotation = Vector3.zero;
        switch (currentDirection)
        {
            case Direction.Up: rotation = new Vector3(0, 0, 0); break;
            case Direction.Down: rotation = new Vector3(0, 0, 180); break;
            case Direction.Left: rotation = new Vector3(0, 0, 90); break;
            case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        }

        transform.DORotate(rotation, shootIntervalFrame * Constant.FrameInterval).SetEase(Ease.OutQuad);
    }

    private void Shoot()
    {
       // EntityManager.Instance.InitializeBullet(currentDirection.ToFixVector2().Item1, tank, tank.myFixRect, shootDamageNum);
    }

    private void Trigger()
    {
        // // 范围伤害
        // var tanks = MapManager.Instance.GetEnemyTankInArea(Pos.x - 1, Pos.y - 1, 3, 3);
        // foreach (var a in tanks)
        // {
        //     a.DamageHP(damageNum, tank as PlayerTankController, DamageType.Landmine);
        // }
        //
        // // 播放爆炸动画
        // animator.Play("Trigger", 0, 0);
        //
        // // 设置死亡状态
        // isDead = true;
        // deadDelayTime = NetworkManager.Instance.currentFrame + (int)(0.33f / Constant.FrameInterval);
    }

    private void PickUp(TankController tank)
    {
        if (isCanHealing)
        {
            tank.AddHP(healingNum);
            isCanHealing = false;
            lastGenateFrame = NetworkManager.Instance.currentFrame;
            healGO.SetActive(false);

        }
    }
}