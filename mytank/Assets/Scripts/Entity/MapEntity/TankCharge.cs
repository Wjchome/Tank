using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 坦克冲撞
/// </summary>
public class TankCharge : MapEntity
{
    public Direction moveDirection;

    public int moveIntervalFrame = 10;
    private long lastMoveFrame;

    public bool isCanBullet = false;

    public int damageNum;

    public override void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame - lastMoveFrame >= moveIntervalFrame)
        {
            Move();
            
            var enemyTank = EnemyManager.Instance.activeEnemies;
            foreach (var enemyTankController in enemyTank)
            {
                // if (poss.Contains(enemyTankController.Pos) || poss.Contains(enemyTankController.PosUp) ||
                //     poss.Contains(enemyTankController.PosRight) || poss.Contains(enemyTankController.PosUpRight))
                // {
                    enemyTankController.DamageHP(damageNum, tank as PlayerTankController, DamageType.Charge);
               // }
            }

            if (isCanBullet)
            {
                var bullets = EntityManager.Instance.activeBullets;
                foreach (var bulletController in bullets)
                {
                    if (bulletController.isPlayerBullet == false)
                    {
                        // if (poss.Contains(bulletController.Pos) || poss.Contains(bulletController.PosUp) ||
                        //     poss.Contains(bulletController.PosRight) || poss.Contains(bulletController.PosUpRight))
                        // {
                            bulletController.isDead = true;
                            bulletController.DestroyBullet();
                        //}
                    }
                }
            }

            lastMoveFrame = NetworkManager.Instance.currentFrame;
        }
    }


    private void Move()
    {
        int dx = 0, dy = 0;
        switch (moveDirection)
        {
            case Direction.Up:
                dx = 0;
                dy = 1;
                break;
            case Direction.Down:
                dx = 0;
                dy = -1;
                break;
            case Direction.Left:
                dx = -1;
                dy = 0;
                break;
            case Direction.Right:
                dx = 1;
                dy = 0;
                break;
        }
        //
        // Vector2Int targetPos = new Vector2Int(Pos.x + dx, Pos.y + dy);
        // if (!MapManager.Instance.IsVailePos(targetPos))
        // {
        //     Destroy();
        //     return;
        // }
        //
        // Pos = targetPos;
        //
        // // 计算坦克中心位置
        // Vector2 centerPos = GetCenter();

        // isMoving = true;
        //  animator.Play("Tank" + animType);
        // lastAnimStartFrame = NetworkManager.Instance.currentFrame;
        //transform.DOMove(centerPos, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);
    }
}