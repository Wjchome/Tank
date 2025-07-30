
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class DisciplineController : UITimerEntity
    {
        public int targetNum = 1;
        public int damageNum = 1;
     
        protected override void ApplyEffect()
        {
            int num = targetNum;
            foreach(var a in EnemyManager.Instance.activeEnemies)
            {
                a.DamageHP(damageNum, tank);
                num--;
                if (num == 0)
                {
                    break;
                }
            }
        }
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>惩戒</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"对<color={Constant.VALUE_COLOR}>{targetNum}</color>造成" +
                  $"<color={Constant.VALUE_COLOR}>{damageNum}</color>伤害" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
