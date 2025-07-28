
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public class LandmineMachine:MonoBehaviour
    {
        public TankController tank;
        public int genateIntervalFrame;
         public long lastTriggerFrame = 0;
        public Image image;
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                EntityManager.Instance. InitLandmine(tank);
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
            else
            {
                image.fillAmount = (float )(NetworkManager.Instance.currentFrame - lastTriggerFrame)/ genateIntervalFrame;
            }
        }
      
    }
