using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脉冲塔，间隔性清楚区域内敌方子弹
/// </summary>
public class PulseTurret : MapEntity
{
    public int targetIntervalFrame = 80;
    private long lastTargetFrame;
    public Animator animator;

    public override void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame - lastTargetFrame >= targetIntervalFrame)
        {
            animator.Play("Pulse", 0, 0);
            var bullets = EntityManager.Instance.activeBullets.FindAll(bullet => bullet.isPlayerBullet == false);


            foreach (BulletController bullet in bullets)
            {
                bullet.isDead = true;
                bullet.DestroyBullet();
            }


            lastTargetFrame = NetworkManager.Instance.currentFrame;
        }
    }
}