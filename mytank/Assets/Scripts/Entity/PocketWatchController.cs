using UnityEngine;
using UnityEngine.UI;


public class PocketWatchController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(30f/Constant.FrameInterval);
        
        public Image  image;
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                EnemyManager.Instance.pauseEndFrame=targetFrame;
                
                tank.bombAnimator.Play("PocketWatch",0,0);
                
                lastTriggerFrame=NetworkManager.Instance.currentFrame;
            }
            else
            {
                image.fillAmount = (float )(NetworkManager.Instance.currentFrame - lastTriggerFrame)/ genateIntervalFrame;
            }
        }
    }
