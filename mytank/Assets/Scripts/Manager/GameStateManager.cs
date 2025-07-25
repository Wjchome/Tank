using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Tankgame;
using TMPro;

public class GameStateManager : SingletonMono<GameStateManager>
{
    public void OnFoodsRequest(List<ChooseFoodRequest> foodRequests)
    {
        foreach (var foodRequest in foodRequests)
        {
            foreach (var tank in PlayerManager.Instance.activePlayers.ToList())
            {
                if (tank.tankID == foodRequest.PlayerId)
                {
                    ApplyFoodToTank(tank, foodRequest);
                    
                }
                
            }
        }
    }
    

  

    public void OnFrameInputs(List<PlayerInput> inputs)
    {
        foreach (var input in inputs)
        {
            foreach (var tank in PlayerManager.Instance.activePlayers.ToList())
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

    void ApplyFoodToTank(TankController tank, ChooseFoodRequest foodRequest)
    {
        FoodType foodType = (FoodType)foodRequest.FoodId;
        Debug.LogWarning(foodType.ToString());
    }
    

    public void GameOver(bool isWin)
    {
        GameUIManager.Instance.playerPanelParent.GetComponent<RectTransform>().
            DOAnchorPos(GameUIManager.Instance.secondPos, 0.5f).SetEase(Ease.OutQuad);
        GameUIManager.Instance.gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = isWin?"You Win":"You Lose!";
        GameUIManager.Instance.gameOverButton.gameObject.SetActive(true);
        NetworkManager.Instance.isGameing = false;
            
        //?
        BulletFactory.Instance.ClearAllBullets();
        TankFactory.Instance.ClearAllTank();
            
        PlayerManager.Instance.activePlayers.Clear();
        EnemyManager.Instance.activeEnemies.Clear();
        DOTween.KillAll();
            
        MapManager.Instance.ClearMap();

    }

    
   
} 