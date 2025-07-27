using UnityEngine;


    public class PocketWatchController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(30f/Constant.FrameInterval);
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                long targetFrame=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                EnemyManager.Instance.pauseEndFrame=targetFrame;
                
                
                lastTriggerFrame=NetworkManager.Instance.currentFrame;
            }
        }
    }
