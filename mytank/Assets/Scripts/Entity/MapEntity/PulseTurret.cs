using System.Collections.Generic;
using FixMath.NET;
using Physics2D;
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

            var bullets =PhysicsWorld2DComponent.Instance.World.QueryRange(myFixRect.Center, (Fix64)2,
                PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy)|PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletFriend))
                .ConvertAll(a=>a.GetCachedComponent<BulletController>());


            foreach (BulletController bullet in bullets)
            {
                bullet.isDead = true;
                bullet.DestroyBullet();
            }


            lastTargetFrame = NetworkManager.Instance.currentFrame;
        }
    }
}