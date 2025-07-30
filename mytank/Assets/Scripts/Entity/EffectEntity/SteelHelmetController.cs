using UnityEngine;
using UnityEngine.UI;


public class SteelHelmetController : EffectEntity
{

        protected override void ApplyEffect()
        {
            tank.isInvincible = true;
            tank.invincibleFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
            tank.transform.localScale = tank.scaleSize * 1.5f;
        }
    }
