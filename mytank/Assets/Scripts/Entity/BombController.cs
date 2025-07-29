
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BombController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(5f/Constant.FrameInterval);

        public Image image;
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(2, tank));
                    tank.bombAnimator.Play("BombShow",0,0);
             
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
            else
            {
                image.fillAmount = (float )(NetworkManager.Instance.currentFrame - lastTriggerFrame)/ genateIntervalFrame;
            }
        }


    }
