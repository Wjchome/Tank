using UnityEngine;
using UnityEngine.UI;


public class ShovelController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(60f/Constant.FrameInterval);
        public Image image;
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                MapManager.Instance.isChange = true;
                MapManager.Instance.ironWallEndFrames=NetworkManager.Instance.currentFrame+Mathf.RoundToInt(10/Constant.FrameInterval);
                MapManager.Instance.HomeWall(MapType.wall);
                    tank.bombAnimator.Play("Shovel",0,0);
                
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
            else
            {
                image.fillAmount = (float )(NetworkManager.Instance.currentFrame - lastTriggerFrame)/ genateIntervalFrame;
            }
        }
    }
