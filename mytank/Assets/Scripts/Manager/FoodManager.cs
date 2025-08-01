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
    Discipline,
    GhostGuard,

    //19个道具

    // 新增合成道具
    AlmightyTurret, // 全能炮台
    SpeedKiller, // 疾速杀手
    LifeEnhancement, // 生命强化
    TimeController, // 时间控制器
    PenetratingBullet, // 穿透子弹
    SuperDefense // 超级守卫
}

public class FoodManager : SingletonMono<FoodManager>
{
    public List<FoodData> foodDatas;
    public PlayerFoodUI playerFoodUIPrefab;

    public RectTransform panelParent;
    public List<FoodUIPanel> panels;
    public Dictionary<FoodType, FoodData> foodDict = new Dictionary<FoodType, FoodData>();


    public List<FoodData> superFoodDatas = new List<FoodData>();

    public List<FoodCombination> foodCombinations = new List<FoodCombination>();

    // 合成条件配置
    private Dictionary<FoodType, List<FoodType>> combinationRequirements = new Dictionary<FoodType, List<FoodType>>();

    private void Start()
    {
        foreach (var foodData in foodDatas)
        {
            foodDict.Add(foodData.foodType, foodData);
        }

        foreach (var foodData in superFoodDatas)
        {
            foodDict.Add(foodData.foodType, foodData);
        }

        foreach (var foodCombination in foodCombinations)
        {
            combinationRequirements[foodCombination.foodTarget] = new List<FoodType>
            {
                foodCombination.food1,
                foodCombination.food2,
                foodCombination.food3,
            };
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

        // 检查合成道具是否解锁
        foreach (var foodData in superFoodDatas)
        {
            if (IsSuperFoodUnlocked(tank, foodData.foodType))
            {
                availableFoods.Add(foodData);
            }
        }

        if (availableFoods.Count == 0)
        {
            ClosePanels();
            
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

    private bool IsSuperFoodUnlocked(TankController tank, FoodType superFoodType)
    {
        if (!combinationRequirements.ContainsKey(superFoodType))
            return false;

        var requirement = combinationRequirements[superFoodType];

        // 检查是否已经拥有这个合成道具
        if (tank.foodDict.ContainsKey(superFoodType))
            return false;

        // 检查是否满足合成条件（三个道具都满级）
        return tank.foodDict.ContainsKey(requirement[0]) && tank.foodDict[requirement[0]] >= 3 &&
               tank.foodDict.ContainsKey(requirement[1]) && tank.foodDict[requirement[1]] >= 3 &&
               tank.foodDict.ContainsKey(requirement[2]) && tank.foodDict[requirement[2]] >= 3;
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
        int num = 0;

        if (tank.foodDict.ContainsKey(food.foodType))
        {
            num = tank.foodDict[food.foodType];
        }
        else
        {
            tank.foodDict.Add(food.foodType, 0);
        }

        num = Mathf.Min(num, 2);
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