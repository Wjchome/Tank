using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class GameManager :SingletonMono<GameManager>
{


    [Header("Prefabs")]
    public GameObject tankPrefab;
    public GameObject bulletPrefab;
    
    
    
    
    
    public Dictionary<string, TankController> players = new Dictionary<string, TankController>();
  
    public Dictionary<string, BulletController> bullets = new Dictionary<string, BulletController>();
    
   
    
    public bool gameStarted = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;


    void Start()
    {
        NetworkManager.Instance.OnMessageReceived += HandleNetworkMessage;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
   

    void HandleNetworkMessage(string type, object data)
    {
        JObject jObject = data as JObject;
        
        switch (type)
        {
            case "player_assigned":
                NetworkManager.Instance.playerID = jObject["playerID"].ToString();
                Debug.Log("Assigned Player ID: " + NetworkManager.Instance.playerID);
                break;
                
            case "game_start":
                gameStarted = true;
                var playersData = jObject["players"] as JObject;
                InitializePlayers(playersData);
                Debug.Log("Game Started!");
                break;
                
            case "player_move":
                var moveData = jObject.ToObject<PlayerMoveData>();
                UpdatePlayerPosition(moveData);
                break;
                
            case "bullet_create":
                var bulletData = jObject.ToObject<BulletCreateData>();
                CreateRemoteBullet(bulletData);
                break;
                
            case "bullet_destroy":
                var destroyData = jObject.ToObject<BulletDestroyData>();
                DestroyBullet(destroyData);
                break;
                
            case "player_hit":
                var hitData = jObject.ToObject<PlayerHitData>();
                HandlePlayerHit(hitData);
                break;
                
            case "game_end":
                string winner = jObject["winner"].ToString();
                HandleGameEnd(winner);
                break;
                
            case "player_disconnected":
                string disconnectedID = jObject["playerID"].ToString();
                HandlePlayerDisconnect(disconnectedID);
                break;
        }
    }

    void InitializePlayers(JObject playersData)
    {
        foreach (var entry in playersData)
        {
            string playerId = entry.Key;
            JObject playerObj = entry.Value as JObject;
        
            if (!players.ContainsKey(playerId))
            {
                int x = playerObj["x"].Value<int>();
                int y = playerObj["y"].Value<int>();
                int hp = playerObj["hp"].Value<int>();
                string direction = playerObj["direction"].Value<string>();
            
                GameObject tank = Instantiate(tankPrefab, new Vector2(x, y), Quaternion.identity);
                tank.tag = "Tank";
                TankController controller = tank.GetComponent<TankController>();
                controller.Initialize(playerId, hp);
                players[playerId] = controller;
            
                Debug.Log($"Created tank for player {playerId} at position ({x}, {y})");
            }
        }
    }

    void UpdatePlayerPosition(PlayerMoveData moveData)
    {
        if (moveData.PlayerID == NetworkManager.Instance.playerID)
            return; // 忽略自己的移动消息
            
        if (players.TryGetValue(moveData.PlayerID, out TankController tank))
        {
            tank.MoveTo(moveData.X, moveData.Y, moveData.Direction);
        }
    }

    void CreateRemoteBullet(BulletCreateData bulletData)
    {
        if (bulletData.PlayerID == NetworkManager.Instance.playerID)
            return; // 忽略自己创建的子弹
            
        GameObject bulletObj = Instantiate(
            bulletPrefab, 
            new Vector3(bulletData.X, 0.5f, bulletData.Y), 
            Quaternion.identity
        );
        
        BulletController bullet = bulletObj.GetComponent<BulletController>();
        bullet.Initialize(bulletData.ID, bulletData.Direction, bulletData.PlayerID);
        bullets[bulletData.ID] = bullet;
    }

    void DestroyBullet(BulletDestroyData destroyData)
    {
        if (destroyData.PlayerID == NetworkManager.Instance.playerID)
            return; // 忽略自己销毁的子弹
        
        if (bullets.TryGetValue(destroyData.ID, out BulletController bullet))
        {
            // 如果子弹击中了可破坏墙，同步销毁墙
            Collider[] colliders = Physics.OverlapSphere(
                new Vector3(destroyData.HitX, 0.5f, destroyData.HitY),
                0.5f
            );
        
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("BreakableWall"))
                {
                    Destroy(collider.gameObject);
                    break;
                }
            }
        
            Destroy(bullet.gameObject);
            bullets.Remove(destroyData.ID);
        }
    }

    void HandlePlayerHit(PlayerHitData hitData)
    {
        if (players.TryGetValue(hitData.PlayerID, out TankController tank))
        {
            tank.TakeDamage(hitData.HP);
            
            if (hitData.HP <= 0)
            {
                Debug.Log($"Player {hitData.PlayerID} was eliminated by {hitData.HitByID}");
            }
        }
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

    void HandlePlayerDisconnect(string playerID)
    {
        Debug.Log($"Player {playerID} disconnected");
        
        if (players.TryGetValue(playerID, out TankController tank))
        {
            Destroy(tank.gameObject);
            players.Remove(playerID);
        }
        
        // 如果对方掉线，显示胜利
        if (gameStarted)
        {
            HandleGameEnd(NetworkManager.Instance.playerID);
        }
    }
}
