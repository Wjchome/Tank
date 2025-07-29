
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EntityManager:SingletonMono<EntityManager>
    {

        public Transform content;
        
        
        public AutoTurret autoTurretPrefab;
    
    
        public LandmineMachine landmineMachinePrefab;
        public Landmine landminePrefab;
    
        public HealingGarden healingGardenPrefab;
        
        public SpikeTrap spikeTrapPrefab;

        public BombController bombControllerPrefab;
        
        public PocketWatchController pocketWatchControllerPrefab;
        
        public ShovelController shovelControllerPrefab;
        
        public SteelHelmetController steelHelmetControllerPrefab;
        
        public ProtectController  protectControllerPrefab;
        
        public DisciplineController disciplineControllerPrefab;
        
        public List<AutoTurret>  autoTurrets = new List<AutoTurret>();
        
        public List<LandmineMachine>  landmineMachines = new List<LandmineMachine>();
        public List<Landmine>  landmines = new List<Landmine>();
        
        public List<HealingGarden> healingGardens = new List<HealingGarden>();
        
        public List<SpikeTrap> spikeTraps = new List<SpikeTrap>();

        public List<BombController> bombControllers = new List<BombController>();
        
        public List<PocketWatchController> pocketWatchControllers = new List<PocketWatchController>();
      
        public List<ShovelController> shovelControllers = new List<ShovelController>();
        
        public List<SteelHelmetController> steelHelmetControllers = new List<SteelHelmetController>();
     
        public List<ProtectController> protectControllers = new List<ProtectController>();
        
        public List<DisciplineController> disciplines = new List<DisciplineController>();
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

            foreach (var spikeTrap in spikeTraps.ToList())
            {
                if (spikeTrap != null)
                {
                    spikeTrap.UpdateFrame();
                    
                }
                else
                {
                    spikeTraps.Remove(spikeTrap);
                }
            }

            foreach (var bombController in bombControllers)
            {
                bombController.UpdateFrame();
            }

            foreach (var pocketWatchController in pocketWatchControllers)
            {
                pocketWatchController.UpdateFrame();
            }

            foreach (var shovelController in shovelControllers)
            {
                shovelController.UpdateFrame();
            }

            foreach (var steelHelmetController in steelHelmetControllers)
            {
                steelHelmetController.UpdateFrame();
            }

            foreach (var protectController in protectControllers)
            {
                protectController.UpdateFrame();
            }

            foreach (var discipline in disciplines)
            {
                discipline.UpdateFrame();
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
            turret.tank = tank;
            autoTurrets.Add(turret);
            
        }

        public void InitLandmineMachine(TankController tank)
        {
          
               
            LandmineMachine landmineMachine = Instantiate(landmineMachinePrefab, content);
            landmineMachine.tank = tank;
            landmineMachine.lastTriggerFrame = -landmineMachine.genateIntervalFrame;
            landmineMachines.Add(landmineMachine);
        }

        public void InitLandmine(TankController tank )
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

        public void InitSpikeTrap(TankController tank, Vector2Int pos,int durationFrame)
        {
            SpikeTrap spikeTrap=Instantiate(spikeTrapPrefab,new Vector2(pos.x , pos.y),Quaternion.identity);
            
            spikeTrap.tank = tank;
            spikeTrap.Pos = pos;
            spikeTrap.durationFrame = 80;
            spikeTraps.Add(spikeTrap);
        }

        public void InitBombController(TankController tank)
        {
            var bombC = Instantiate(bombControllerPrefab,content);
            bombC.tank = tank;
            bombC.lastTriggerFrame = -bombC.genateIntervalFrame;
            bombControllers.Add(bombC);
            
        }
        
        public void InitPocketWatchController(TankController tank)
        {
            var pocketWatchController = Instantiate(pocketWatchControllerPrefab,content);
            pocketWatchController.tank = tank;
            pocketWatchController.lastTriggerFrame = -pocketWatchController.genateIntervalFrame;
            pocketWatchControllers.Add(pocketWatchController);
            
        }
        public void InitShovelController(TankController tank)
        {
            var shovelController = Instantiate(shovelControllerPrefab,content);
            shovelController.tank = tank;
            shovelController.lastTriggerFrame = -shovelController.genateIntervalFrame;
            shovelControllers.Add(shovelController);
            
        }
        public void InitSteelHelmetController(TankController tank)
        {
            var steelHelmet = Instantiate(steelHelmetControllerPrefab,content);
            steelHelmet.tank = tank;
            steelHelmet.lastTriggerFrame = -steelHelmet.genateIntervalFrame;
            steelHelmetControllers.Add(steelHelmet);
            
        }
        
        public void InitProtectController(TankController tank)
        {
            var protectController = Instantiate(protectControllerPrefab,content);
            protectController.tank = tank;
            protectController.lastTriggerFrame = -protectController.genateIntervalFrame;
            protectControllers.Add(protectController);
            
        }

        public void InitDisciplineController(TankController tank)
        {
            var disciplineController = Instantiate(disciplineControllerPrefab,content);
            disciplineController.tank = tank;
            disciplineController.lastTriggerFrame = -disciplineController.genateIntervalFrame;
            disciplines.Add(disciplineController);

        }
        
    }
