
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
        
        
     
        public void GameStart(int level)
        {
           
               currentLevel =availableLevels[level];
               levelNameText.text = currentLevel.levelName;
        }

        
        

        
    }
