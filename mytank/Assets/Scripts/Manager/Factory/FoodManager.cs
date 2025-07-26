
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
   Protect
    
   
   
    
}
    public class FoodManager:SingletonMono<FoodManager>
    {
        public List<FoodData> foodDatas;
      

    public RectTransform panelParent;
    public List<FoodUIPanel> panels;
    

    public void ShowPanels(TankController tank, Random random)
    {
        panelParent.DOAnchorPos(new Vector2(0, 0), 0.5f);

        // 1. 先移除所有监听器，防止重复绑定
        foreach (var panel in panels)
        {
            panel.chooseButton.onClick.RemoveAllListeners();
        }

        // 2. 随机选取不重复的食物
        var availableFoods = new List<FoodData>(foodDatas);
        for (int i = 0; i < 3; i++)
        {
            if (availableFoods.Count == 0) break;
            int foodIndex = random.Next(0, availableFoods.Count);
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
        var panel = panels[index];
        panel.gameObject.SetActive(true);
        panel.chooseButton.interactable = true;
        panel.image.sprite = food.sprite;
        panel.foodName.text = food.foodName;
        panel.foodDescription.text = food.foodDescription;

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

    