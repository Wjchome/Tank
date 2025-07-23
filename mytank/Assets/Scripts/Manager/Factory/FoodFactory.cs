
using System;
using System.Collections.Generic;
using UnityEngine;

public enum FoodType
{
    WarCar,
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

       private void Awake()
       {
           foodPool = new ObjectPool<FoodController>(
               prefab:foodPrefab,
               onSpawn:CreateFood,
               onDespawn:KillBullet
           );
       }

       private void CreateFood(FoodController food)
       {
           food.gameObject.SetActive(true);
       }


    private void KillBullet(FoodController food)
    {
        food.gameObject.SetActive(false);
    }

    public FoodController Initialize()
    {
        FoodController food = foodPool.GetObject();
        
    // 假设你的枚举如下

// 获取所有枚举值
    Array values = Enum.GetValues(typeof(FoodType));
    DeterministicRandom.GetInt
// 随机选取一个（假设 random 是 System.Random 实例）
        FoodType randomProp = (FoodType)values.GetValue(random.Next(values.Length));
        food.foodType = foodType;
        
        food.transform.position = new Vector2(x,y);
        
        food.Pos = new Vector2Int(x, y);
        
        
        
        return food;
    }
    
    
    }
