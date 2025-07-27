
    using System.Collections.Generic;
    using UnityEngine;

    public class BombController:MonoBehaviour
    {
        public Vector2Int Pos;
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(5f/Constant.FrameInterval);
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(2, tank));
             
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
        }


    }
