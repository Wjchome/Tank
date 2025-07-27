
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EntityManager:SingletonMono<EntityManager>
    {
        public AutoTurret autoTurretPrefab;
    
    
        public LandmineMachine landmineMachinePrefab;
        public Landmine landminePrefab;
    
        public HealingGarden healingGardenPrefab;
        
        public List<AutoTurret>  autoTurrets = new List<AutoTurret>();
        
        public List<LandmineMachine>  landmineMachines = new List<LandmineMachine>();
        public List<Landmine>  landmines = new List<Landmine>();
        
        public List<HealingGarden> healingGardens = new List<HealingGarden>();

        public void UpdateFrame()
        {
            foreach (AutoTurret turret in autoTurrets)
            {
                turret.UpdateFrame();
            }

            foreach (var landmineMachine in landmineMachines )
            {
                landmineMachine.UpdateFrame();
            }

            foreach (var healingGarden in healingGardens)
            {
                healingGarden.UpdateFrame();
            }
            
        }


        public void InitAutoTurrent(TankController tank)
        {
            var spawnPos=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                .Except(MapManager.Instance.GetAllTankPos()).ToList();
            var pos = spawnPos[tank.random.Next(spawnPos.Count)];
               
            AutoTurret turret = Instantiate(autoTurretPrefab, 
                new Vector2(pos.x+0.5f , pos.y+0.5f), 
                Quaternion.identity);
            
            turret.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
            turret.Pos = pos;
            autoTurrets.Add(turret);
            
        }

        public void InitLandmineMachine(TankController tank)
        {
            var spawnPos=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                .Except(MapManager.Instance.GetAllTankPos()).ToList();
            var pos = spawnPos[tank.random.Next(spawnPos.Count)];
               
            LandmineMachine landmineMachine = Instantiate(landmineMachinePrefab, 
                new Vector2(pos.x , pos.y), 
                Quaternion.identity);
            landmineMachine.tank = tank;
            landmineMachine.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
            landmineMachine.Pos = pos;
            landmineMachines.Add(landmineMachine);
        }

        public void InitLandmine(TankController tank)
        {
            var spawnPos=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                .Except(MapManager.Instance.GetAllTankPos()).ToList();
            var pos = spawnPos[tank.random.Next(spawnPos.Count)];
               
            Landmine landmine = Instantiate(landminePrefab, 
                new Vector2(pos.x , pos.y), 
                Quaternion.identity);
            landmine.tank = tank;
            landmine.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
            landmine.Pos = pos;
            landmines.Add(landmine);
        }

        public void InitHealingGarden(TankController tank)
        {
            var spawnPos=MapManager.Instance.GetMapTypePos(new List<MapType>() { MapType.floor })
                .Except(MapManager.Instance.GetAllTankPos()).ToList();
            var pos = spawnPos[tank.random.Next(spawnPos.Count)];
            HealingGarden healingGarden = Instantiate(healingGardenPrefab, 
                new Vector2(pos.x , pos.y), 
                Quaternion.identity).GetComponent<HealingGarden>();
            
            healingGarden .Pos=pos;
            healingGarden.color=tank.playerColor;
            healingGarden.GetComponent<SpriteRenderer>().material.color = tank.playerColor;
                
            healingGardens.Add(healingGarden);
        }
        
    }
