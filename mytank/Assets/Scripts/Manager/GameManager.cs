using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
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
}
