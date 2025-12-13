using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FixMath.NET;
using QuadTreeV3;
using UnityEngine;
using Tankgame;
using TMPro;
using UnityEngine.Serialization;


public class EnemyManager : SingletonMono<EnemyManager>
{
    public EnemyTankController enemyTankPrefab; // 改为 TankController 类型
    public ObjectPool<EnemyTankController> enemyTankPool { get; private set; }
    public List<EnemyTankController> activeEnemies;


    public List<TankData> orignalDatas;


    private List<Vector2Int> tankPawnsPos;


    public int sumEnemies = 0; //总敌人数量
    public int maxEnemies; // 场上最大敌人数量
    public int enemySpawnInterval; // 敌人生成间隔
    public int specialRange;

    public int leafEnemies = 0;
    private long lastSpawnFrame = 0;

    private int enemyIndex = 0;
    public TextMeshProUGUI enemyleafText;
    public System.Random random;


    public Dictionary<EnemyEvent, int> enemyEvents = new Dictionary<EnemyEvent, int>();

    public Dictionary<int, EnemyEvent> enemyUp = new Dictionary<int, EnemyEvent>();


    public long pauseEndFrame = 0;


    private void Awake()
    {
        enemyTankPool = new ObjectPool<EnemyTankController>(
            prefab: enemyTankPrefab,
            onSpawn: CreateTank,
            onDespawn: KillTank
        );
        activeEnemies = new List<EnemyTankController>();
    }

    private void CreateTank(EnemyTankController tank)
    {
        tank.isDead = false;
        tank.lastShootFrame = 0;

        activeEnemies.Add(tank);
        tank.gameObject.SetActive(true);
        EntityManager.Instance.allTanks.Add(tank);
    }

    private void KillTank(EnemyTankController tank)
    {
        tank.gameObject.SetActive(false);
        leafEnemies--;
        enemyleafText.text = "剩余：" + leafEnemies.ToString();
        var a = tank.GetComponent<Special>();
        if (a != null)
        {
            Destroy(a);
        }

        EntityManager.Instance.allTanks.Remove(tank);
        activeEnemies.Remove(tank);
        if (leafEnemies == 0)
        {
            GameStateManager.Instance.GameOver(true);
        }
    }


    public void LoadLevel(Level levelData)
    {
        sumEnemies = levelData.enemySum;
        tankPawnsPos = levelData.enemyTankPawns.ToList();
        maxEnemies = levelData.maxEnemies;
        enemySpawnInterval = levelData.enemySpawnFrame;
        specialRange = levelData.specialRate;
        enemyUp = levelData.GetDic();
        enemyEvents = new Dictionary<EnemyEvent, int>()
        {
            { EnemyEvent.Move_Interval_1, 0 },
            { EnemyEvent.HP_Add_20, 0 },
            { EnemyEvent.Shoot_Interval_1, 0 },
        };
        leafEnemies = sumEnemies;
        lastSpawnFrame = 0;
        enemyleafText.text = "剩余：" + leafEnemies.ToString();
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
        int initialEnemyCount = 2;

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    public void UpdateFrame()
    {
        CheckEnemySpawn();
        activeEnemies.ToList().ForEach(a => a.UpdateFrame());
    }

    void CheckEnemySpawn()
    {
        if (activeEnemies.Count >= maxEnemies || enemyIndex >= sumEnemies | !NetworkManager.Instance.isGameing) return;


        if (NetworkManager.Instance.currentFrame - lastSpawnFrame >= enemySpawnInterval)
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

        if (enemyUp.TryGetValue(enemyIndex, out var enemyEvent))
        {
            if (enemyEvents.ContainsKey(enemyEvent))
            {
                enemyEvents[enemyEvent]++;
            }
            else
            {
                enemyEvents.Add(
                    enemyEvent, 1
                );
            }

            if (enemyEvent == EnemyEvent.Spawn_Interval_80)
            {
                enemySpawnInterval = (int)(enemySpawnInterval * 0.8f);
            }
        }

        // 使用确定性随机选择生成坦克类型
        int spawnType = random.Next(1, 5);
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
        var res = QuadTreeV3.QuadTreeV3.Instance.Query(new FixRect((Fix64)pos.x -Constant.tankSize, (Fix64)pos.y - Constant.tankSize,
            Constant.tankSize * (Fix64)2, Constant.tankSize * (Fix64)2),
            QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankEnemy) |
            QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankFriend));
        return res.Count > 0;
    }


    public EnemyTankController InitialEnemy(string tankID, string tankName, int x, int y, int dataIndex)
    {
        EnemyTankController temp = enemyTankPool.GetObject();


        temp.tankID = tankID;
        int dir = random.Next(0, 8);

        temp.tankDirection = (Direction)dir;


        temp.identity = Identity.Enemy;
        temp.playerName = tankName;

  
        FixRect fixRect = new FixRect((Fix64)x -Constant.tankSize, (Fix64)y -Constant.tankSize,
            Constant.tankSize * (Fix64)2, Constant.tankSize * (Fix64)2);
        temp.myFixRect = fixRect;
        //temp.PosF = new FixVector2((Fix64)(x + MapManager.Instance.gridSize / 2), (Fix64)(y + MapManager.Instance.gridSize / 2));
        QuadTreeLayer layer =
            QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankEnemy);

        QuadTreeV3.QuadTreeV3.Instance.AddObject(fixRect, temp.gameObject, layer);

        temp.transform.position = new Vector2((float)fixRect.CenterX, (float)fixRect.CenterY);


        temp.playerColor = Color.red;
        temp.spriteRenderer.color = Color.red;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);

        TankData data = orignalDatas[dataIndex];
        InitialEnemyData(temp, data);

        temp.animType = dataIndex;
        temp.tankEntityTankUI.UpdateHealthBar();

        EntityManager.Instance.UpdateTankPos(temp, new Vector2Int(-2, -2), new Vector2Int(x, y));

        return temp;
    }


    public EnemyTankController InitialSpecialEnemy(string playerID, string playerName, int x, int y, int dataIndex)
    {
        EnemyTankController temp = enemyTankPool.GetObject();

        temp.tankID = playerID;
        int dir = random.Next(0, 4);
        temp.tankDirection = (Direction)dir;

        temp.identity = Identity.Enemy;

        temp.playerName = playerName;
        temp.playerColor = Color.red;

        // 设置坦克中心位置

        
        FixRect fixRect = new FixRect((Fix64)x - Constant.tankSize, (Fix64)y - Constant.tankSize,
            Constant.tankSize * (Fix64)2, Constant.tankSize * (Fix64)2);
        temp.myFixRect = fixRect;
        QuadTreeLayer layer =
            QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankEnemy);
        QuadTreeV3.QuadTreeV3.Instance.AddObject(fixRect, temp.gameObject, layer);
        temp.transform.position = (Vector2)fixRect.Center;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);

        TankData data = orignalDatas[dataIndex];
        InitialEnemyData(temp, data);


        temp.animType = dataIndex;
        temp.tankEntityTankUI.UpdateHealthBar();

        var a= temp.gameObject.AddComponent<Special>();
        a.spriteRenderer= temp.spriteRenderer;
        EntityManager.Instance.UpdateTankPos(temp, new Vector2Int(-2, -2), new Vector2Int(x, y));


        return temp;
    }


    void InitialEnemyData(EnemyTankController temp, TankData data)
    {
        //temp.moveIntervalFrame = data.moveIntervalFrame - enemyEvents[EnemyEvent.Move_Interval_1];
        temp.shootIntervalFrame = data.shootIntervalFrame - enemyEvents[EnemyEvent.Shoot_Interval_1];
        temp.orignalHP = (int)(data.orignalHP * Mathf.Pow(1.2f, enemyEvents[EnemyEvent.HP_Add_20]));
        temp.HP = temp.orignalHP;
    }
}