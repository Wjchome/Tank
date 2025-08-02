using UnityEngine;
using UnityEngine.UI;


public class ShovelController : UITimerEntity
{
   
    public float stopTimerNum=10;
        
        protected override void ApplyEffect()
        {
            MapManager.Instance.isChange = true;
            MapManager.Instance.ironWallEndFrames = NetworkManager.Instance.currentFrame + Mathf.RoundToInt(stopTimerNum/ Constant.FrameInterval);
            MapManager.Instance.SetHomeWall(MapType.wall);
            tank.bombAnimator.Play("Shovel", 0, 0);
        }
        
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>工兵铲</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"将<color={Constant.VALUE_COLOR}>基地附近的地形</color>改造成墙" +
                  $"<color={Constant.VALUE_COLOR}>{stopTimerNum}</color>秒后恢复为砖墙" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
