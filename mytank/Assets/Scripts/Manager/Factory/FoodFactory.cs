
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum FoodType
{
    WarCar,
    PocketWatch,
    Bomb,
    SteelHelmet,
    Shovel,
    Star,
    Pistol
    /*战车 +1h
    怀表 暂停敌方
    炸弹 秒杀全场
    钢盔 10秒无敌
    工兵铲 基地附近变刚
    星星 一次射击两次 
    手枪 二次可打刚*/
}
    public class FoodFactory:SingletonMono<FoodFactory>
    {
       public List<Sprite> foodSprites;
       public FoodController  foodPrefab;
       public ObjectPool<FoodController>  foodPool;
        public Dictionary<FoodType, Sprite> foodToSprite;
        public List<FoodController> activeFood;
       private void Awake()
       {
           foodPool = new ObjectPool<FoodController>(
               prefab:foodPrefab,
               onSpawn:CreateFood,
               onDespawn:KillBullet
           );
           foodToSprite = new Dictionary<FoodType, Sprite>()
           {
               { FoodType.WarCar, foodSprites[0] },
               { FoodType.PocketWatch, foodSprites[1] },
               { FoodType.Bomb, foodSprites[2] },
               { FoodType.SteelHelmet, foodSprites[3] },
               { FoodType.Shovel, foodSprites[4] },
               { FoodType.Star, foodSprites[5] },
               { FoodType.Pistol, foodSprites[6] },
           };
       }

       private void CreateFood(FoodController food)
       {
           food.gameObject.SetActive(true);
           activeFood.Add(food);
       }


    private void KillBullet(FoodController food)
    {
        food.gameObject.SetActive(false);
        activeFood.Remove(food);
    }

    public FoodController Initialize(Random random)
    {
        FoodController food = foodPool.GetObject();
        
    // 假设你的枚举如下

// 获取所有枚举值
    Array values = Enum.GetValues(typeof(FoodType));
    
// 随机选取一个（假设 random 是 System.Random 实例）
        FoodType randomFood = (FoodType)values.GetValue(random.Next(values.Length));
        food.foodType = randomFood;
        //随机到找到一个空地
        List<Vector2Int> emptyPositions = MapManager.Instance.GetEmptyPositions();
        Vector2Int randomPos=emptyPositions[random.Next(emptyPositions.Count)];
            
            
        food.transform.position = new Vector2(randomPos.x,randomPos.y);

        food.Pos = randomPos;
        
        food.GetComponent<SpriteRenderer>().sprite = foodToSprite[randomFood];
        
        
        return food;
    }


    public FoodController HasFoodOn(int x,int y)
    {
        List<Vector2Int> a = new List<Vector2Int>
        {
            new Vector2Int(x, y),
            new Vector2Int(x + 1, y),
            new Vector2Int(x + 1, y + 1),
            new Vector2Int(x, y + 1),
        };
        for (int i = 0; i < activeFood.Count; i++)
        {
            foreach (var pos in a)
            {
                if (activeFood[i].Pos==pos)
                {
                    return activeFood[i];
                }
            }
            
           
        }
        return null;
    }
    
    }
