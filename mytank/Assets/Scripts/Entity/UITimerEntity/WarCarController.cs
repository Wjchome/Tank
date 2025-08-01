using UnityEngine;

public class WarCarController : UITimerEntity
{
    public int addNum = 1;

    public bool islimited = true;

    protected override void ApplyEffect()
    {
        if (islimited)
        {
            if (tank.currentData.orignalHP == tank.currentData.HP)
            {
                tank.AddOrignalHP(addNum);
            }
        }
        else
        {
            tank.AddOrignalHP(addNum);
            tank.AddHP(5*addNum);
        }
    }

    protected override void ShowTooltip()
    {
        TooltipUI.Instance.titleText.text = $"<b><color={Constant.TITLE_COLOR}>战车控制器</color></b>";
        TooltipUI.Instance.isFollow = true;

        string showText = "";
        if (islimited)
        {
            showText = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                       $"检测<color={Constant.VALUE_COLOR}>自身</color>是否满血,是则增加" +
                       $"<color={Constant.VALUE_COLOR}>{addNum}</color>点血量上限" +
                       $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";
        }
        else
        {
            showText = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                       $"增加<color={Constant.VALUE_COLOR}>自身</color>" +
                       $"<color={Constant.VALUE_COLOR}>{addNum}</color>点血量上限和" +
                       $"恢复<color={Constant.VALUE_COLOR}>{5*addNum}</color>点血量" +
                       $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";
        }

           
        TooltipUI.Instance.descriptionText.text = showText;
    }
}