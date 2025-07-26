
    using UnityEngine;

    public class Landmine:MonoBehaviour
    {
        public Vector2Int Pos;
        public TankController tank;

        public void UpdateFrame()
        {
            
        }
        public void Trigger()
        {
            var tanks= MapManager.Instance.GetTankInArea(Pos.x-1, Pos.y-1,3,3);
            foreach (var a in tanks)
            {
                a.DamageHP(1,tank);
            }
            Destroy(this.gameObject);
        }
    }
