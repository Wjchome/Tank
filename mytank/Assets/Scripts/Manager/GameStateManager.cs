using System.Collections.Generic;
using UnityEngine;
using Tankgame;

public class GameStateManager : SingletonMono<GameStateManager>
{
    public Dictionary<string, TankController> allTanks = new Dictionary<string, TankController>();
    public List<BulletController> allBullets = new List<BulletController>();
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
        Debug.Log($"收到帧同步输入，输入数量: {frameInputs.Inputs.Count}");
        foreach (var input in frameInputs.Inputs)
        {
            ApplyInputToTank(allTanks[input.PlayerId], input);
        }
    }


    


    void ApplyInputToTank(TankController tank, PlayerInput input)
    {
        if (tank == null) return;
        switch (input.InputType)
        {
            case InputType.InputMoveUp:
                tank.MoveBy(Direction.Up);
                break;
            case InputType.InputMoveDown:
                tank.MoveBy(Direction.Down);
                break;
            case InputType.InputMoveLeft:
                tank.MoveBy( Direction.Left);
                break;
            case InputType.InputMoveRight:
                tank.MoveBy(Direction.Right);
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
        MapManager.Instance.currentLevel=gameStart.Level;
        MapManager.Instance.LoadLevel(MapManager.Instance.availableLevels[ MapManager.Instance.currentLevel]);
        
     
        // 创建所有玩家的坦克
        CreateAllPlayerTanks(gameStart);
    }
    

    
    void CreateAllPlayerTanks(GameStart gameStart)
    {
        
        
        // 为每个玩家创建坦克
        for (int i = 0; i < gameStart.PlayerInfos.Count; i++)
        {
            var playerInfo = gameStart.PlayerInfos[i];
            
            // 获取出生点
            Vector2Int spawnPoint = MapManager.Instance.GetSpawnPoint(i);
            
            // 创建坦克
       /*     GameObject tankObj = Instantiate(GameManager.Instance.tankPrefab, 
                new Vector2(spawnPoint.x, spawnPoint.y), Quaternion.identity);
            TankController tank = tankObj.GetComponent<TankController>();
            
            
            Color color= new Color(
                playerInfo.ColorR / 255f,
                playerInfo.ColorG / 255f,
                playerInfo.ColorB / 255f
            );
            tank.Initialize(playerInfo.PlayerId,  playerInfo.PlayerName,spawnPoint.x, spawnPoint.y,color,true,0);
         */
       Color color= new Color(
           playerInfo.ColorR / 255f,
           playerInfo.ColorG / 255f,
           playerInfo.ColorB / 255f
       );
       var tank = TankFactory.Instance.Initialize(playerInfo.PlayerId, playerInfo.PlayerName, spawnPoint.x,
           spawnPoint.y, color, true, 0);
            PlayerManager.Instance.activePlayers.Add(tank);
            Debug.Log($"Created tank for player {playerInfo.PlayerName} ({playerInfo.PlayerId}) at position ({spawnPoint.x}, {spawnPoint.y})");
        }
    }
    
   
} 