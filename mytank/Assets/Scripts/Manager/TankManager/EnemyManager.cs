using System;
using System.Collections.Generic;
using UnityEngine;
using Tankgame;
using TMPro;
using Random = UnityEngine.Random;

public class EnemyManager : SingletonMono<EnemyManager>
{
    
    public int sumEnemies = 0;//总敌人数量
    public int maxEnemies = 3; // 场上最大敌人数量
    public float enemySpawnInterval = 5f; // 敌人生成间隔
    private List<TankController> activeEnemies = new List<TankController>();
    public int leafEnemies = 0;
    private float lastSpawnTime = 0f;
    private List<Vector2Int> tankPawnsPos;
    private int enemyIndex = 0;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI enemyleafText;
    
    void Start()
    {
        NetworkManager.Instance.OnGameStart += OnGameStart;
    }
    
    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnGameStart -= OnGameStart;
        }
    }
    
    void OnGameStart(GameStart gameStart)
    {
        sumEnemies=MapManager.Instance.availableLevels[gameStart.Level].enemyNum;
        leafEnemies=sumEnemies;
        lastSpawnTime = 0f;
        tankPawnsPos = MapManager.Instance.availableLevels[MapManager.Instance.currentLevel].tankPawns;
        levelNameText.text = MapManager.Instance.availableLevels[MapManager.Instance.currentLevel].levelName;
        enemyleafText.text = leafEnemies.ToString();
        
        // 清理现有敌人
        ClearAllEnemies();
        
        // 初始化随机种子
        Random.InitState((int)gameStart.RandomSeed);
        
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

    private void Update()
    {
        CheckEnemySpawn();
    }

    void CheckEnemySpawn()
    {
        if (activeEnemies.Count >= maxEnemies) return;
        
        long currentFrame = NetworkManager.Instance.currentFrame;
        float currentTime = currentFrame * 0.05f; // 每帧0.05秒
        
        if (currentTime - lastSpawnTime >= enemySpawnInterval)
        {
            SpawnEnemy();
            lastSpawnTime = currentTime;
        }
    }
    
    void SpawnEnemy()
    {
       
        
        // 使用确定性随机选择生成点
        int spawnIndex = Random.Range(0, tankPawnsPos.Count);
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
        TankController enemyTank = Instantiate(GameManager.Instance.tankPrefab, 
            new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, 0), 
            Quaternion.identity).GetComponent<TankController>();
        
        enemyTank.Initialize(enemyID, $"{enemyID}", spawnPos.x, spawnPos.y, 
            Color.red, false, 1); // 使用红色，非玩家，数据索引1
        
        activeEnemies.Add(enemyTank);
        
        Debug.Log($"Spawned enemy {enemyID} at position {spawnPos}");
    }
    
    bool IsPositionOccupied(Vector2Int pos)
    {
        // 检查是否有坦克占用2x2区域
        return MapManager.Instance.GetTankInArea(pos.x, pos.y, 2, 2) != null;
    }
    
    public void RemoveEnemy(TankController enemy)
    {
       
        activeEnemies.Remove(enemy);
        leafEnemies--;
        enemyleafText.text= leafEnemies.ToString();
        
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
    }
    
   
} 