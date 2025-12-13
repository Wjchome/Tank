using System.Linq;
using UnityEngine;


    public class SpikeTrap : MapEntity
    {
        public long durationFrame;
        
        public int damageNum;
        
        public override void UpdateFrame()
        {
            if (durationFrame-- < 0)
            {
                Destroy();
                return;
            }
            // var enemyTankInArea = MapManager.Instance.GetEnemyTankInArea(Pos.x, Pos.y, 1, 1);
            // if (enemyTankInArea != null&&enemyTankInArea.Count > 0)
            // {
            //     enemyTankInArea.ForEach(enemy=>enemy.DamageHP(damageNum,tank as PlayerTankController,DamageType.SpikeTrap));
            //     Destroy(gameObject);
            // }
        }
        
   
        
    }
