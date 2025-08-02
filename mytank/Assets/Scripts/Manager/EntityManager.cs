using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : SingletonMono<EntityManager>
{
    public Transform content;

    [Header("Bullet Prefabs")] public BulletController bulletPrefab;

    [Header("Map Entity Prefabs")] public AutoTurret autoTurretPrefab;
    public Landmine landminePrefab;
    public HealingGarden healingGardenPrefab;
    public SpikeTrap spikeTrapPrefab;
    public GhostGuard ghostGuardPrefab;
    public AlmightyTurret almightyTurretPrefab;


    [Header("UI Timer Entity Prefabs")] public LandmineMachine landmineMachinePrefab;
    public BombController bombControllerPrefab;
    public PocketWatchController pocketWatchControllerPrefab;
    public ShovelController shovelControllerPrefab;
    public SteelHelmetController steelHelmetControllerPrefab;
    public ProtectController protectControllerPrefab;
    public DisciplineController disciplineControllerPrefab;
    public GhostGuardMachine ghostGuardMachinePrefab;
    public WarCarController warCarControllerPrefab;
    public AlmightyTurretMachine almightyTurretMachinePrefab;

    public TimeStormController timeStormControllerPrefab;

    // 分类管理
    private List<MapEntity> mapEntities = new List<MapEntity>();
    private List<UITimerEntity> uiEntities = new List<UITimerEntity>();

    // 子弹对象池
    public ObjectPool<BulletController> BulletPool { get; private set; }
    public List<BulletController> activeBullets = new List<BulletController>();

    
    public List<TankController> allTanks = new List<TankController>();
    
    private void Awake()
    {
        InitializeBulletPool();
    }

    private void InitializeBulletPool()
    {
        BulletPool = new ObjectPool<BulletController>(
            prefab: bulletPrefab,
            onSpawn: CreateBullet,
            onDespawn: KillBullet
        );


        activeBullets = new List<BulletController>();
    }

    private void CreateBullet(BulletController bullet)
    {
        bullet.isDead = false;
        bullet.gameObject.SetActive(true);
        bullet.isShouldDestroy = false;
        bullet.lastMoveTimeFrame = 0;
        bullet.animator.Play("Idle", 0, 0);
        activeBullets.Add(bullet);
    }

    private void KillBullet(BulletController bullet)
    {
        bullet.gameObject.SetActive(false);
        activeBullets.Remove(bullet);
    }

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

        // 更新子弹
        foreach (var bullet in activeBullets.ToList())
        {
            if (bullet != null)
            {
                bullet.UpdateFrame();
            }
        }
    }

    public void AddMapEntity(MapEntity entity)
    {
        mapEntities.Add(entity);
    }

    public void GameOver()
    {
        foreach (var entity in mapEntities.ToList())
        {
            entity.Destroy();
        }

        // 更新UI实体
        foreach (var entity in uiEntities.ToList())
        {
            entity.Destroy();
        }

        // 清理所有子弹
        foreach (var bullet in activeBullets.ToList())
        {
            if (bullet != null)
            {
                BulletPool.ReturnObject(bullet);
            }
        }
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
    public T FindFirstOrDefaultUIEntity<T>(TankController tank) where T : UITimerEntity
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

    public List<T> FindUIEntities<T>(TankController tank) where T : UITimerEntity
    {
        List<T> list = new List<T>();
        foreach (var entity in uiEntities)
        {
            if (entity is T targetEntity && targetEntity.tank == tank)
            {
                list.Add(targetEntity);
            }
        }

        return list;
    }


    // 查找特定类型的地图实体
    public T FindFirstOrDefaultMapEntity<T>(TankController tank) where T : MapEntity
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

    public List<T> FindMapEntities<T>(TankController tank) where T : MapEntity
    {
        List<T> list = new List<T>();
        foreach (var entity in mapEntities)
        {
            if (entity is T targetEntity && targetEntity.tank == tank)
            {
                list.Add(targetEntity);
            }
        }

        return list;
    }

    // 生成方法
    private T SpawnMapEntity<T>(T prefab, PlayerTankController tank, Vector2 position, Vector2Int pos)
        where T : MapEntity
    {
        T entity = Instantiate(prefab, new Vector2(position.x, position.y), Quaternion.identity);
        entity.Init(tank, pos);
        AddMapEntity(entity);
        return entity;
    }

    private T SpawnUIEntity<T>(T prefab, PlayerTankController tank) where T : UITimerEntity
    {
        T entity = Instantiate(prefab, content);
        entity.Init(tank);

        AddUIEntity(entity);
        return entity;
    }

    #region 生成方法

    // 子弹生成方法
    public void InitializeBullet(Direction direction, TankController tank, Vector2Int pos, int damageNum)
    {
        BulletController bullet = BulletPool.GetObject();
        bullet.direction = direction;
        bullet.tank = tank;

        if (tank.identity == Identity.Myself || tank.identity == Identity.OtherPlayer)
            bullet.isPlayerBullet = true;
        else
        {
            bullet.isPlayerBullet = false;
        }

        // 设置子弹朝向
        Vector3 rotation = Vector3.zero;
        bullet.dir = direction.ToVector2Int();
        bullet.GetComponent<SpriteRenderer>().material.color = tank.playerColor;

        switch (direction)
        {
            case Direction.Up:
                rotation = new Vector3(0, 0, 0);
                break;
            case Direction.Down:
                rotation = new Vector3(0, 0, 180);
                break;
            case Direction.Left:
                rotation = new Vector3(0, 0, 90);
                break;
            case Direction.Right:
                rotation = new Vector3(0, 0, -90);
                break;
        }

        bullet.transform.rotation = Quaternion.Euler(rotation);
        bullet.Pos = pos;
        bullet.transform.position = bullet.GetCenter();
        bullet.damageNum = damageNum;

        if (tank is PlayerTankController playerTank)
        {
            if (playerTank.isPenetrate)
            {
                bullet.penetrationCount = 3;
            }
            else
            {
                bullet.penetrationCount = 1;
            }
        }
        else
        {
            bullet.penetrationCount = 1;
        }


        // 调用基类的Init方法
        // bullet.Init(tank, pos);
    }


    // 兼容性方法 - 保持原有接口
    public void InitAutoTurrent(PlayerTankController tank)
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

        SpawnMapEntity(autoTurretPrefab, tank, new Vector2(pos.x + 0.5f, pos.y + 0.5f), pos);
    }

    public void InitLandmine(PlayerTankController tank)
    {
        var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
            .Except(MapManager.Instance.GetAllTankPos()).ToList();
        var pos = spawnPos[tank.random.Next(spawnPos.Count)];

        SpawnMapEntity(landminePrefab, tank, new Vector2(pos.x, pos.y), pos);
    }

    public void InitHealingGarden(PlayerTankController tank)
    {
        var spawnPos = MapManager.Instance.GetTwoMapTypePos(new List<MapType>() { MapType.floor })
            .Except(MapManager.Instance.GetAllTankPos()).ToList();
        var pos = spawnPos[tank.random.Next(spawnPos.Count)];
        SpawnMapEntity(healingGardenPrefab, tank, new Vector2(pos.x, pos.y), pos);
    }

    public void InitSpikeTrap(PlayerTankController tank, Vector2Int pos, int durationFrame)
    {
        var spikeTrap = SpawnMapEntity(spikeTrapPrefab, tank, new Vector2(pos.x, pos.y), pos);
        spikeTrap.durationFrame = durationFrame;
    }

    public void InitGhostGuard(PlayerTankController tank)
    {
        List<Vector2Int> poss = new List<Vector2Int>()
        {
            new Vector2Int((MapManager.Instance.mapWidth - 1) / 2 - 1, 1),
            new Vector2Int((MapManager.Instance.mapWidth) / 2, 1),
            new Vector2Int((MapManager.Instance.mapWidth - 1) / 2 - 1, 3),
            new Vector2Int((MapManager.Instance.mapWidth) / 2, 3),
        };


        Color tankColor = tank.playerColor;
        Color transparentColor = new Color(tankColor.r, tankColor.g, tankColor.b, 0.5f); // 半透明白色

        var ghostGuardLeft = SpawnMapEntity(ghostGuardPrefab, tank, new Vector2(poss[0].x + 0.5f, poss[0].y + 0.5f),
            poss[0]);
        ghostGuardLeft.moveDirection = Direction.Left;
        ghostGuardLeft.GetComponent<SpriteRenderer>().color = transparentColor;
        ghostGuardLeft.transform.rotation = Quaternion.Euler(0, 0, 90);

        var ghostGuardRight = SpawnMapEntity(ghostGuardPrefab, tank, new Vector2(poss[1].x + 0.5f, poss[1].y + 0.5f),
            poss[1]);
        ghostGuardRight.moveDirection = Direction.Right;
        ghostGuardRight.GetComponent<SpriteRenderer>().color = transparentColor;
        ghostGuardRight.transform.rotation = Quaternion.Euler(0, 0, -90);

        var ghostGuardUp1 = SpawnMapEntity(ghostGuardPrefab, tank, new Vector2(poss[2].x + 0.5f, poss[2].y + 0.5f),
            poss[2]);
        ghostGuardUp1.moveDirection = Direction.Up;
        ghostGuardUp1.GetComponent<SpriteRenderer>().color = transparentColor;

        var ghostGuardUp2 = SpawnMapEntity(ghostGuardPrefab, tank, new Vector2(poss[3].x + 0.5f, poss[3].y + 0.5f),
            poss[3]);
        ghostGuardUp2.moveDirection = Direction.Up;
        ghostGuardUp2.GetComponent<SpriteRenderer>().color = transparentColor;
    }

    public void InitAlmightyTurret(PlayerTankController tank)
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

        SpawnMapEntity(almightyTurretPrefab, tank, new Vector2(pos.x + 0.5f, pos.y + 0.5f), pos);
    }

    public void InitAlmightyTurret(PlayerTankController tank, Vector2Int pos)
    {
        var homePos = new List<int>()
        {
            (MapManager.Instance.mapWidth - 1) / 2 - 1,
            (MapManager.Instance.mapWidth - 1) / 2,
            MapManager.Instance.mapWidth / 2,
        };
        if (pos.y == 0 || pos.y == 1 || pos.y == 2 ||
            homePos.Contains(pos.x))
        {
            return;
        }

        SpawnMapEntity(almightyTurretPrefab, tank, new Vector2(pos.x + 0.5f, pos.y + 0.5f), pos);
    }

    public void InitAlmightyTurretMachine(PlayerTankController tank)
    {
        SpawnUIEntity(almightyTurretMachinePrefab, tank);
    }

    public void InitLandmineMachine(PlayerTankController tank)
    {
        SpawnUIEntity(landmineMachinePrefab, tank);
    }

    public void InitBombController(PlayerTankController tank)
    {
        SpawnUIEntity(bombControllerPrefab, tank);
    }

    public void InitPocketWatchController(PlayerTankController tank)
    {
        SpawnUIEntity(pocketWatchControllerPrefab, tank);
    }

    public void InitShovelController(PlayerTankController tank)
    {
        SpawnUIEntity(shovelControllerPrefab, tank);
    }

    public void InitSteelHelmetController(PlayerTankController tank)
    {
        SpawnUIEntity(steelHelmetControllerPrefab, tank);
    }

    public void InitProtectController(PlayerTankController tank)
    {
        SpawnUIEntity(protectControllerPrefab, tank);
    }

    public void InitDisciplineController(PlayerTankController tank)
    {
        SpawnUIEntity(disciplineControllerPrefab, tank);
    }

    public void InitGhostGuardMachine(PlayerTankController tank)
    {
        SpawnUIEntity(ghostGuardMachinePrefab, tank);
    }

    public void InitWarCarController(PlayerTankController tank)
    {
        SpawnUIEntity(warCarControllerPrefab, tank);
    }

    public void InitTimeStormController(PlayerTankController tank)
    {
        SpawnUIEntity(timeStormControllerPrefab, tank);
    }

    #endregion
}