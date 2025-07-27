using UnityEngine;


    public class ShovelController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(60f/Constant.FrameInterval);
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                MapManager.Instance.isChange = true;
                MapManager.Instance.ironWallEndFrames=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                MapManager.Instance.HomeWall(MapType.wall);
                
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
        }
    }
