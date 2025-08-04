
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class BombController : UITimerEntity
    {

        public int damageNum;


        protected override void ApplyEffect()
        {
            EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(damageNum, tank,DamageType.Bomb));
            tank.bombAnimator.Play("BombShow", 0, 0);
        }

        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>炸弹控制</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"对<color={Constant.VALUE_COLOR}>所有敌人</color>造成" +
                  $"<color={Constant.VALUE_COLOR}>{damageNum}</color>伤害" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
        


    }
