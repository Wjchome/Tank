using System;
using Physics2D;
using UnityEngine;
using UnityEngine.Serialization;

public class HealingGarden : MapEntity
{
    public long lastGenateFrame;

    public int genateIntervalFrame;

    public bool isCanHealing = false;

    public int healingNum;

    public RigidBody2DComponent rigidBody;

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
            foreach (var stay in rigidBody.Stay)
            {
                if (stay.Layer == PhysicsLayer.GetLayer((int)QuadTreeLayerType.TankFriend))
                {
                    var _tank = stay.GetCachedComponent<PlayerTankController>();
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