
    using UnityEngine;

    public class TimeStormController : UITimerEntity
    {

    
        public float stopTimerNum=10;

        public int damageNum;
        protected override void ApplyEffect()
        {
           
            
            long targetFrame = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(stopTimerNum / Constant.FrameInterval);
            EnemyManager.Instance.pauseEndFrame = targetFrame;
            
            EnemyManager.Instance.activeEnemies.ForEach((a) => a.DamageHP(damageNum, tank));
            
            
            tank.bombAnimator.Play("PocketWatch", 0, 0);
            
        }
        
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>时间风暴</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"暂停<color={Constant.VALUE_COLOR}>所有敌人</color>" +
                  $"<color={Constant.VALUE_COLOR}>{stopTimerNum}</color>秒并造成" +
                  $"<color={Constant.VALUE_COLOR}>{damageNum}</color>伤害" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
