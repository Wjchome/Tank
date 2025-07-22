using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Tankgame;
using TMPro;


public class EnemyManager : SingletonMono<EnemyManager>
{
    
    public int sumEnemies = 0;//总敌人数量
    private List<Vector2Int> tankPawnsPos;
    
    public int maxEnemies = 3; // 场上最大敌人数量
    public float enemySpawnInterval = 5f; // 敌人生成间隔
    private List<TankController> activeEnemies = new List<TankController>();//激活的敌人坦克
    public int leafEnemies = 0;
    [SerializeField]private float lastSpawnTime = 0f;
    
    private int enemyIndex = 0;
    public TextMeshProUGUI enemyleafText;
    public System.Random random;
    
 

    public void LoadLevel(Level levelData)
    {
        sumEnemies = levelData.enemyNum;
        tankPawnsPos=levelData.enemyTankPawns.ToList();
        leafEnemies=sumEnemies;
        lastSpawnTime = 0f;
        enemyleafText.text= leafEnemies.ToString();
        enemyIndex = 0;
        
        // 清理现有敌人
        ClearAllEnemies();
        
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

    private void Update()
    {
        CheckEnemySpawn();
    }

    void CheckEnemySpawn()
    {
        if (activeEnemies.Count >= maxEnemies||enemyIndex>=sumEnemies|!NetworkManager.Instance.isGameing) return;
        
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
   
// 使用确定性随机选择生成点
        int spawnType= random.Next(1, 5);
        var  enemyTank= TankFactory.Instance.Initialize(enemyID, $"{enemyID}", spawnPos.x, spawnPos.y,
            Color.red, false, spawnType);
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
        
        if (leafEnemies == 0)
        {
            GameSuccess();
        }
        
    }

  
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                TankFactory.Instance.TankPool.ReturnObject(enemy);
            }
        }
        activeEnemies.Clear();
    }


    public void GameSuccess()
    {
       GameUIManager.Instance.playerPanelParent.GetComponent<RectTransform>().
           DOAnchorPos( GameUIManager.Instance.secondPos, 0.5f).SetEase(Ease.OutQuad);
       GameUIManager.Instance.gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = "You Win!";
       GameUIManager.Instance.gameOverButton.gameObject.SetActive(true);
            NetworkManager.Instance.isGameing = false;
       
    }

  
    
    
    
    
   
} 