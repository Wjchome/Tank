using System.Collections.Generic;
using UnityEngine;
using Tankgame;

public class GameStateManager : SingletonMono<GameStateManager>
{
    public Dictionary<string, TankController> playerTanks = new Dictionary<string, TankController>();

    void Start()
    {
        NetworkManager.Instance.OnFrameInputs += OnFrameInputs;
        NetworkManager.Instance.OnGameStart += OnGameStart;
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnFrameInputs -= OnFrameInputs;
            NetworkManager.Instance.OnGameStart -= OnGameStart;
        }
    }

    void OnFrameInputs(FrameInputs frameInputs)
    {
        foreach (var input in frameInputs.Inputs)
        {
           
            // 推进本地状态
            ApplyInputToTank(playerTanks[input.PlayerId], input);
        }
    }



    void ApplyInputToTank(TankController tank, PlayerInput input)
    {
        if (tank == null) return;
        switch (input.InputType)
        {
            case InputType.InputMoveUp:
                tank.MoveBy(0, 1, Direction.Up);
                break;
            case InputType.InputMoveDown:
                tank.MoveBy(0, -1, Direction.Down);
                break;
            case InputType.InputMoveLeft:
                tank.MoveBy(-1, 0, Direction.Left);
                break;
            case InputType.InputMoveRight:
                tank.MoveBy(1, 0, Direction.Right);
                break;
            case InputType.InputShoot:
                tank.Shoot();
                break;
        }
    }

    void OnGameStart(GameStart gameStart)
    {
        Debug.Log($"Game started! Level: {gameStart.Level}");
        
        // 加载关卡
        MapManager.Instance.SetCurrentLevel(gameStart.Level);
        // 创建所有玩家的坦克
        CreateAllPlayerTanks(gameStart);
    }
    

    
    void CreateAllPlayerTanks(GameStart gameStart)
    {
        // 清空现有坦克
        foreach (var tank in playerTanks.Values)
        {
            if (tank != null)
            {
                Destroy(tank.gameObject);
            }
        }
        playerTanks.Clear();
        
        // 为每个玩家创建坦克
        for (int i = 0; i < gameStart.PlayerInfos.Count; i++)
        {
            var playerInfo = gameStart.PlayerInfos[i];
            
            // 获取出生点
            Vector2Int spawnPoint = MapManager.Instance.GetSpawnPoint(i);
            
            // 创建坦克
            GameObject tankObj = Instantiate(GameManager.Instance.tankPrefab, 
                new Vector2(spawnPoint.x, spawnPoint.y), Quaternion.identity);
            TankController tank = tankObj.GetComponent<TankController>();
            tank.Initialize(playerInfo.PlayerId, spawnPoint.x, spawnPoint.y);
            
            // 设置玩家信息
            tank.playerName = playerInfo.PlayerName;
            tank.playerColor = new Color(
                playerInfo.ColorR / 255f,
                playerInfo.ColorG / 255f,
                playerInfo.ColorB / 255f
            );
            
            // 设置坦克颜色
            tank.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
            
            playerTanks[playerInfo.PlayerId] = tank;
            Debug.Log($"Created tank for player {playerInfo.PlayerName} ({playerInfo.PlayerId}) at position ({spawnPoint.x}, {spawnPoint.y})");
        }
    }
    
   
} 