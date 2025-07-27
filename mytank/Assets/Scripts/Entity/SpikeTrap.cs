using System.Linq;
using UnityEngine;


    public class SpikeTrap:MonoBehaviour
    {
        public Vector2Int Pos;
        
        public TankController tank;

        public long durationFrame;
        
        public void UpdateFrame()
        {
            if (durationFrame-- < 0)
            {
                Destroy(gameObject);
                return;
            }

            CheckEnemyCollision();
        }
        
        private void CheckEnemyCollision()
        {
            
            
            foreach (TankController enemy in EnemyManager.Instance.activeEnemies)
            {
                if (enemy.Pos == Pos)
                {
                    enemy.DamageHP(1,tank);
                    Destroy(gameObject);
                    
                    break;
                }
                else if(enemy.Pos.x==Pos.x&&enemy.Pos.y+1==Pos.y)
                {
                    enemy.DamageHP(1,tank);
                    Destroy(gameObject);
                    
                    break;
                }
                else if(enemy.Pos.x+1==Pos.x&&enemy.Pos.y+1==Pos.y)
                {
                    enemy.DamageHP(1,tank);
                    Destroy(gameObject);
                    
                    break;
                }
                else if(enemy.Pos.x+1==Pos.x&&enemy.Pos.y==Pos.y)
                {
                    enemy.DamageHP(1,tank);
                    Destroy(gameObject);
                    
                    break;
                }
               
            }
        }
        
    }
