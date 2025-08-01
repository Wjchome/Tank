using DG.Tweening;
using UnityEngine;


public class Landmine : MapEntity
{
    public Animator animator;

    public int damageNum;

    public bool isDead = false;
    public long deadDelayTime;
    public override void UpdateFrame()
    {
        if (isDead)
        {
            if (NetworkManager.Instance.currentFrame >= deadDelayTime)
            {
                Destroy();
            }
        }
        else
        {
            var tanks = MapManager.Instance.GetTankInArea(Pos.x, Pos.y, 1, 1);
            if (tanks != null&&tanks.Count>0)
            {
                Trigger();
            }
        }

       
    }


    public void Trigger()
    {
        var tanks = MapManager.Instance.GetEnemyTankInArea(Pos.x - 1, Pos.y - 1, 3, 3);
        foreach (var a in tanks)
        {
            a.DamageHP(damageNum, tank);
        }

        animator.Play("Trigger", 0, 0);
        isDead = true;
        deadDelayTime = NetworkManager.Instance.currentFrame + (int)(0.33f / Constant.FrameInterval);
    }
}