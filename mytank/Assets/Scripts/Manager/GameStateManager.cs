using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Tankgame;
using TMPro;
using UnityEngine.Serialization;

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
        int num= tank.foodDict[foodType]++;
        switch (foodType)
        {
            case FoodType.Boat:
                if (num == 0)
                {
                    tank.isBoat = true;
                    tank.boatFrame=NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
                }
                else if (num == 1)
                {
                    tank.isBoat = true;
                    tank.boatFrame=NetworkManager.Instance.currentFrame + Mathf.RoundToInt(30 / Constant.FrameInterval);

                }
                else if (num == 2)
                {
                    tank.isBoat = true;
                    tank.boatFrame=long.MaxValue;

                }
                break;
            case FoodType.Bomb:
                if (num == 0)
                {
                    EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(1, tank));
                    
                }
                else if (num == 1)
                {
                    EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(2, tank));
                    
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitBombController(tank);
                }
                break;
            case FoodType.Encourage:
                if (num == 0)
                {
                    tank.isEncourage = true;
                    tank.encourageFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);

    
                }
                else if (num == 1)
                {
                    tank.isEncourage = true;
                    tank.encourageFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(30 / Constant.FrameInterval);

    
                }
                else if (num == 2)
                {
                    tank.isEncourage = true;
                    tank.encourageFrame = long.MaxValue;
                }
                break;
            case FoodType.Star:
                if (num == 0)
                {
                    tank.isShootTwice = true;   
                    tank.shootTwiceFrame=NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);

    
                }
                else if (num == 1)
                {
                    tank.isShootTwice = true;   
                    tank.shootTwiceFrame=long.MaxValue;
    
                }
                else if (num == 2)
                {
                    tank.isShootThree = true;
                }
                break;
            case FoodType.PocketWatch:
                if (num == 0)
                {
                    long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                    EnemyManager.Instance.pauseEndFrame=targetFrame;
    
                }
                else if (num == 1)
                {
                    long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(30/Constant.FrameInterval);
                    EnemyManager.Instance.pauseEndFrame=targetFrame;
    
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitPocketWatchController(tank);
                }
                break;
            case FoodType.Shoe: 
                if (num == 0)
                {
                    tank.currentData.moveIntervalFrame--;


                }
                else if (num == 1)
                {
                    tank.currentData.moveIntervalFrame--;

    
                }
                else if (num == 2)
                {
                    tank.currentData.moveIntervalFrame--;
  
                }
                break;
            case FoodType.Shovel:
                if (num == 0)
                {
                    MapManager.Instance.isChange = true;
                    MapManager.Instance.ironWallEndFrames=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                    MapManager.Instance.HomeWall(MapType.wall);
    
                }
                else if (num == 1)
                {

                    MapManager.Instance.isChange = true;
                    MapManager.Instance.ironWallEndFrames=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(30/Constant.FrameInterval);
                    MapManager.Instance.HomeWall(MapType.wall);
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitShovelController(tank);
                }
                break;
            case FoodType.Pistol:
                if (num == 0)
                {

                    tank.isBreakWall = true;
                    tank.breakWallFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);

                }
                else if (num == 1)
                {

                    tank.isBreakWall = true;
                    tank.breakWallFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(30/Constant.FrameInterval);

                }
                else if (num == 2)
                {
                    tank.isBreakWall = true;
                    tank.breakWallFrame=long.MaxValue;
                }
              break;
            case FoodType.SteelHelmet:
                if (num == 0)
                {

                    tank.isInvincible = true;
                    tank.invincibleFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(5/Constant.FrameInterval);

                }
                else if (num == 1)
                {
                    tank.isInvincible = true;
                    tank.invincibleFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(15/Constant.FrameInterval);
                }
                else if (num == 2)
                {
                    EntityManager.Instance. InitSteelHelmetController(tank);
                }
                break;
            case FoodType.WarCar:
                if (num == 0)
                {
                    tank.AddHP(1);
                }
                else if (num == 1)
                {
                    tank.AddHP(2);
                }
                else if (num == 2)
                {
                    tank.AddHP(3);
                }
                break;
            case FoodType.GenerateWall:
                if (num == 0)
                {
                    MapManager.Instance.GenerateWall(3, tank.random);

    
                }
                else if (num == 1)
                {
                    MapManager.Instance.GenerateWall(4, tank.random);

    
                }
                else if (num == 2)
                {
                    MapManager.Instance.GenerateWall(5, tank.random);
  
                }

                break;
            case FoodType.AutoTurret:
                if (num == 0)
                {

                    EntityManager.Instance.InitAutoTurrent(tank);
    
                }
                else if (num == 1)
                {
                    EntityManager.Instance.InitAutoTurrent(tank);
                    EntityManager.Instance.InitAutoTurrent(tank);

    
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitAutoTurrent(tank);
                    EntityManager.Instance.InitAutoTurrent(tank);
                    EntityManager.Instance.InitAutoTurrent(tank);
  
                }

                
                break;
            case FoodType.Landmine:
                if (num == 0)
                {

                    EntityManager.Instance.InitLandmineMachine(tank);
    
                }
                else if (num == 1)
                {

                    EntityManager.Instance.InitLandmineMachine(tank);
                    EntityManager.Instance.InitLandmineMachine(tank);
    
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitLandmineMachine(tank);
                    EntityManager.Instance.InitLandmineMachine(tank);
                    EntityManager.Instance.InitLandmineMachine(tank);
  
                }

                break;
              
            case FoodType.HealingGarden:
                if (num == 0)
                {
                EntityManager.Instance.InitHealingGarden(tank);
                    
    
                }
                else if (num == 1)
                {

                EntityManager.Instance.InitHealingGarden(tank);
                EntityManager.Instance.InitHealingGarden(tank);
    
                }
                else if (num == 2)
                {
                EntityManager.Instance.InitHealingGarden(tank);
                EntityManager.Instance.InitHealingGarden(tank);
                EntityManager.Instance.InitHealingGarden(tank);
  
                }
                break;
            case FoodType.RearFire:
                if (num == 0)
                {
                    tank.rearFire = true;
                    tank.encourageFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);

                }
                else if (num == 1)
                {
                    tank.rearFire = true;
                    tank.encourageFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(30 / Constant.FrameInterval);

    
                }
                else if (num == 2)
                {
                    tank.rearFire = true;
                    tank.encourageFrame = long.MaxValue;
                }
                
                break;
            case FoodType.SpikeTrap:
                if (num == 0)
                {

                tank.isSpikeTrap = true;
    tank.spikeTrapDurationFrame=Mathf.RoundToInt(1/Constant.FrameInterval);
                }
                else if (num == 1)
                {
                    tank.isSpikeTrap = true;
                    tank.spikeTrapDurationFrame=Mathf.RoundToInt(2/Constant.FrameInterval);
    
                }
                else if (num == 2)
                {
                    tank.isSpikeTrap = true;
                    tank.spikeTrapDurationFrame=Mathf.RoundToInt(4/Constant.FrameInterval);
                }
                break;
            
            case FoodType.Protect:
                if (num == 0)
                {
                    EntityManager.Instance.InitProtectController(tank);
    
                }
                else if (num == 1)
                {
                    var targetProtect = EntityManager.Instance.protectControllers
                        .SingleOrDefault(pro => pro.tank == tank);

                    if (targetProtect != null)
                    {
                        targetProtect.genateIntervalFrame = (int)(3f / Constant.FrameInterval);
                    }
                   
    
                }
                else if (num == 2)
                {
                    var targetProtect = EntityManager.Instance.protectControllers
                        .SingleOrDefault(pro => pro.tank == tank);

                    if (targetProtect != null)
                    {
                        targetProtect.genateIntervalFrame = (int)(2f / Constant.FrameInterval);
                    }

                }
                
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