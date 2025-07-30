using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : SingletonMono<EntityManager>
{
    public Transform content;

    [Header("Map Entity Prefabs")]
    public AutoTurret autoTurretPrefab;
    public Landmine landminePrefab;
    public HealingGarden healingGardenPrefab;
    public SpikeTrap spikeTrapPrefab;

    [Header("Spawner Entity Prefabs")]
    public LandmineMachine landmineMachinePrefab;

    [Header("Effect Entity Prefabs")]
    public BombController bombControllerPrefab;
    public PocketWatchController pocketWatchControllerPrefab;
    public ShovelController shovelControllerPrefab;
    public SteelHelmetController steelHelmetControllerPrefab;
    public ProtectController protectControllerPrefab;
    public DisciplineController disciplineControllerPrefab;

    // 分类管理
    private List<MapEntity> mapEntities = new List<MapEntity>();
    private List<UITimerEntity> uiEntities = new List<UITimerEntity>();

    public void UpdateFrame()
    {
        // 更新地图实体
        foreach (var entity in mapEntities.ToList())
        {
            if (entity != null)
            {
                entity.UpdateFrame();
            }
            else
            {
                mapEntities.Remove(entity);
            }
        }
        
        // 更新UI实体
        foreach (var entity in uiEntities.ToList())
        {
            if (entity != null)
            {
                entity.UpdateFrame();
            }
            else
            {
                uiEntities.Remove(entity);
            }
        }
    }

    public void AddMapEntity(MapEntity entity)
    {
        mapEntities.Add(entity);
    }

    public void AddUIEntity(UITimerEntity entity)
    {
        uiEntities.Add(entity);
    }

    public void RemoveEntity(IEntity entity)
    {
        if (entity is MapEntity mapEntity)
        {
            mapEntities.Remove(mapEntity);
        }
        else if (entity is UITimerEntity uiEntity)
        {
            uiEntities.Remove(uiEntity);
        }
    }
    
    // 查找特定类型的UI实体
    public T FindUIEntity<T>(TankController tank) where T : UITimerEntity
    {
        foreach (var entity in uiEntities)
        {
            if (entity is T targetEntity && targetEntity.tank == tank)
            {
                return targetEntity;
            }
        }
        return null;
    }
    
    // 查找特定类型的地图实体
    public T FindMapEntity<T>(TankController tank) where T : MapEntity
    {
        foreach (var entity in mapEntities)
        {
            if (entity is T targetEntity && targetEntity.tank == tank)
            {
                return targetEntity;
            }
        }
        return null;
    }




    // 生成方法
    public T SpawnMapEntity<T>(T prefab, TankController tank, Vector2 position,Vector2Int pos) where T : MapEntity
    {
 
        T entity = Instantiate(prefab, new Vector2(position.x, position.y), Quaternion.identity);
        entity.Init(tank,pos);
        AddMapEntity(entity);
        return entity;
    }
    
    public T SpawnUIEntity<T>(T prefab, TankController tank) where T : UITimerEntity
    {
        T entity = Instantiate(prefab, content);
        entity.Init(tank);
        
        AddUIEntity(entity);
        return entity;
    }
    


    // 兼容性方法 - 保持原有接口
    public void InitAutoTurrent(TankController tank)
    {
        var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
            .Except(MapManager.Instance.GetAllTankPos()).ToList();
        var homePos = new List<int>()
        {
            (MapManager.Instance.mapWidth - 1) / 2 - 1,
            (MapManager.Instance.mapWidth - 1) / 2,
            MapManager.Instance.mapWidth / 2,
        };
        foreach (var spawnPo in spawnPos.ToList())
        {
            if (spawnPo.y == 0 || spawnPo.y == 1 || spawnPo.y == 2 ||
                homePos.Contains(spawnPo.x))
            {
                spawnPos.Remove(spawnPo);
            }
        }

        var pos = spawnPos[tank.random.Next(spawnPos.Count)];

        SpawnMapEntity(autoTurretPrefab, tank,new Vector2(pos.x+0.5f, pos.y+0.5f),pos);
    }

    public void InitLandmineMachine(TankController tank)
    {
        SpawnUIEntity(landmineMachinePrefab, tank);
    }

    public void InitLandmine(TankController tank)
    {
        var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
            .Except(MapManager.Instance.GetAllTankPos()).ToList();
        var pos = spawnPos[tank.random.Next(spawnPos.Count)];
        
        SpawnMapEntity(landminePrefab, tank, new Vector2(pos.x, pos.y), pos);
    }

    public void InitHealingGarden(TankController tank)
    {
        var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
            .Except(MapManager.Instance.GetAllTankPos()).ToList();
        var pos = spawnPos[tank.random.Next(spawnPos.Count)];
        SpawnMapEntity(healingGardenPrefab, tank,new Vector2(pos.x, pos.y), pos);
    }

    public void InitSpikeTrap(TankController tank, Vector2Int pos, int durationFrame)
    {
        var spikeTrap = SpawnMapEntity(spikeTrapPrefab, tank, new Vector2(pos.x, pos.y), pos);
        spikeTrap.durationFrame = durationFrame;
    }

    public void InitBombController(TankController tank)
    {
        SpawnUIEntity(bombControllerPrefab, tank);
    }

    public void InitPocketWatchController(TankController tank)
    {
        SpawnUIEntity(pocketWatchControllerPrefab, tank);
    }

    public void InitShovelController(TankController tank)
    {
        SpawnUIEntity(shovelControllerPrefab, tank);
    }

    public void InitSteelHelmetController(TankController tank)
    {
        SpawnUIEntity(steelHelmetControllerPrefab, tank);
    }

    public void InitProtectController(TankController tank)
    {
        SpawnUIEntity(protectControllerPrefab, tank);
    }

    public void InitDisciplineController(TankController tank)
    {
        SpawnUIEntity(disciplineControllerPrefab, tank);
    }
}