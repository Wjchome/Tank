using UnityEngine;
using UnityEngine.UI;


public class SteelHelmetController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(60f/Constant.FrameInterval);

        public Image image;
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                tank.isInvincible = true;
                tank.invincibleFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                tank.transform.localScale =tank.scaleSize* 1.5f;
                
                
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
            else
            {
                image.fillAmount = (float )(NetworkManager.Instance.currentFrame - lastTriggerFrame)/ genateIntervalFrame;
            }
        }
    }
