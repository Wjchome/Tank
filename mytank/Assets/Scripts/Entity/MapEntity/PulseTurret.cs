using System.Collections.Generic;
using UnityEngine;

public class PulseTurret : MapEntity
{
    public int targetIntervalFrame = 80;
    private long lastTargetFrame;
    public Animator animator;

    public override void UpdateFrame()
    {
        List<Vector2Int> points = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                points.Add(Pos + new Vector2Int(i, j));
            }
        }

        if (NetworkManager.Instance.currentFrame - lastTargetFrame >= targetIntervalFrame)
        {
            animator.Play("Pulse", 0, 0);
            var bullets = EntityManager.Instance.activeBullets.FindAll(bullet => bullet.isPlayerBullet == false);


            foreach (BulletController bullet in bullets)
            {
                if (points.Contains(bullet.Pos) || points.Contains(bullet.Pos + new Vector2Int(0, 1)) ||
                    points.Contains(bullet.Pos + new Vector2Int(1, 1)) ||
                    points.Contains(bullet.Pos + new Vector2Int(1, 0)))
                {
                    bullet.isShouldDestroy = true;
                    bullet.DestroyBullet();
                }
            }

          

            lastTargetFrame = NetworkManager.Instance.currentFrame;
        }
    }
}