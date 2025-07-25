/*
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class FoodController:MonoBehaviour
    {
        public FoodType foodType;
        
        public Vector2Int Pos;
        
        public void OnPropPickup(TankController tank)
        {
            switch ( foodType)
            {
                case FoodType.WarCar:
                    AddHP(tank);
                    break;
                case FoodType.PocketWatch:
                   StopAllEnemy();
                    break;
                case FoodType.Bomb:
                    KillAllEnemies(tank);
                    break;
                case FoodType.SteelHelmet:
                    Invincible(tank);
                    break;
                case FoodType.Shovel:
                    BecomeIron();
                    break;
                case FoodType.Star:
                    ShootTwice(tank);
                    break;              
                case FoodType.Pistol:
                    CanBreakWall(tank);
                    break;

  
            }
            
            FoodFactory.Instance.foodPool.ReturnObject(this);
        }

        void AddHP(TankController tank )
        {
           tank.AddHP(1);
        }

        void StopAllEnemy()
        {
            
            int targetTime = 10;
            long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(targetTime/Constant.FrameInterval);
         
            EnemyManager.Instance.pauseEndFrame=targetFrame;
            
        }

        void KillAllEnemies(TankController tank)
        {
            foreach (var enemy in EnemyManager.Instance.activeEnemies.ToList())
            {
                enemy.Dead(tank);
            }

        }

        void Invincible(TankController tank)
        {
            int targetTime = 10;
            long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(targetTime/Constant.FrameInterval);
         
            tank.invincibleFrame=targetFrame;

        }

        void BecomeIron()
        {
            MapManager.Instance.isChange = true;
            int targetTime = 10;
            long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(targetTime/Constant.FrameInterval);
            MapManager.Instance.ironWallEndFrames=targetFrame;
            
                List<Vector2Int> pos = new List<Vector2Int>
                {
                    new Vector2Int(12, 1),
                    new Vector2Int(13, 1),
                    new Vector2Int(12, 2),
                    new Vector2Int(13, 2),
                };
                for (int i = 10; i <= 15; i++)
                {
                    for (int j = 1; j <= 4; j++)
                    {
                        Vector2Int pos1 = new Vector2Int(i, j);
                        if(!pos.Contains(pos1))
                            MapManager.Instance.SetWallType(i, j, MapType.wall);
                    }
                }
            }

        void ShootTwice(TankController tank)
        {
            tank.isCanShootTwice = true;
        }

        void CanBreakWall(TankController tank)
        {
            tank.isCanBreakWall = true;
        }
        
        
        
    }
*/