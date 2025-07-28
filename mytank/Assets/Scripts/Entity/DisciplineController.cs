
    using System.Linq;
    using UnityEngine;

    public class DisciplineController:MonoBehaviour
    {
        public TankController tank;
        public long lastTriggerFrame;
        
        public int genateIntervalFrame=(int)(60f/Constant.FrameInterval);

        public int targetNum = 1;
        
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastTriggerFrame > genateIntervalFrame)
            {
                int num = targetNum;
                foreach(var a in EnemyManager.Instance.activeEnemies)
                {
                    a.DamageHP(2, tank);
                    num--;
                    if (num == 0)
                    {
                        break;
                    }
                }
                
                
                
                lastTriggerFrame = NetworkManager.Instance.currentFrame;
            }
        }
    }
