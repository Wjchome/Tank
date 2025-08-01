using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Tankgame;
using TMPro;
using UnityEngine;

public class PlayerManager : SingletonMono<PlayerManager>
{
    public List<PlayerTankController> activePlayers = new List<PlayerTankController>();

    public int recoverTime = 200;

    public PlayerTankController playerTankControllerPrefab;
    public TankData playerTankData;

    public void OnGameStart(List<PlayerInfo> playerInfos)
    {
        //      清除
        //ClearAllPlayers();
        playerNum = playerInfos.Count;
        // 创建所有玩家的坦克
        CreateAllPlayerTanks(playerInfos);
    }


    void CreateAllPlayerTanks(List<PlayerInfo> playerInfos)
    {
        // 为每个玩家创建坦克
        for (int i = 0; i < playerInfos.Count; i++)
        {
            var playerInfo = playerInfos[i];


            // 获取出生点
            Vector2Int spawnPoint = LevelManager.Instance.currentLevel.playerTankPawns[i];


            Color color = new Color(
                playerInfo.ColorR / 255f,
                playerInfo.ColorG / 255f,
                playerInfo.ColorB / 255f
            );
           InitialPlayer(playerInfo.PlayerId, playerInfo.PlayerName, spawnPoint.x,
                spawnPoint.y, color, playerInfo.PlayerRole);
        }
    }


    private int playerNum;

    public void RemovePlayer(PlayerTankController tank)
    {
        //activePlayers.Remove(tank);
        playerNum--;
        if (playerNum == 0)
        {
            GameStateManager.Instance.GameOver(false);
        }
        else
        {
            deadPlayerDic.Add(tank, recoverTime);
        }
    }

    Dictionary<PlayerTankController, int> deadPlayerDic = new Dictionary<PlayerTankController, int>();

    public void UpdateFrame()
    {
        if (deadPlayerDic.Count > 0)
        {
            // 反向遍历，避免修改集合的问题
            var keys = deadPlayerDic.Keys.ToList();

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                var tank = keys[i];
                var timer = deadPlayerDic[tank] - 1;

                if (timer <= 0)
                {
                    var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
                        .Except(MapManager.Instance.GetAllTankPos()).ToList();
                    var pos = spawnPos[tank.random.Next(spawnPos.Count)];
                    RevivalPlayer(tank, pos.x, pos.y);
                    playerNum++;
                    deadPlayerDic.Remove(tank);
                }
                else
                {
                    deadPlayerDic[tank] = timer;
                }
            }
        }
    }

    public PlayerTankController InitialPlayer(string tankID, string tankName, int x, int y, Color color, int dataIndex)
    {
        PlayerTankController temp = Instantiate(playerTankControllerPrefab);


        temp.tankID = tankID;
        temp.tankDirection = Direction.Up;
        temp.transform.rotation = Quaternion.identity;
        temp.Pos = new Vector2Int(x, y); // 左下角坐标

        if (tankID == NetworkManager.Instance.playerID)
        {
            temp.identity = Identity.Myself;
            NetworkManager.Instance.myTank = temp;
        }
        else
        {
            temp.identity = Identity.OtherPlayer;
        }

        temp.playerName = tankName;

        // 设置坦克中心位置
        temp.transform.position = temp.GetCenter();
        temp.playerColor = color;
        temp.GetComponent<SpriteRenderer>().material.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);


        temp.currentData = ScriptableObject.CreateInstance<TankData>();
        temp.currentData.InitializeTankData(playerTankData);


        temp.playerPanelUI =
            Instantiate(GameUIManager.Instance.playerPanelPrefab, GameUIManager.Instance.playerPanelParent)
                .GetComponent<PlayerPanelUI>();
        temp.playerPanelUI.Init(temp, PlayerManager.Instance.activePlayers.Count);

        temp.playerPanelUI.UpdateUI();
        temp.animType = dataIndex;

        PlayerManager.Instance.activePlayers.Add(temp);

        switch (dataIndex)
        {
            case 1:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Shoe);


                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);

                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);

                break;
            case 2:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.WarCar);
                break;
            case 3:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.HealingGarden);
                break;
            case 4:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Shovel);
                break;
        }


        return temp;
    }

    public TankController RevivalPlayer(PlayerTankController tank, int x, int y)
    {
        tank.isDead = false;
        tank.tankDirection = Direction.Up;
        tank.transform.rotation = Quaternion.identity;
        tank.Pos = new Vector2Int(x, y); // 左下角坐标


        // 设置坦克中心位置
        tank.transform.position = tank.GetCenter();

        tank.currentData.HP = tank.currentData.orignalHP;

        tank.playerPanelUI?.UpdateUI();

        return tank;
    }
}