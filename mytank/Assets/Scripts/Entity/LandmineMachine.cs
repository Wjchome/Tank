
    using UnityEngine;

    public class LandmineMachine:MonoBehaviour
    {
        public Vector2Int Pos;
        public TankController tank;
        public int genateorIntervalFrame;
        public long lastGenateFrame = 0;
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastGenateFrame > genateorIntervalFrame)
            {
                EntityManager.Instance. InitLandmine(tank);
                lastGenateFrame = NetworkManager.Instance.currentFrame;
            }
        }
      
    }
