
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
                
                
               /* case PropType.Star:
                    Upgrade();
                    break;
                case PropType.Helmet:
                    StartCoroutine(Invincible(10f));
                    break;
                case PropType.Clock:
                    EnemyManager.Instance.PauseAllEnemies(5f);
                    break;
                case PropType.Bomb:
                    EnemyManager.Instance.KillAllEnemies();
                    break;
                case PropType.Shovel:
                    MapManager.Instance.UpgradeHome(10f);
                    break;
                case PropType.Gun:
                    FirePowerUp();
                    break;
                case PropType.Ship:
                    EnableRiverPass(10f);
                    break;*/
                // ...
            }
            
            FoodFactory.Instance.foodPool.ReturnObject(this);
        }

        void AddHP(TankController tank )
        {
           tank.AddHP(1);
        }
    }
