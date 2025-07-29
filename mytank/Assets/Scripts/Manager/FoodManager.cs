
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

public enum FoodType
{
    Boat,
    Bomb,
    Encourage,
    Pistol,
    PocketWatch,
    Shoe,
    Shovel,
    Star, 
    SteelHelmet,
    WarCar,
    GenerateWall,
    AutoTurret,
    Landmine,
    HealingGarden,
    RearFire,
    SpikeTrap,
    Protect,
    Discipline//惩戒
    
   
   
    
}
    public class FoodManager:SingletonMono<FoodManager>
    {
        public List<FoodData> foodDatas;
      

    public RectTransform panelParent;
    public List<FoodUIPanel> panels;
    public Dictionary<FoodType, FoodData> foodDict=new Dictionary<FoodType, FoodData>();

    private void Start()
    {
        foreach (var foodData in foodDatas)
        {
            foodDict.Add(foodData.foodType, foodData);
        }
    }

    public void ShowPanels(TankController tank)
    {
        panelParent.DOAnchorPos(new Vector2(0, 0), 0.5f);

        // 1. 先移除所有监听器，防止重复绑定
        foreach (var panel in panels)
        {
            panel.chooseButton.onClick.RemoveAllListeners();
        }

        // 2. 过滤掉已经满级的道具
        var availableFoods = new List<FoodData>();
        foreach (var foodData in foodDatas)
        {
            // 检查坦克是否已经有这个道具，以及是否已经满级
            if (!tank.foodDict.ContainsKey(foodData.foodType) || tank.foodDict[foodData.foodType] < 3)
            {
                availableFoods.Add(foodData);
            }
        }

        // 3. 随机选取不重复的食物
        for (int i = 0; i < 3; i++)
        {
            if (availableFoods.Count == 0) break;
            int foodIndex = UnityEngine.Random.Range(0, availableFoods.Count);
            FoodData randomFood = availableFoods[foodIndex];
            availableFoods.RemoveAt(foodIndex);

            SetupPanel(i, randomFood, tank);
        }
    }

    public void ClosePanels()
    {
        panelParent.DOAnchorPos(new Vector2(0, -800), 0.5f);
        foreach (var panel in panels)
        {
            panel.chooseButton.onClick.RemoveAllListeners();
        }
    }

    void SetupPanel(int index, FoodData food, TankController tank)
    {
        int num=0  ;
        
        if (tank.foodDict.ContainsKey(food.foodType))
        {
            num = tank.foodDict[food.foodType];
        }
        else
        {
            tank.foodDict.Add(food.foodType, 0);
        }
        num=Mathf.Min(num,2);
        var panel = panels[index];
        panel.gameObject.SetActive(true);
        panel.chooseButton.interactable = true;
        panel.image.sprite = food.sprite;
        panel.foodName.text = food.foodName;
        panel.foodDescription.text = food.foodDescription[num];
        for (int i = 0; i < 3; i++)
        {
            if (i < num)
            {
                panel.isGetImage[i].gameObject.SetActive(true);
            }
            else
            {
                panel.isGetImage[i].gameObject.SetActive(false);
            }
        }
        panel.chooseButton.onClick.AddListener(() =>
        {
            // 1. 禁用所有按钮，防止多次点击
            foreach (var p in panels)
                p.chooseButton.interactable = false;

            // 2. 发送网络请求
            NetworkManager.Instance.FoodChooseRequest(food.foodType);

            // 3. 关闭面板
            ClosePanels();
        });
    }
    
    }

    