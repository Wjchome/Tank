using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
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
    BulletDeadzone,//脉冲塔
    AutoTurret,
    Landmine,
    HealingGarden,
    RearFire,
    SpikeTrap,
    Protect,
    Discipline,
    GhostGuard,
    TankCharge,//坦克冲锋
    ChainImplosion,//连锁爆炸
    //19个道具

    // 新增合成道具
    AlmightyTurret, // 全能炮台
    SpeedKiller, // 疾速杀手
    LifeEnhancement, // 生命强化
    TimeStorm, // 时间风暴
    PenetratingBullet, // 穿透子弹
    SuperDefense, // 超级守卫
    LegionoftheFallen//死亡军团协议
}

public class FoodManager : SingletonMono<FoodManager>
{
    public List<FoodData> foodDatas;
    public PlayerFoodUI playerFoodUIPrefab;
    
    public Vector2 firstPos;
    public Vector2 secondPos;
    public RectTransform panelParent;
    public Button openOrCloseButton;
    
    public TextMeshProUGUI openOrCloseButtonText;
    public List<FoodUIPanel> panels;
    public Dictionary<FoodType, FoodData> foodDict = new Dictionary<FoodType, FoodData>();

    [Header("食物数值")]
    public List<FoodData> superFoodDatas = new List<FoodData>();
    public List<FoodCombination> foodCombinations = new List<FoodCombination>();
    private Dictionary<FoodType, List<FoodType>> combinationRequirements = new Dictionary<FoodType, List<FoodType>>();

    // 新增状态跟踪
    private bool isFoodPanelActive = false;
    public int chooseNum = 0;
    
    // 新增：延迟显示相关变量
    private bool needDelayedShow = false;
    private long delayedShowFrame = 0;

    public Sprite getSprite0;
    public Sprite getSprite1;

  
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

        secondPos =panelParent.anchoredPosition ;
        firstPos=secondPos+new Vector2(0, 800);
        
       
        openOrCloseButton.onClick.AddListener(() =>
        {
            UIChange.GoLeft(openOrCloseButton,panelParent,firstPos,secondPos);
        });
    }
  
    public void GameStart()
    {
        chooseNum = 0;
        isFoodPanelActive = false;
        needDelayedShow = false;
        delayedShowFrame = 0;
        openOrCloseButtonText.text = "待选：" + chooseNum;
        panelParent.anchoredPosition=secondPos;
    }

    // 新增：外部调用的方法，用于增加选择次数
    public void AddChooseNum(int amount = 1)
    {
        chooseNum += amount;
        openOrCloseButtonText.text = "待选：" + chooseNum;
        
        // 如果面板没有激活且有选择次数，则显示食物选择
        if (!isFoodPanelActive && chooseNum > 0)
        {
            ShowFoodSelection();
        }
    }

    public void UpdateFrame()
    {
        // 检查是否需要延迟显示食物选择
        if (needDelayedShow && NetworkManager.Instance.currentFrame >= delayedShowFrame)
        {
            needDelayedShow = false;
            if (chooseNum > 0 && !isFoodPanelActive)
            {
                ShowFoodSelection();
            }
        }
    }
    
    // 新增：显示食物选择面板
    public void ShowFoodSelection()
    {
        if (chooseNum <= 0 || isFoodPanelActive)
            return;

        isFoodPanelActive = true;
        ShowPanelAndFoods();
    }

    // 修改：私有方法，实际显示食物选择
    private void ShowPanelAndFoods()
    {
        PlayerTankController tank = NetworkManager.Instance.myTank;
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

    private bool IsSuperFoodUnlocked(PlayerTankController tank, FoodType superFoodType)
    {
        if (!combinationRequirements.ContainsKey(superFoodType))
            return false;

        var requirement = combinationRequirements[superFoodType];

        // 检查是否已经拥有这个合成道具（且等级>0）
        if (tank.foodDict.ContainsKey(superFoodType) && tank.foodDict[superFoodType] > 0)
            return false;

        // 检查是否满足合成条件（三个道具都满级）
        return tank.foodDict.ContainsKey(requirement[0]) && tank.foodDict[requirement[0]] >= 3 &&
               tank.foodDict.ContainsKey(requirement[1]) && tank.foodDict[requirement[1]] >= 3 &&
               tank.foodDict.ContainsKey(requirement[2]) && tank.foodDict[requirement[2]] >= 3;
    }

    void SetupPanel(int index, FoodData food, PlayerTankController tank)
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
        if (superFoodDatas.Contains(food))
        {
            panel.foodName.color = Color.red;
        }
        else
        {
            panel.foodName.color = Color.white;
        }

        panel.foodDescription.text = food.foodDescription[num];
        for (int i = 0; i < 3; i++)
        {
            if (i < num)
            {
                panel.isGetImage[i].sprite = getSprite0;
            }
            else
            {
                panel.isGetImage[i].sprite = getSprite1;
            }
        }

        panel.chooseButton.onClick.AddListener(() =>
        {
            // 1. 禁用所有按钮，防止多次点击
            foreach (var p in panels)
                p.chooseButton.interactable = false;
            if(!NetworkManager.Instance.isGameing)
                return;
            // 2. 减少选择次数
            chooseNum--;
            openOrCloseButtonText.text = "待选：" + chooseNum;
            
            // 3. 标记面板为非激活状态
            isFoodPanelActive = false;
            
            // 4. 发送网络请求
            NetworkManager.Instance.SendFrameData(foodId: (int)food.foodType);
            
            // 5. 如果还有选择次数，设置延迟显示
            if (chooseNum > 0)
            {
                needDelayedShow = true;
                delayedShowFrame = NetworkManager.Instance.currentFrame + 2; // 延迟一帧
            }
            else
            {
                openOrCloseButton.onClick.Invoke();
            }
        });
    }
}