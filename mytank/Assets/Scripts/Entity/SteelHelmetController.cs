using UnityEngine;


    public class SteelHelmetController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(60f/Constant.FrameInterval);
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                tank.isInvincible = true;
                tank.invincibleFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);

                
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
        }
    }
