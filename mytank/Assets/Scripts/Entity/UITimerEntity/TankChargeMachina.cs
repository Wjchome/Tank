
    public class TankChargeMachina:UITimerEntity
    {
        protected override void ApplyEffect()
        {
            EntityManager.Instance.InitTankCharge(tank);

        }
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>冲刺坦克生成器</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"释放<color={Constant.VALUE_COLOR}>1</color>个冲刺坦克" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
