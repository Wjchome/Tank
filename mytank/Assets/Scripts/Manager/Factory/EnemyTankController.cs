using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum DamageType
{
    Bullet, 
    Landmine,
    SpikeTrap,
    Bomb,
    Discipline,
    Protect,
}



public class EnemyTankController : TankController
{
    public EnemyTankUI enemyTankUI;
    public override void UpdateFrame()
    {
        base.UpdateFrame();
        if (identity == Identity.Enemy && !isDead &&
            NetworkManager.Instance.currentFrame >= EnemyManager.Instance.pauseEndFrame)
        {
            AiControls(); 
        }
    }


    void AiControls()
    {
        
        if (NetworkManager.Instance.currentFrame - lastMoveFrame > moveIntervalFrame)
        {
            if (!MoveBy(tankDirection))
            {
                int a = random.Next(0, 4);
                tankDirection = (Direction)a;
            }

            lastMoveFrame = NetworkManager.Instance.currentFrame;
        }

        if (NetworkManager.Instance.currentFrame - lastShootFrame > shootIntervalFrame)
        {
            Shoot();
            lastShootFrame = NetworkManager.Instance.currentFrame;
        }
    }

    protected override bool IsCanMoveTo(Vector2Int targetPos)
    {
        return MapManager.Instance.IsAreaWalkable(
            targetPos.x, targetPos.y, 2, 2, tankID,
            new List<MapType> { MapType.floor, MapType.tree, MapType.ice });
    }

    protected override void MoveTo(Vector2Int targetPos)
    {
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
    }

    public override void AddOrignalHP(int num)
    {
        orignalHP += num;
    }

    public override void AddHP(int num)
    {
        HP = Mathf.Min(HP + num, orignalHP);
    }

    public void DamageHP(int damage, PlayerTankController attacker,DamageType damageType)
    {
        HP -= damage;
        
        enemyTankUI.UpdateHealthBar();
        
        if (HP <= 0)
        {
            Dead(attacker);
        }
    }

    public void Dead(PlayerTankController attacker)
    {
        if (isDead) return;
        isDead = true;
        animator.Play("BigBoom");
        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
        if (attacker != null)
        {
            if (attacker.isSpeedKiller)
            {
                EntityManager.Instance.InitSpikeTrap(attacker, Pos, int.MaxValue);
                EntityManager.Instance.InitSpikeTrap(attacker, PosRight, int.MaxValue);
                EntityManager.Instance.InitSpikeTrap(attacker, PosUp, int.MaxValue);
                EntityManager.Instance.InitSpikeTrap(attacker, PosUpRight, int.MaxValue);
            }

            attacker.Kill(GetComponent<Special>() != null);
        }

        Pos = new Vector2Int(-2, -2);
    }

    protected override void ExecuteDeathLogic()
    {
        EnemyManager.Instance.enemyTankPool.ReturnObject(this);
    }
}