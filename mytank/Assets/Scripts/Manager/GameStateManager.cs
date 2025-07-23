using System.Collections.Generic;
using UnityEngine;
using Tankgame;

public class GameStateManager : SingletonMono<GameStateManager>
{
    

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
            foreach (var tank in PlayerManager.Instance.activePlayers)
            {
                if (tank.tankID == input.PlayerId)
                {
                    ApplyInputToTank(tank, input);
                    
                }
                
            }
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