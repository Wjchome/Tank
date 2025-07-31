
    using UnityEngine;

    public class WarCarController: UITimerEntity
    {
        public int addNum = 1;
        protected override void ApplyEffect()
        {
            if (tank.currentData.orignalHP == tank.currentData.HP)
            {
                tank.AddOrignalHP(addNum);
            }
        }
        
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>战车控制器</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"检测<color={Constant.VALUE_COLOR}>自身</color>是否满血,是则增加"+
                  $"<color={Constant.VALUE_COLOR}>{addNum}</color>点血量上限" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
