
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using Tankgame;
    using TMPro;
    using UnityEngine;

    public class PlayerManager:SingletonMono<PlayerManager>
    {
        public List<TankController> activePlayers = new List<TankController>();

        private void Start()
        {
            NetworkManager.Instance.OnGameStart += OnGameStart;

        }

        private void OnDestroy()
        {
            NetworkManager.Instance.OnGameStart -= OnGameStart;

        }


        void OnGameStart(GameStart gameStart)
        {
            //      清除
            ClearAllPlayers();
        
            // 创建所有玩家的坦克
            CreateAllPlayerTanks(gameStart);
        }
    
        public void ClearAllPlayers()
        {
            foreach (var enemy in activePlayers)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            activePlayers.Clear();
        }
    
        void CreateAllPlayerTanks(GameStart gameStart)
        {

   
            // 为每个玩家创建坦克
            for (int i = 0; i <gameStart.PlayerInfos.Count; i++)
            {
                var playerInfo = gameStart.PlayerInfos[i];
            
                // 获取出生点
                Vector2Int spawnPoint = MapManager.Instance.GetSpawnPoint(i);
            
    
                Color color= new Color(
                    playerInfo.ColorR / 255f,
                    playerInfo.ColorG / 255f,
                    playerInfo.ColorB / 255f
                );
                var tank = TankFactory.Instance.Initialize(playerInfo.PlayerId, playerInfo.PlayerName, spawnPoint.x,
                    spawnPoint.y, color, true, 0);
                activePlayers.Add(tank);
            }
        }
        public void RemovePlayer(TankController tank)
        {
            activePlayers.Remove(tank);
            if (activePlayers.Count == 0)
            {
                GameFail();
            }
        }
        
        
        public void GameFail()
        {
            GameUIManager.Instance.playerPanelParent.GetComponent<RectTransform>().
                DOAnchorPos(GameUIManager.Instance.secondPos, 0.5f).SetEase(Ease.OutQuad);
            GameUIManager.Instance.gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = "You Lose!";
            GameUIManager.Instance.gameOverButton.gameObject.SetActive(true);
            NetworkManager.Instance.isGameing = false;


        }
    }
