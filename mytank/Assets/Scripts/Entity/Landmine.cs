using DG.Tweening;
using UnityEngine;


public class Landmine : MonoBehaviour
{
    public Vector2Int Pos;
    public TankController tank;
    public Animator animator;

    public void UpdateFrame()
    {
        var tanks = MapManager.Instance.GetTankInArea(Pos.x, Pos.y, 1, 1);
        if (tanks != null)
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
        animator.Play("Trigger",0,0);
        DOVirtual.DelayedCall(0.33f, () =>
                Destroy(this.gameObject))
            ;
    }
}