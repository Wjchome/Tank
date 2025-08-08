using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


public class EnemyTankController : TankController
{

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
        //AudioManager.Instance.Play("Shoot");
    }

    public override void AddOrignalHP(int num)
    {
        orignalHP += num;
    }

    public override void AddHP(int num)
    {
        HP = Mathf.Min(HP + num, orignalHP);
    }

    public void DamageHP(int damage, PlayerTankController attacker, DamageType damageType)
    {
        HP = Mathf.Max(HP - damage, 0);
        DamageUIManager.Instance.ShowDamageWord(damageType, damage, transform);
        tankEntityTankUI.UpdateHealthBar();
        if (damageType == DamageType.Discipline)
        {
            bombAnimator.Play("DisciplineDamage", 0, 0);
        }

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
        AudioManager.Instance.Play("Bomb");
        CamController.Instance.ShakeDead();

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

            if (attacker.isChainImplosion)
            {
                int range = attacker.chainImplosionRange;

                if (range == 1)
                {
                    bombAnimator.Play("Protect", 0, 0);
                }
                else if (range == 2)
                {
                    bombAnimator.Play("Protect2", 0, 0);
                }


                List<Vector2Int> FindAll()
                {
                    List<Vector2Int> list = new List<Vector2Int>();

                    for (int x = Pos.x - range; x <= Pos.x + range + 1; x++)
                    {
                        for (int y = Pos.y - range; y <= Pos.y + range + 1; y++)
                        {
                            if (MapManager.Instance.IsVailePos(new Vector2Int(x, y)))
                            {
                                list.Add(new Vector2Int(x, y));
                            }
                        }
                    }

                    return list;
                }

                List<Vector2Int> vets = FindAll();

                List<EnemyTankController> tanks
                    = EnemyManager.Instance.activeEnemies.FindAll(a => vets.Contains(a.Pos) || vets.Contains(a.PosUp) ||
                                                                       vets.Contains(a.PosUpRight) ||
                                                                       vets.Contains(a.PosRight));

                foreach (EnemyTankController a in tanks)
                {
                    if (a == this) continue;
                    a.DamageHP(attacker.chainImplosionDamage, attacker, DamageType.Bomb);
                }
            }

            if (attacker.isLegionoftheFallen)
            {
                EntityManager.Instance.InitTankCharge(attacker,true,Pos);
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