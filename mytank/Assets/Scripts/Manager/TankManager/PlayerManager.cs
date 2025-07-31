
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DG.Tweening;
    using Tankgame;
    using TMPro;
    using UnityEngine;

    public class PlayerManager:SingletonMono<PlayerManager>
    {
        public List<TankController> activePlayers = new List<TankController>();

        public int recoverTime = 200;

        
        public void OnGameStart(List<PlayerInfo> playerInfos)
        {
            //      清除
            //ClearAllPlayers();
            playerNum=playerInfos.Count;
            // 创建所有玩家的坦克
            CreateAllPlayerTanks(playerInfos);
        }
    
        public void ClearAllPlayers()
        {
            foreach (var player in activePlayers)
            {
                if (player != null)
                {
                    TankFactory.Instance.TankPool.ReturnObject(player);

                }
            }
            activePlayers.Clear();
        }
    
        void CreateAllPlayerTanks(List<PlayerInfo> playerInfos)
        {

   
            // 为每个玩家创建坦克
            for (int i = 0; i <playerInfos.Count; i++)
            {
                var playerInfo = playerInfos[i];
             
                
                // 获取出生点
                Vector2Int spawnPoint = LevelManager.Instance.currentLevel.playerTankPawns[i];
            
                
    
                Color color= new Color(
                    playerInfo.ColorR / 255f,
                    playerInfo.ColorG / 255f,
                    playerInfo.ColorB / 255f
                );
                TankFactory.Instance.InitialPlayer(playerInfo.PlayerId, playerInfo.PlayerName, spawnPoint.x,
                    spawnPoint.y, color,playerInfo.PlayerRole);
            
            }
        }
        
       

        private int playerNum;
        public void RemovePlayer(TankController tank)
        {
            //activePlayers.Remove(tank);
            playerNum--;
            if (playerNum == 0)
            {
                GameStateManager.Instance.GameOver(false);
            }
            else
            {
                deadPlayerDic.Add(tank,recoverTime);
            }
        }
        Dictionary<TankController, int> deadPlayerDic = new Dictionary<TankController, int>();
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
                        TankFactory.Instance.RevivalPlayer(tank, pos.x, pos.y);
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
        
        
        
    }
