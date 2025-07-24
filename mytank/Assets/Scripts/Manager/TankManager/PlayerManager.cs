
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using Tankgame;
    using TMPro;
    using UnityEngine;

    public class PlayerManager:SingletonMono<PlayerManager>
    {
        public List<TankController> activePlayers = new List<TankController>();

       

        
        public void OnGameStart(List<PlayerInfo> playerInfos)
        {
            //      清除
            //ClearAllPlayers();
        
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
                    spawnPoint.y, color);
            
            }
        }
        public void RemovePlayer(TankController tank)
        {
            activePlayers.Remove(tank);
            if (activePlayers.Count == 0)
            {
                GameStateManager.Instance.GameOver(false);
            }
        }
        
        
        
    }
