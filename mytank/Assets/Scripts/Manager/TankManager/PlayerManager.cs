using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FixMath.NET;
using Physics2D;
using Tankgame;
using TMPro;
using UnityEngine;

public class PlayerManager : SingletonMono<PlayerManager>
{
    public List<PlayerTankController> activePlayers = new List<PlayerTankController>();
    public Dictionary<string, PlayerTankController> activePlayerDic = new Dictionary<string, PlayerTankController>();


    public PlayerTankController playerTankControllerPrefab;
    public TankData playerTankData;


    public void OnGameStart(List<PlayerInfo> playerInfos)
    {
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


    public void UpdateFrame()
    {
        activePlayers.ToList().ForEach(a => a.UpdateFrame());
    }

    public PlayerTankController InitialPlayer(string tankID, string tankName, int x, int y, Color color, int dataIndex)
    {
        PlayerTankController temp = Instantiate(playerTankControllerPrefab);
        PhysicsWorld2DComponent.Instance.AddRigidBody(temp.GetComponent<RigidBody2DComponent>(),new FixVector2((Fix64)x,(Fix64)y));


        temp.tankID = tankID;
        temp.tankDirection = Direction.Up;
        temp.transform.rotation = Quaternion.identity;
        temp.scaleSize = temp.transform.localScale;
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


        FixRect fixRect = new FixRect((Fix64)x -Constant.tankSize, (Fix64)y - Constant.tankSize,
            Constant.tankSize * (Fix64)2, Constant.tankSize * (Fix64)2);
        

        QuadTreeLayer layer =
            QuadTreeLayer.GetLayer((int)QuadTreeLayerType.TankFriend);
       // QuadTreeV3.QuadTreeV3.Instance.AddObject(fixRect, temp.gameObject, layer);
        
        temp.transform.position = (Vector2)fixRect.Center;
        temp.playerColor = color;
        temp.spriteRenderer.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);

        temp.random = new System.Random(seed);

        TankData data = playerTankData;
        temp.shootIntervalFrame = data.shootIntervalFrame;
        temp.orignalHP = data.orignalHP;
        temp.HP = data.HP;


        temp.playerPanelUI =
            Instantiate(GameUIManager.Instance.playerPanelPrefab, GameUIManager.Instance.playerPanelParent)
                .GetComponent<PlayerPanelUI>();
        temp.playerPanelUI.Init(temp);
        temp.playerPanelUI.UpdateUI();

        temp.tankEntityTankUI.healthBarFill.color = color;
        Color.RGBToHSV(color, out float h, out float s, out float v);
        Color delayColor = Color.HSVToRGB(
            h, // 保持色相（Hue）不变
            0.3f, // 降低饱和度（Saturation）
            0.7f // 降低亮度（Value）
        );
        delayColor.a = 0.8f; // 适当透明度
        temp.tankEntityTankUI.healthBarDelay.color = delayColor;
        temp.tankEntityTankUI.nameText.color = color;
        temp.tankEntityTankUI.nameText.text = tankName;


        temp.animType = dataIndex;
        activePlayers.Add(temp);
        activePlayerDic.Add(tankID, temp);
        EntityManager.Instance.allTanks.Add(temp);
        EntityManager.Instance.UpdateTankPos(temp, new Vector2Int(-2, -2), new Vector2Int(x, y));
        switch (dataIndex)
        {
            case 1:
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Shoe);

/*
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Discipline);

                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.PocketWatch);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);
                GameStateManager.Instance.ApplyFoodToTank(temp, (int)FoodType.Bomb);
*/
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

    public void OnGameOver()
    {
        foreach (var playerTank in activePlayers.ToList())
        {
            Destroy(playerTank.gameObject);
        }

        activePlayers.Clear();
        activePlayerDic.Clear();
    }

    public TankController RevivalPlayer(PlayerTankController tank, int x, int y)
    {
        tank.isDead = false;
        tank.tankDirection = Direction.Up;
        tank.transform.rotation = Quaternion.identity;


        // 设置坦克中心位置
        tank.transform.position = new Vector3(x, y, 0);

        tank.HP = tank.orignalHP;

        tank.playerPanelUI.UpdateUI();

        return tank;
    }
}