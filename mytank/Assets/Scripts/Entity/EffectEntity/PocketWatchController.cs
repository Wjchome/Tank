using UnityEngine;
using UnityEngine.UI;


public class PocketWatchController : EffectEntity
{

        
        protected override void ApplyEffect()
        {
            long targetFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(10 / Constant.FrameInterval);
            EnemyManager.Instance.pauseEndFrame = targetFrame;
            tank.bombAnimator.Play("PocketWatch", 0, 0);
        }
    }
