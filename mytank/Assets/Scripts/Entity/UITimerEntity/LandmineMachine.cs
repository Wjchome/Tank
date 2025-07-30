
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public class LandmineMachine : UITimerEntity
    {
        protected override void ApplyEffect()
        {
            EntityManager.Instance.InitLandmine(tank);

        }
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>地雷生成器</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"释放<color={Constant.VALUE_COLOR}>1</color>个地雷" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    
      
    }
