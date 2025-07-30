using UnityEngine;
using UnityEngine.UI;


public class SteelHelmetController : UITimerEntity
{

    
    public float stopTimerNum=10;
        protected override void ApplyEffect()
        {
            tank.isInvincible = true;
            tank.invincibleFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(stopTimerNum/ Constant.FrameInterval);
            tank.transform.localScale = tank.scaleSize * 1.5f;
        }
        
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>头盔</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"<color={Constant.VALUE_COLOR}>自身</color>将无敌"+
                  $"<color={Constant.VALUE_COLOR}>{stopTimerNum}</color>秒" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
