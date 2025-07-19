using System.Collections.Generic;
using UnityEngine;
using Tankgame;

public class GameStateManager : SingletonMono<GameStateManager>
{
    public Dictionary<string, TankController> playerTanks = new Dictionary<string, TankController>();

    void Start()
    {
        NetworkManager.Instance.OnFrameInputs += OnFrameInputs;
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnFrameInputs -= OnFrameInputs;
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

     public TankController CreatePlayerTank(string playerId)
    {
        GameObject tankObj = Instantiate(GameManager.Instance.tankPrefab, new Vector2(1, 1), Quaternion.identity);
        TankController tank = tankObj.GetComponent<TankController>();
        tank.Initialize(playerId, 3);
        
        // 设置玩家信息
      /*  if (playerId == NetworkManager.Instance.playerID)
        {
            // 本地玩家
            tank.playerName = NetworkManager.Instance.playerName;
            tank.playerColor = NetworkManager.Instance.playerColor;
        }
        else
        {
            // 其他玩家，暂时使用默认值，后续可以从网络消息中获取
            tank.playerName = $"Player_{playerId.Substring(0, 4)}";
            tank.playerColor = Color.red;
        }*/
        
        playerTanks[playerId] = tank;
        return tank;
    }

    void ApplyInputToTank(TankController tank, PlayerInput input)
    {
        if (tank == null) return;
        switch (input.InputType)
        {
            case InputType.InputMoveUp:
                tank.MoveBy(0, 1, "up");
                break;
            case InputType.InputMoveDown:
                tank.MoveBy(0, -1, "down");
                break;
            case InputType.InputMoveLeft:
                tank.MoveBy(-1, 0, "left");
                break;
            case InputType.InputMoveRight:
                tank.MoveBy(1, 0, "right");
                break;
            case InputType.InputShoot:
                tank.Shoot();
                break;
        }
    }
} 