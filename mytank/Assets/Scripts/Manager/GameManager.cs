using System;
using System.Collections.Generic;
using Tankgame;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public enum GameMode
{
    opponent,
    friend
}

public class GameManager : SingletonMono<GameManager>
{
    [Header("Prefabs")]
    public GameObject tankPrefab;
    public GameObject bulletPrefab;
    
    public bool gameStarted = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public GameMode gameMode=GameMode.opponent;

    public int randseed;
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        NetworkManager.Instance.OnGameStart += OnGameStartFun;
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.OnGameStart -= OnGameStartFun;

    }

    void HandleGameEnd(string winnerID)
    {
        Debug.Log($"Game Over! Winner: {winnerID}");
        
        gameStarted = false;
        
        // 显示游戏结束界面
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (winnerText != null)
            {
                string winnerName = winnerID == NetworkManager.Instance.playerID ? "You" : "Opponent";
                winnerText.text = $"{winnerName} Won!";
            }
        }
    }
    
    void OnGameStartFun(GameStart gameStart)
    {
        randseed = (int)gameStart.RandomSeed;
        // 用服务器下发的随机种子初始化Unity随机数
        UnityEngine.Random.InitState(randseed);
        int index = 0;
        // 随机生成一个敌人坦克d w
        List<Vector2Int> positions = MapManager.Instance.GetCurrentLevel().tankPawns;
        Vector2Int enemyPos =  positions[Random.Range(0, positions.Count)];
        TankController  enemyTank = GameObject.Instantiate(GameManager.Instance.tankPrefab, (Vector2)enemyPos, Quaternion.identity).GetComponent<TankController>();
        enemyTank.Initialize("enemy"+index++,"",enemyPos.x,enemyPos.y, Color.red,false);
      
    }

}
