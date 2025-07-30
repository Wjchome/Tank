
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class DisciplineController : EffectEntity
    {
        public int targetNum = 1;
        
     
        protected override void ApplyEffect()
        {
            int num = targetNum;
            foreach(var a in EnemyManager.Instance.activeEnemies)
            {
                a.DamageHP(2, tank);
                num--;
                if (num == 0)
                {
                    break;
                }
            }
        }
    }
