using System.Collections.Generic;
using UnityEngine;
using Tankgame;

public class GameStateManager : SingletonMono<GameStateManager>
{
    public Dictionary<string, TankController> allTanks = new Dictionary<string, TankController>();

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


    
   
} 