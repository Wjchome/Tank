using System;
using UnityEngine;
using UnityEngine.Serialization;

public class HealingGarden : MapEntity
{
    public long lastGenateFrame;

    public int genateIntervalFrame;

     public bool isCanHealing = false;

     public int healingNum;

    public override void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame - lastGenateFrame > genateIntervalFrame)
        {
            if (!isCanHealing)
            {
                isCanHealing = true;
                GetComponent<SpriteRenderer>().color = Color.green;
                lastGenateFrame = NetworkManager.Instance.currentFrame;
            }
        }

        if (isCanHealing)
        {
            var tanks = MapManager.Instance.GetPlayerTankInArea(Pos.x, Pos.y, 1, 1);
            if (tanks != null)
            {
                foreach (var _tank in tanks)
                {
                    PickUp(_tank);
                }
            }
        }
    }


    void PickUp(TankController tank)
    {
        if (isCanHealing)
        {
            tank.AddHP(healingNum);
            isCanHealing = false;
            lastGenateFrame = NetworkManager.Instance.currentFrame;
            GetComponent<SpriteRenderer>().color = tank.playerColor;
        }
    }
}