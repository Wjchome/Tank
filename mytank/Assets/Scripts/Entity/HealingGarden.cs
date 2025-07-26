
    using System;
    using UnityEngine;

    public class HealingGarden:MonoBehaviour
    {
        public Vector2Int Pos;

        public long lastGenateFrame;

        public int genateIntervalFrame=10;

        public bool isOk = false;
        
        public Color color;
        public void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastGenateFrame > genateIntervalFrame)
            {
                if (!isOk)
                {
                    isOk = true;
                    GetComponent<SpriteRenderer>().color = Color.green;
                }
            }
        }

        public void PickUp(TankController tank)
        {
            if (isOk)
            {
                tank.AddHP(1);
                isOk = false;
                lastGenateFrame = NetworkManager.Instance.currentFrame;
                GetComponent<SpriteRenderer>().color =color;
            }
        }
    }
