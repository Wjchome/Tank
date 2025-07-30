using UnityEngine;
using UnityEngine.UI;


public class ShovelController : EffectEntity
{
   
        
        protected override void ApplyEffect()
        {
            MapManager.Instance.isChange = true;
            MapManager.Instance.ironWallEndFrames = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
            MapManager.Instance.HomeWall(MapType.wall);
            tank.bombAnimator.Play("Shovel", 0, 0);
        }
    }
