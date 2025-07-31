using System.Collections.Generic;
using Tankgame;
using UnityEngine;

public class TankFactory : SingletonMono<TankFactory>
{
    public TankController tankPrefab; // 改为 TankController 类型
    public ObjectPool<TankController> TankPool { get; private set; }

    public List<TankController> activeTanks;


    public List<TankData> orignalDatas;


    private void Awake()
    {
        TankPool = new ObjectPool<TankController>(
            prefab: tankPrefab,
            onSpawn: CreateTank,
            onDespawn: KillTank
        );
        activeTanks = new List<TankController>();
    }

    private void CreateTank(TankController tank)
    {
        tank.isDead = false;
        tank.lastMoveFrame = 0;
        tank.lastShootFrame = 0;
        tank.lastAnimStartFrame = 0;
        tank.foodDict=new Dictionary<FoodType, int>();
        tank.isBreakWall = false;
        tank.isShootTwice = false;
        tank.invincibleFrame = 0;
        tank.killNum = 0;
        tank.isBoat = false;
        tank.boatFrame = -1;
        tank.boatShow.SetActive(false);
        tank.isInvincible = false;
        tank.invincibleFrame = -1;
        tank.scaleSize = tank.transform.localScale;
        tank.isShootTwice = false;
        tank.shootTwiceFrame = -1;
        tank.isShootThree = false;
        tank.secondShootFrame = -1;
        tank.threeShootFrame = -1;
        tank.shootTwiceShow.SetActive(false);
        tank.isBreakWall = false;
        tank.breakWallFrame = -1;
        tank.breakWallShow.SetActive(false);
        tank.isEncourage = false;
        tank.encourageFrame = -1;
        tank.encourageShow.SetActive(false);
        tank.rearFire = false;
        tank.rearFireFrame = -1;
        tank.rearFireShow.SetActive(false);
        tank.isSpikeTrap = false;
        tank.spikeTrapDurationFrame = 0;
        tank.shoeShow.SetActive(false);

        activeTanks.Add(tank);
        tank.gameObject.SetActive(true);
    }

    private void KillTank(TankController tank)
    {
        tank.gameObject.SetActive(false);
        tank.playerPanelUI = null;
        var a = tank.GetComponent<Special>();
        if (a != null)
        {
            Destroy(a);
        }

        activeTanks.Remove(tank);
    }


    public TankController InitialPlayer(string tankID, string tankName, int x, int y, Color color, int dataIndex)
    {
        TankController temp = TankPool.GetObject();


        temp.tankID = tankID;
        temp.tankDirection = Direction.Up;
        temp.transform.rotation=Quaternion.identity;
        temp.Pos = new Vector2Int(x, y); // 左下角坐标

        if (tankID == NetworkManager.Instance.playerID)
        {
            temp.identity = Identity.Myself;
        }
        else
        {
            temp.identity = Identity.OtherPlayer;
        }

        temp.playerName = tankName;

        // 设置坦克中心位置
        temp.transform.position = temp.GetCenter();
        temp.playerColor = color;
        temp.GetComponent<SpriteRenderer>().material.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);


        temp.currentData = ScriptableObject.CreateInstance<TankData>();
        temp.currentData.InitializeTankData(orignalDatas[0]);


        temp.playerPanelUI =
            Instantiate(GameUIManager.Instance.playerPanelPrefab, GameUIManager.Instance.playerPanelParent)
                .GetComponent<PlayerPanelUI>();
        temp.playerPanelUI.Init(temp, PlayerManager.Instance.activePlayers.Count);

        temp.playerPanelUI.UpdateUI();
        temp.animType = dataIndex;

        PlayerManager.Instance.activePlayers.Add(temp);

        switch (dataIndex)
        {
            case 1:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Shoe);
                break;
            case 2:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.WarCar);
                break;
            case 3:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.HealingGarden);
                break;
            case 4:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Shovel);
                break;
                
        }


        return temp;
    }

      public TankController RevivalPlayer(TankController tank,  int x, int y)
      {

          tank.isDead = false;
        tank.tankDirection = Direction.Up;
        tank.transform.rotation=Quaternion.identity;
        tank.Pos = new Vector2Int(x, y); // 左下角坐标


        // 设置坦克中心位置
        tank.transform.position = tank.GetCenter();

        tank.currentData.HP = tank.currentData.orignalHP;

          tank.playerPanelUI?.UpdateUI();

        return tank;
    }

    
    public TankController InitialEnemy(string tankID, string tankName, int x, int y, int dataIndex)
    {
        TankController temp = TankPool.GetObject();


        temp.tankID = tankID;
        temp.tankDirection = Direction.Up;

        temp.Pos = new Vector2Int(x, y); // 左下角坐标

        temp.identity = Identity.Enemy;
        temp.playerName = tankName;

        // 设置坦克中心位置
        temp.transform.position = temp.GetCenter();
        temp.playerColor = Color.red;
        temp.GetComponent<SpriteRenderer>().material.color = Color.red;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);


        temp.currentData = ScriptableObject.CreateInstance<TankData>();
        temp.currentData.InitializeTankData(orignalDatas[dataIndex]);
        temp.animType = dataIndex;
        EnemyManager.Instance.activeEnemies.Add(temp);


        return temp;
    }

    public TankController InitialSpecialEnemy(string playerID, string playerName, int x, int y,
        int dataIndex)
    {
        TankController temp = TankPool.GetObject();


        temp.tankID = playerID;
        temp.tankDirection = Direction.Up;
        temp.Pos = new Vector2Int(x, y); // 左下角坐标

        temp.identity = Identity.Enemy;

        temp.playerName = playerName;
        temp.playerColor = Color.red;

        // 设置坦克中心位置
        temp.transform.position = temp.GetCenter();

        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);


        temp.currentData = ScriptableObject.CreateInstance<TankData>();
        temp.currentData.InitializeTankData(orignalDatas[dataIndex]);


        temp.animType = dataIndex;

        temp.gameObject.AddComponent<Special>();

        EnemyManager.Instance.activeEnemies.Add(temp);
        return temp;
    }

    public void ClearAllTank()
    {
        // 创建副本，避免遍历时修改集合
        var tanks = new List<TankController>(activeTanks);
        foreach (var tank in tanks)
        {
            TankPool.ReturnObject(tank);
        }
    }
}