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
                    ApplyFoodToTank(tank, foodRequest.FoodId);
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

    void ApplyInputToTank(PlayerTankController tank, PlayerInput input)
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
                tank.MoveBy(Direction.Left);
                break;
            case InputType.InputMoveRight:
                tank.MoveBy(Direction.Right);
                break;
            case InputType.InputShoot:
                tank.Shoot();
                break;
        }
    }

    public void ApplyFoodToTank(PlayerTankController tank, int foodId)
    {
        FoodType foodType = (FoodType)foodId;
        int num = 0;
        if (tank.foodDict.ContainsKey(foodType)) // 0 
        {
            num = tank.foodDict[foodType]++;
        }
        else
        {
            tank.foodDict.Add(foodType, 1);
        }

        tank.playerPanelUI.UpdateFoodUI(foodType, num);
        switch (foodType)
        {
            case FoodType.Boat:
                if (num == 0)
                {
                    tank.isBoat = true;
                    tank.boatFrame = NetworkManager.Instance.currentFrame +
                                     Mathf.RoundToInt(10 / Constant.FrameInterval);
                    tank.boatShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.isBoat = true;
                    tank.boatFrame = NetworkManager.Instance.currentFrame +
                                     Mathf.RoundToInt(30 / Constant.FrameInterval);
                    tank.boatShow.SetActive(true);
                }
                else if (num == 2)
                {
                    tank.isBoat = true;
                    tank.boatFrame = long.MaxValue;
                    tank.boatShow.SetActive(true);
                }

                break;
            case FoodType.Bomb:
                if (num == 0)
                {
                    EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(10, tank,DamageType.Bomb));
                    tank.bombAnimator.Play("BombShow", 0, 0);
                }
                else if (num == 1)
                {
                    EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(20, tank,DamageType.Bomb));
                    tank.bombAnimator.Play("BombShow", 0, 0);
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
                    tank.encourageFrame = NetworkManager.Instance.currentFrame +
                                          Mathf.RoundToInt(10 / Constant.FrameInterval);
                    tank.encourageShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.isEncourage = true;
                    tank.encourageFrame = NetworkManager.Instance.currentFrame +
                                          Mathf.RoundToInt(30 / Constant.FrameInterval);
                    tank.encourageShow.SetActive(true);
                }
                else if (num == 2)
                {
                    tank.isEncourage = true;
                    tank.encourageFrame = long.MaxValue;
                    tank.encourageShow.SetActive(true);
                }

                break;
            case FoodType.Star:
                if (num == 0)
                {
                    tank.isShootTwice = true;
                    tank.shootTwiceFrame = NetworkManager.Instance.currentFrame +
                                           Mathf.RoundToInt(10 / Constant.FrameInterval);
                    tank.shootTwiceShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.isShootTwice = true;
                    tank.shootTwiceFrame = long.MaxValue;
                    tank.shootTwiceShow.SetActive(true);
                }
                else if (num == 2)
                {
                    tank.isShootThree = true;
                }

                break;
            case FoodType.PocketWatch:
                if (num == 0)
                {
                    long targetFrame = NetworkManager.Instance.currentFrame +
                                       Mathf.RoundToInt(10 / Constant.FrameInterval);
                    EnemyManager.Instance.pauseEndFrame = targetFrame;
                    tank.bombAnimator.Play("PocketWatch", 0, 0);
                }
                else if (num == 1)
                {
                    long targetFrame = NetworkManager.Instance.currentFrame +
                                       Mathf.RoundToInt(30 / Constant.FrameInterval);
                    EnemyManager.Instance.pauseEndFrame = targetFrame;
                    tank.bombAnimator.Play("PocketWatch", 0, 0);
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitPocketWatchController(tank);
                }

                break;
            case FoodType.Shoe:
                if (num == 0)
                {
                    tank.moveIntervalFrame--;
                    tank.shoeShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.moveIntervalFrame--;
                    tank.shoeShow.GetComponent<SpriteRenderer>().color = new Color32(0, 180, 0, 255);
                }
                else if (num == 2)
                {
                    tank.moveIntervalFrame--;
                    tank.shoeShow.GetComponent<SpriteRenderer>().color = new Color32(0, 90, 0, 255);
                }

                break;
            case FoodType.Shovel:
                if (num == 0)
                {
                    MapManager.Instance.isChange = true;
                    MapManager.Instance.ironWallEndFrames = NetworkManager.Instance.currentFrame +
                                                            Mathf.RoundToInt(10 / Constant.FrameInterval);
                    MapManager.Instance.SetHomeWall(MapType.wall);
                    tank.bombAnimator.Play("Shovel", 0, 0);
                }
                else if (num == 1)
                {
                    MapManager.Instance.isChange = true;
                    MapManager.Instance.ironWallEndFrames = NetworkManager.Instance.currentFrame +
                                                            Mathf.RoundToInt(30 / Constant.FrameInterval);
                    MapManager.Instance.SetHomeWall(MapType.wall);
                    tank.bombAnimator.Play("Shovel", 0, 0);
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
                    tank.breakWallFrame = NetworkManager.Instance.currentFrame +
                                          Mathf.RoundToInt(10 / Constant.FrameInterval);
                    tank.breakWallShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.isBreakWall = true;
                    tank.breakWallFrame = NetworkManager.Instance.currentFrame +
                                          Mathf.RoundToInt(30 / Constant.FrameInterval);
                    tank.breakWallShow.SetActive(true);
                }
                else if (num == 2)
                {
                    tank.isBreakWall = true;
                    tank.breakWallFrame = long.MaxValue;
                    tank.breakWallShow.SetActive(true);
                }

                break;
            case FoodType.SteelHelmet:
                if (num == 0)
                {
                    tank.isInvincible = true;
                    tank.invincibleFrame = NetworkManager.Instance.currentFrame +
                                           Mathf.RoundToInt(5 / Constant.FrameInterval);
                    tank.transform.localScale = tank.scaleSize * 1.5f;
                }
                else if (num == 1)
                {
                    tank.isInvincible = true;
                    tank.invincibleFrame = NetworkManager.Instance.currentFrame +
                                           Mathf.RoundToInt(15 / Constant.FrameInterval);
                    tank.transform.localScale = tank.scaleSize * 1.5f;
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitSteelHelmetController(tank);
                }

                break;
            case FoodType.WarCar:
                if (num == 0)
                {
                    tank.AddOrignalHP(1);
                }
                else if (num == 1)
                {
                    tank.AddOrignalHP(3);
                }
                else if (num == 2)
                {
                    EntityManager.Instance.InitWarCarController(tank);
                }

                break;
            case FoodType.GenerateWall:
                if (num == 0)
                {
                    MapManager.Instance.GenerateWall(3, tank.random);
                    tank.bombAnimator.Play("GenateWall", 0, 0);
                }
                else if (num == 1)
                {
                    MapManager.Instance.GenerateWall(4, tank.random);
                    tank.bombAnimator.Play("GenateWall", 0, 0);
                }
                else if (num == 2)
                {
                    MapManager.Instance.GenerateWall(5, tank.random);
                    tank.bombAnimator.Play("GenateWall", 0, 0);
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
                    tank.rearFireFrame = NetworkManager.Instance.currentFrame +
                                         Mathf.RoundToInt(10 / Constant.FrameInterval);
                    tank.rearFireShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.rearFire = true;
                    tank.rearFireFrame = NetworkManager.Instance.currentFrame +
                                         Mathf.RoundToInt(30 / Constant.FrameInterval);
                    tank.rearFireShow.SetActive(true);
                }
                else if (num == 2)
                {
                    tank.rearFire = true;
                    tank.rearFireFrame = long.MaxValue;
                    tank.rearFireShow.SetActive(true);
                }

                break;
            case FoodType.SpikeTrap:
                if (num == 0)
                {
                    tank.isSpikeTrap = true;
                    tank.spikeTrapDurationFrame = Mathf.RoundToInt(1 / Constant.FrameInterval);
                    tank.spikeTrapShow.SetActive(true);
                }
                else if (num == 1)
                {
                    tank.isSpikeTrap = true;
                    tank.spikeTrapDurationFrame = Mathf.RoundToInt(2 / Constant.FrameInterval);
                }
                else if (num == 2)
                {
                    tank.isSpikeTrap = true;
                    tank.spikeTrapDurationFrame = Mathf.RoundToInt(4 / Constant.FrameInterval);
                }

                break;

            case FoodType.Protect:
                if (num == 0)
                {
                    EntityManager.Instance.InitProtectController(tank);
                }
                else if (num == 1)
                {
                    var targetProtect = EntityManager.Instance.FindFirstOrDefaultUIEntity<ProtectController>(tank);

                    if (targetProtect != null)
                    {
                        targetProtect.genateIntervalFrame = (int)(3f / Constant.FrameInterval);
                    }
                }
                else if (num == 2)
                {
                    var targetProtect = EntityManager.Instance.FindFirstOrDefaultUIEntity<ProtectController>(tank);

                    if (targetProtect != null)
                    {
                        targetProtect.genateIntervalFrame = (int)(2f / Constant.FrameInterval);
                    }
                }

                break;
            case FoodType.Discipline:
                if (num == 0)
                {
                    EntityManager.Instance.InitDisciplineController(tank);
                }
                else if (num == 1)
                {
                    var targetDiscipline =
                        EntityManager.Instance.FindFirstOrDefaultUIEntity<DisciplineController>(tank);

                    if (targetDiscipline != null)
                    {
                        targetDiscipline.targetNum = 2;
                    }
                }
                else if (num == 2)
                {
                    var targetDiscipline =
                        EntityManager.Instance.FindFirstOrDefaultUIEntity<DisciplineController>(tank);

                    if (targetDiscipline != null)
                    {
                        targetDiscipline.targetNum = 3;
                    }
                }

                break;
            case FoodType.GhostGuard:
                if (num == 0)
                {
                    EntityManager.Instance.InitGhostGuard(tank);
                }
                else if (num == 1)
                {
                    EntityManager.Instance.InitGhostGuardMachine(tank);
                }
                else if (num == 2)
                {
                    var target = EntityManager.Instance.FindFirstOrDefaultUIEntity<GhostGuardMachine>(tank);
                    target.genateIntervalFrame = 600;
                }

                break;
            case FoodType.AlmightyTurret:
                //  => 所有地雷制造机变成自动发射机 所有现存自动炮台，地雷，治疗花园
                // => 变成自动设计台（能发射子弹，子弹可以打敌人，打玩家可以恢复血量，敌人触碰爆炸） 

                var landmineMachines = EntityManager.Instance.FindUIEntities<LandmineMachine>(tank);

                foreach (var machine in landmineMachines)
                {
                    machine.Destroy();
                    EntityManager.Instance.InitAlmightyTurretMachine(tank);
                }

                // 转换所有地图实体
                var landmines = EntityManager.Instance.FindMapEntities<Landmine>(tank);
                var autoTurrets = EntityManager.Instance.FindMapEntities<AutoTurret>(tank);
                var healingGardens = EntityManager.Instance.FindMapEntities<HealingGarden>(tank);
                // 转换地雷
                foreach (var landmine in landmines)
                {
                    Vector2Int pos = landmine.Pos;
                    EntityManager.Instance.InitAlmightyTurret(tank, pos);
                    landmine.Destroy();
                }

                // 转换自动炮台
                foreach (var turret in autoTurrets)
                {
                    Vector2Int pos = turret.Pos;
                    EntityManager.Instance.InitAlmightyTurret(tank, pos);
                    turret.Destroy();
                }

                // 转换治疗花园
                foreach (var garden in healingGardens)
                {
                    Vector2Int pos = garden.Pos;
                    EntityManager.Instance.InitAlmightyTurret(tank, pos);
                    garden.Destroy();
                }

                break;
            case FoodType.SpeedKiller:
                tank.isSpeedKiller = true;
                tank.speedKillerShow.SetActive(true);
                break;
            case FoodType.LifeEnhancement:
                WarCarController a=EntityManager.Instance.FindFirstOrDefaultUIEntity<WarCarController>(tank);
                a.islimited = false;
                break;
            case FoodType.TimeStorm:
                // 移除所有相关控制器
                var bombControllers = EntityManager.Instance.FindUIEntities<BombController>(tank);
                var disciplineControllers = EntityManager.Instance.FindUIEntities<DisciplineController>(tank);
                var pocketWatchControllers = EntityManager.Instance.FindUIEntities<PocketWatchController>(tank);
    
                // 销毁所有控制器
                foreach (var controller in bombControllers)
                {
                    controller.Destroy();
                }
    
                foreach (var controller in disciplineControllers)
                {
                    controller.Destroy();
                }
    
                foreach (var controller in pocketWatchControllers)
                {
                    controller.Destroy();
                }
    
                // 生成新的TimeController
                EntityManager.Instance.InitTimeStormController(tank);
                break;
                
            case FoodType.PenetratingBullet:
                
                    tank.isPenetrate = true;
                    tank.penetrateShow.SetActive(true);
                
                break;
    
        }
    }


    public void GameOver(bool isWin)
    {
        if (!NetworkManager.Instance.isGameing) return;
        GameUIManager.Instance.playerPanelParent.GetComponent<RectTransform>()
            .DOAnchorPos(GameUIManager.Instance.secondPos, 0.5f).SetEase(Ease.OutQuad);
        GameUIManager.Instance.gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text =
            isWin ? "You Win" : "You Lose!";
        GameUIManager.Instance.gameOverButton.gameObject.SetActive(true);
        NetworkManager.Instance.isGameing = false;

        /*
        BulletFactory.Instance.ClearAllBullets();
        TankFactory.Instance.ClearAllTank();

        PlayerManager.Instance.activePlayers.Clear();
        EnemyManager.Instance.activeEnemies.Clear();
       */

        PlayerManager.Instance.activePlayers.ForEach(a => a.Dead());
         EnemyManager.Instance.activeEnemies.ForEach(a => a.Dead(null));
        EntityManager.Instance.GameOver();

        MapManager.Instance.ClearMap();
    }
}