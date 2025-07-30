
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProtectController : EffectEntity
    {
        public Vector2Int Pos;
    
        
        
        protected override void ApplyEffect()
        {
            Pos = tank.Pos;
            List<Vector2Int> vets = FindAll();

            List<TankController> tanks 
                = EnemyManager.Instance.activeEnemies.FindAll(
                    a => vets.Contains(a.Pos)||vets.Contains(a.PosUp)||
                         vets.Contains(a.PosUpRight)||vets.Contains(a.PosRight));
            
            HashSet<TankController> tanksSet=new HashSet<TankController>(tanks);
            foreach (TankController a in tanksSet.ToList())
            {
                a.DamageHP(1, tank);
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
                if (!MapManager.Instance.isVailePos(item))
                {
                    list.Remove(item);
                }
            }
            
            return list;
        }
        
    }
