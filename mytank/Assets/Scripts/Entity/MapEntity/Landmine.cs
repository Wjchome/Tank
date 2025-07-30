using DG.Tweening;
using UnityEngine;


public class Landmine : MapEntity
{
    public Animator animator;

    public override void UpdateFrame()
    {
        var tanks = MapManager.Instance.GetTankInArea(Pos.x, Pos.y, 1, 1);
        if (tanks != null&&tanks.Count>0)
        {
            Trigger();
        }
    }


    public void Trigger()
    {
        var tanks = MapManager.Instance.GetTankInArea(Pos.x - 1, Pos.y - 1, 3, 3);
        foreach (var a in tanks)
        {
            a.DamageHP(1, tank);
        }

        animator.Play("Trigger", 0, 0);
        DOVirtual.DelayedCall(0.33f, Destroy);
    }
}