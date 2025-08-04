
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProtectController : UITimerEntity
    {
        public Vector2Int Pos;


        public int damageNum = 1;
        protected override void ApplyEffect()
        {
            Pos = tank.Pos;
            List<Vector2Int> vets = FindAll();

            List<EnemyTankController> tanks 
                = EnemyManager.Instance.activeEnemies.FindAll(
                    a => vets.Contains(a.Pos)||vets.Contains(a.PosUp)||
                         vets.Contains(a.PosUpRight)||vets.Contains(a.PosRight));
            
            HashSet<TankController> tanksSet=new HashSet<TankController>(tanks);
            foreach (EnemyTankController a in tanksSet.ToList())
            {
                a.DamageHP(damageNum, tank,DamageType.Protect);
            }
            
            tank.bombAnimator.Play("Protect",0,0);
        }
        
        
// 1 1 1 1
// 1 0 0 1
// 1 0 0 1
// 1 1 1 1
        List<Vector2Int> FindAll()
        {
            List<Vector2Int> list = new List<Vector2Int>
            {
                new Vector2Int(Pos.x-1,Pos.y),
                new Vector2Int(Pos.x-1,Pos.y-1),
                new Vector2Int(Pos.x-1,Pos.y+1),
                new Vector2Int(Pos.x-1,Pos.y+2),
                new Vector2Int(Pos.x,Pos.y-1),
                new Vector2Int(Pos.x,Pos.y+2),
                new Vector2Int(Pos.x+1,Pos.y-1),
                new Vector2Int(Pos.x+1,Pos.y+2),
                new Vector2Int(Pos.x+2,Pos.y-1),
                new Vector2Int(Pos.x+2,Pos.y),
                new Vector2Int(Pos.x+2,Pos.y+1),
                new Vector2Int(Pos.x+2,Pos.y+2),
            };
            foreach (var item in list.ToList())
            {
                if (!MapManager.Instance.IsVailePos(item))
                {
                    list.Remove(item);
                }
            }
            
            return list;
        }
        
        protected override void ShowTooltip()
        {
            
            TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>保护</color></b>";
            TooltipUI.Instance.isFollow = true;
            TooltipUI.Instance.descriptionText.text
                = $"每隔<color={Constant.TIME_COLOR}>{genateIntervalFrame * Constant.FrameInterval}</color>秒" +
                  $"对<color={Constant.VALUE_COLOR}>自身一圈敌人</color>造成" +
                  $"<color={Constant.VALUE_COLOR}>{damageNum}</color>伤害" +
                  $"\n时间:<color={Constant.TIME_COLOR}>{((NetworkManager.Instance.currentFrame - lastTriggerFrame) * Constant.FrameInterval).ToString("F2")}</color>";

        }
    }
