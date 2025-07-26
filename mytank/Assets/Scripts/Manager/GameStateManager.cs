using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Tankgame;
using TMPro;

public class GameStateManager : SingletonMono<GameStateManager>
{

    public GameObject autoTurretPrefab;
    
    public GameObject landminePrefab;
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

        switch (foodType)
        {
            case FoodType.Boat:
                tank.isBoat = true;
                tank.boatFrame=NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
                break;
            case FoodType.Bomb:
                EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(1, tank));
                break;
            case FoodType.Encourage:
                tank.isEncourage = true;
                tank.encourageFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
                break;
            case FoodType.Pistol:
                tank.isShootTwice = true;   
                tank.shootTwiceFrame=NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
                break;
            case FoodType.PocketWatch:
                long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                EnemyManager.Instance.pauseEndFrame=targetFrame;
                break;
            case FoodType.Shoe: 
                tank.currentData.moveIntervalFrame = Mathf.Max(1, tank.currentData.moveIntervalFrame - 1);
                break;
            case FoodType.Shovel:
                MapManager.Instance.isChange = true;
                MapManager.Instance.ironWallEndFrames=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                MapManager.Instance.aaa(MapType.wall);
                break;
            case FoodType.Star:
                tank.isBreakWall = true;
                tank.breakWallFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                break;
            case FoodType.SteelHelmet:
                tank.isInvincible = true;
                tank.invincibleFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(5/Constant.FrameInterval);
                break;
            case FoodType.WarCar:
                tank.AddHP(1);
                break;
            case FoodType.GenerateWall:
                MapManager.Instance.GenerateWall(3, tank.random);
                break;
            case FoodType.AutoTurret:
                var spawnPos=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                    .Except(MapManager.Instance.GetAllTankPos()).ToList();
                var pos = spawnPos[tank.random.Next(spawnPos.Count)];
               
                    GameObject turretObj = Instantiate(autoTurretPrefab, 
                        new Vector2(pos.x+0.5f , pos.y+0.5f), 
                        Quaternion.identity);
                    AutoTurret turret = turretObj.GetComponent<AutoTurret>();
                    turret.tank = tank;
                turret.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
                
                turret.Pos = pos;
                
                break;
            case FoodType.Landmine:
                var spawnPos1=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                    .Except(MapManager.Instance.GetAllTankPos()).ToList();
                var pos1 = spawnPos1[tank.random.Next(spawnPos1.Count)];
               
                GameObject landmine = Instantiate(landminePrefab, 
                    new Vector2(pos1.x , pos1.y), 
                    Quaternion.identity);
                Landmine landmineObj = landmine.GetComponent<Landmine>();
                landmineObj.tank = tank;
                landmineObj.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
                
                landmineObj.Pos = pos1;
                break;
                
        }
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