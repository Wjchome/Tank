using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Tankgame;
using TMPro;


public class EnemyManager : SingletonMono<EnemyManager>
{
    
    public EnemyTankController enemyTankPrefab; // 改为 TankController 类型
    public ObjectPool<EnemyTankController > TankPool { get; private set; }

    public List<EnemyTankController > activeTanks;


    public List<TankData> orignalDatas;

    
    public int sumEnemies = 0;//总敌人数量
    private List<Vector2Int> tankPawnsPos;
    
    public int maxEnemies = 3; // 场上最大敌人数量
    public int enemySpawnInterval = 200; // 敌人生成间隔
    public List<EnemyTankController> activeEnemies = new List<EnemyTankController>();//激活的敌人坦克
    public int leafEnemies = 0;
    private long lastSpawnFrame = 0;
    
    private int enemyIndex = 0;
    public TextMeshProUGUI enemyleafText;
    public System.Random random;

    public int specialRange = 5;

    
    
    public long pauseEndFrame=0;
    
    
    
    

    public void LoadLevel(Level levelData)
    {
        sumEnemies = levelData.enemyNum;
        tankPawnsPos=levelData.enemyTankPawns.ToList();
        leafEnemies=sumEnemies;
        lastSpawnFrame = 0;
        enemyleafText.text= leafEnemies.ToString();
        enemyIndex = 0;
        pauseEndFrame = 0;
        // 清理现有敌人
       // ClearAllEnemies();
        
        // 初始化随机种子
        random = new System.Random((int)NetworkManager.Instance.seed);
        
        // 生成初始敌人
        SpawnInitialEnemies();
    }
  
    void SpawnInitialEnemies()
    {

        
        int initialEnemyCount =2;
        
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    public void UpdateFrame()
    {
        CheckEnemySpawn();
        activeTanks.ToList().ForEach(a=>a.UpdateFrame());
    }

    void CheckEnemySpawn()
    {
        if (activeEnemies.Count >= maxEnemies||enemyIndex>=sumEnemies|!NetworkManager.Instance.isGameing) return;
        
     
        if ( NetworkManager.Instance.currentFrame- lastSpawnFrame >= enemySpawnInterval)
        {
            SpawnEnemy();
            lastSpawnFrame = NetworkManager.Instance.currentFrame;
        }
    }
    
    void SpawnEnemy()
    {
       
        
        // 使用确定性随机选择生成点
        int spawnIndex = random.Next(0, tankPawnsPos.Count);
        Vector2Int spawnPos = tankPawnsPos[spawnIndex];
        
        // 检查生成点是否被占用
        if (IsPositionOccupied(spawnPos))
        {
            // 尝试其他生成点
            for (int i = 0; i < tankPawnsPos.Count; i++)
            {
                int tryIndex = (spawnIndex + i) % tankPawnsPos.Count;
                Vector2Int tryPos = tankPawnsPos[tryIndex];
                if (!IsPositionOccupied(tryPos))
                {
                    spawnPos = tryPos;
                    break;
                }
            }
            
            // 如果所有生成点都被占用，放弃生成
            if (IsPositionOccupied(spawnPos))
            {
                return;
            }
        }
        
        // 创建敌人坦克
        string enemyID = $"enemy_{enemyIndex++}";
   
        // 使用确定性随机选择生成坦克类型
        int spawnType= random.Next(1, 5);
        //随机坦克是否特殊
        int a = random.Next(0, specialRange);
      
        if (a == 0)
        {
            InitialSpecialEnemy(enemyID, $"{enemyID}", spawnPos.x, spawnPos.y,
                 spawnType);
        }
        else
        {
            InitialEnemy(enemyID, $"{enemyID}", spawnPos.x, spawnPos.y,
                spawnType);
        }
        
    }
    
    bool IsPositionOccupied(Vector2Int pos)
    {
        // 检查是否有坦克占用2x2区域
        List<TankController> tanks = MapManager.Instance.GetTankInArea(pos.x, pos.y, 2, 2);
        return  tanks.Count>0;
    }
    
    public void RemoveEnemy(EnemyTankController enemy)
    {
       
        activeEnemies.Remove(enemy);
        leafEnemies--;
        enemyleafText.text= leafEnemies.ToString();
        
        if (leafEnemies == 0)
        {
            GameStateManager.Instance.GameOver(true);

        }
        
    }

  


    private void Awake()
    {
        TankPool = new ObjectPool<EnemyTankController >(
            prefab: enemyTankPrefab,
            onSpawn: CreateTank,
            onDespawn: KillTank
        );
        activeTanks = new List<EnemyTankController >();
    }

    private void CreateTank(EnemyTankController  tank)
    {
        tank.isDead = false;
        tank.lastMoveFrame = 0;
        tank.lastShootFrame = 0;
        tank.lastAnimStartFrame = 0;
   

        activeTanks.Add(tank);
        tank.gameObject.SetActive(true);
    }

    private void KillTank(EnemyTankController  tank)
    {
        tank.gameObject.SetActive(false);
        var a = tank.GetComponent<Special>();
        if (a != null)
        {
            Destroy(a);
        }

        activeTanks.Remove(tank);
    }




    
    public EnemyTankController InitialEnemy(string tankID, string tankName, int x, int y, int dataIndex)
    {
        EnemyTankController temp = TankPool.GetObject();


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

    public EnemyTankController InitialSpecialEnemy(string playerID, string playerName, int x, int y,
        int dataIndex)
    {
        EnemyTankController temp = TankPool.GetObject();


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




  
    
    
    
    
   
} 