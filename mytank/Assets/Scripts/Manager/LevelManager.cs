
    using System;
    using System.Collections.Generic;
    using Tankgame;
    using TMPro;
    using UnityEngine;

    public class LevelManager:SingletonMono<LevelManager>
    {
        
        public List<Level> availableLevels = new List<Level>();
        public Level currentLevel;

        public TextMeshProUGUI levelNameText;
        
        
        private void Start()
        {
            NetworkManager.Instance.OnGameStart += GameStart;
        }

        public void GameStart(GameStart gameStart)
        {
           
                currentLevel = availableLevels [gameStart.Level];
                levelNameText.text = currentLevel.levelName;
                
                MapManager.Instance.LoadLevel(currentLevel);
                EnemyManager.Instance.LoadLevel(currentLevel);
               PlayerManager.Instance.   OnGameStart(gameStart);
        }

        
        

        private void OnDestroy()
        {
            NetworkManager.Instance.OnGameStart -= GameStart;
        }
        
        
    }
