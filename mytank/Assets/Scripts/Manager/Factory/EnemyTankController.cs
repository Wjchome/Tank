using System.Collections.Generic;
using DG.Tweening;
using FixMath.NET;
using Physics2D;
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
        MoveBy(tankDirection);
        foreach (var enter in rigidBody2D.Enter)
        {
            if (enter.gameObject.CompareTag("Wall") ||
                enter.gameObject.CompareTag("BreakableWall") ||
                enter.gameObject.CompareTag("Ice") ||
                enter.gameObject.CompareTag("Tank"))
            {
                int a = random.Next(0, 8);
                tankDirection = (Direction)a;

            }
        }
        
        if (NetworkManager.Instance.currentFrame - lastShootFrame > shootIntervalFrame)
        {
            Shoot();
            lastShootFrame = NetworkManager.Instance.currentFrame;
        }
    }
    


    public  void Shoot()
    {
        //FixRect currentRect = myFixRect;

       // EntityManager.Instance.InitializeBullet(tankDirection.ToFixVector2().Item1, this, currentRect, bulletDamageNum);
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

   public override void Update()
    {
        base.Update();
        barrel.rotation = body.rotation;
    }
    public void Dead(PlayerTankController attacker)
    {
        if (isDead) return;
        isDead = true;
        animator.Play("BigBoom");
        AudioManager.Instance.Play("Bomb");
        CamController.Instance.ShakeDead();

        deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);
        //QuadTreeV3.QuadTreeV3.Instance.RemoveObject(gameObject);
        
        if (attacker != null)
        {
            if (attacker.isSpeedKiller)
            {
                // EntityManager.Instance.InitSpikeTrap(attacker, Pos, int.MaxValue);
                // EntityManager.Instance.InitSpikeTrap(attacker, PosRight, int.MaxValue);
                // EntityManager.Instance.InitSpikeTrap(attacker, PosUp, int.MaxValue);
                // EntityManager.Instance.InitSpikeTrap(attacker, PosUpRight, int.MaxValue);
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


                // List<Vector2Int> FindAll()
                // {
                //     List<Vector2Int> list = new List<Vector2Int>();
                //
                //     for (int x = Pos.x - range; x <= Pos.x + range + 1; x++)
                //     {
                //         for (int y = Pos.y - range; y <= Pos.y + range + 1; y++)
                //         {
                //             if (MapManager.Instance.IsVailePos(new Vector2Int(x, y)))
                //             {
                //                 list.Add(new Vector2Int(x, y));
                //             }
                //         }
                //     }
                //
                //     return list;
                // }
                //
                // List<Vector2Int> vets = FindAll();
                //
                // List<EnemyTankController> tanks
                //     = EnemyManager.Instance.activeEnemies.FindAll(a => vets.Contains(a.Pos) || vets.Contains(a.PosUp) ||
                //                                                        vets.Contains(a.PosUpRight) ||
                //
                // vets.Contains(a.PosRight));

                // 所有受伤的敌人
                List<EnemyTankController> tanks = new List<EnemyTankController>();
                foreach (EnemyTankController a in tanks)
                {
                    if (a == this) continue;
                    a.DamageHP(attacker.chainImplosionDamage, attacker, DamageType.Bomb);
                }
            }

            // if (attacker.isLegionoftheFallen)
            // {
            //     EntityManager.Instance.InitTankCharge(attacker,true,Pos);
            // }
            attacker.Kill(GetComponent<Special>() != null);
        }
        // EntityManager.Instance. UpdateTankPos(this,Pos,new Vector2Int(-2, -2));
        //
        // Pos = new Vector2Int(-2, -2);
    }

    protected override void ExecuteDeathLogic()
    {
        EnemyManager.Instance.enemyTankPool.ReturnObject(this);
    }
}