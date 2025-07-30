
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BombController : EffectEntity
    {
        
        
        protected override void ApplyEffect()
        {
            EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(2, tank));
            tank.bombAnimator.Play("BombShow", 0, 0);
        }


    }
