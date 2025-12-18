using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using Physics2D;
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
    public PulseTurret pulseTurretPrefab;
    public TankCharge tankChargePrefab;


    [Header("UI Timer Entity Prefabs")] public LandmineMachine landmineMachinePrefab;
    public BombController bombControllerPrefab;
    public PocketWatchController pocketWatchControllerPrefab;
    public ShovelController shovelControllerPrefab;
    public SteelHelmetController steelHelmetControllerPrefab;
    public ProtectController protectControllerPrefab;
    public DisciplineController disciplineControllerPrefab;
    public GhostGuardMachine ghostGuardMachinePrefab;
    public WarCarController warCarControllerPrefab;
    public TankChargeMachina tankChargeMachinePrefab;
    public AlmightyTurretMachine almightyTurretMachinePrefab;
    public TimeStormController timeStormControllerPrefab;

    // 分类管理
    private List<MapEntity> mapEntities = new List<MapEntity>();
    private List<UITimerEntity> uiEntities = new List<UITimerEntity>();

    // 子弹对象池
    public ObjectPool<BulletController> BulletPool { get; private set; }
    public List<BulletController> activeBullets = new List<BulletController>();


    public List<TankController> allTanks = new List<TankController>();
    public Dictionary<Vector2Int, TankController> tankPositionCache = new Dictionary<Vector2Int, TankController>();

    private List<MapEntity> mapEntitiesToUpdate = new List<MapEntity>();
    private List<UITimerEntity> uiEntitiesToUpdate = new List<UITimerEntity>();
    private List<BulletController> bulletsToUpdate = new List<BulletController>();

    public void UpdateTankPos(TankController tank, Vector2Int originPos, Vector2Int targetPos)
    {
        tankPositionCache[originPos] = null;
        tankPositionCache[originPos + new Vector2Int(0, 1)] = null;
        tankPositionCache[originPos + new Vector2Int(1, 1)] = null;
        tankPositionCache[originPos + new Vector2Int(1, 0)] = null;
        tankPositionCache[targetPos] = tank;
        tankPositionCache[targetPos + new Vector2Int(0, 1)] = tank;
        tankPositionCache[targetPos + new Vector2Int(1, 1)] = tank;
        tankPositionCache[targetPos + new Vector2Int(1, 0)] = tank;
    }

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
        mapEntitiesToUpdate.Clear();
        mapEntitiesToUpdate.AddRange(mapEntities);
        for (int i = mapEntitiesToUpdate.Count - 1; i >= 0; i--)
        {
            var entity = mapEntitiesToUpdate[i];
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
        uiEntitiesToUpdate.Clear();
        uiEntitiesToUpdate.AddRange(uiEntities);
        for (int i = uiEntitiesToUpdate.Count - 1; i >= 0; i--)
        {
            var entity = uiEntitiesToUpdate[i];
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
        bulletsToUpdate.Clear();
        bulletsToUpdate.AddRange(activeBullets);
        for (int i = bulletsToUpdate.Count - 1; i >= 0; i--)
        {
            var bullet = bulletsToUpdate[i];
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

        allTanks.Clear();
        tankPositionCache.Clear();
    }

    // 清除坦克缓存（用于坦克死亡时）
    public void ClearTankFromCache(TankController tank)
    {
        // if (tank != null)
        // {
        //     Vector2Int pos;//= tank.Pos;
        //     tankPositionCache[pos] = null;
        //     tankPositionCache[pos + new Vector2Int(0, 1)] = null;
        //     tankPositionCache[pos + new Vector2Int(1, 1)] = null;
        //     tankPositionCache[pos + new Vector2Int(1, 0)] = null;
        // }
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
    private T SpawnMapEntity<T>(T prefab, PlayerTankController tank, FixRect fixRect)
        where T : MapEntity
    {
        T entity = Instantiate(prefab, (Vector2)fixRect.Center, Quaternion.identity);
        entity.Init(tank, fixRect);
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
    public void InitializeBullet(FixVector2 dir, TankController tank, FixVector2 pos, int damageNum)
    {
        dir.Normalize();
        BulletController bullet = BulletPool.GetObject();

        bullet.tank = tank;
        if (tank.identity == Identity.Myself || tank.identity == Identity.OtherPlayer)
            bullet.isPlayerBullet = true;
        else
        {
            bullet.isPlayerBullet = false;
        }

        // 设置子弹朝向
        Fix64 rotation = dir.ToRotation();
        bullet.dir = dir;

        bullet.GetComponent<SpriteRenderer>().material.color = tank.playerColor;


        PhysicsLayer layer = bullet.isPlayerBullet
            ? PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletFriend)
            : PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy);
        // QuadTreeV3.QuadTreeV3.Instance.AddObject(bullet.myFixRect, bullet.gameObject,layer);
        PhysicsWorld2DComponent.Instance.AddRigidBody(bullet.GetComponent<RigidBody2DComponent>(),pos,layer);
        bullet.transform.position = (Vector2)bullet.myFixRect.Center;
        bullet.rigidBody2D.Body.ApplyImpulse(dir*bullet.moveSpeedF);


        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, (float)rotation));
        //bullet.transform.position = bullet.GetCenter();
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


    /// <summary>
    /// 生成一个不与墙体碰撞的随机位置（使用四叉树检测）
    /// </summary>
    /// <param name="entitySize">实体大小（Fix64，通常为1.0）</param>
    /// <param name="maxAttempts">最大尝试次数</param>
    /// <returns>有效的FixRect位置，如果找不到则返回null</returns>
    private FixRect? GetRandomValidPosition(Fix64 entitySize, int maxAttempts = 50)
    {
        if (MapManager.Instance == null) return null;

        // 获取所有可用的floor位置作为候选（使用网格系统快速筛选）
        List<Vector2Int> floorPositions = MapManager.Instance.GetMapTypePos(new List<MapType> { MapType.floor });

        if (floorPositions.Count == 0)
        {
            Debug.LogWarning("No floor positions available for random spawn");
            return null;
        }

        // 随机打乱候选位置
        System.Random random = new System.Random();
        floorPositions = floorPositions.OrderBy(x => random.Next()).ToList();

        // 尝试每个候选位置
        int attempts = 0;
        foreach (var gridPos in floorPositions)
        {
            if (attempts >= maxAttempts) break;
            attempts++;

            // 将网格坐标转换为Fix64世界坐标（网格中心）
            Fix64 worldX = (Fix64)gridPos.x * MapManager.Instance.GridSizeF + MapManager.Instance.HalfGridSizeF;
            Fix64 worldY = (Fix64)gridPos.y * MapManager.Instance.GridSizeF + MapManager.Instance.HalfGridSizeF;

            // 创建实体的FixRect（以中心为基准）
            FixRect testRect = new FixRect(
                worldX - entitySize / (Fix64)2,
                worldY - entitySize / (Fix64)2,
                entitySize,
                entitySize
            );

            // 使用四叉树检查是否与墙体碰撞
            if (IsPositionValid(testRect))
            {
                return testRect;
            }
        }

        Debug.LogWarning($"Failed to find valid position after {attempts} attempts");
        return null;
    }

    /// <summary>
    /// 检查位置是否有效（不与墙体碰撞）
    /// </summary>
    private bool IsPositionValid(FixRect rect)
    {
        // // 查询四叉树中与目标矩形重叠的物体
        // List<QuadTreeObject> collidingObjects = QuadTreeV3.QuadTreeV3.Instance.Query(rect);
        //
        // // 检查是否与墙体碰撞
        // foreach (var obj in collidingObjects)
        // {
        //     // 检查是否是墙体或可破坏墙体
        //     if (obj.Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.Wall)) ||
        //         obj.Layer.Intersects(QuadTreeLayer.GetLayer((int)QuadTreeLayerType.BreakableWall)))
        //     {
        //         return false; // 与墙体碰撞，位置无效
        //     }
        // }

        return true; // 位置有效
    }

    // 兼容性方法 - 保持原有接口
    public void InitAutoTurrent(PlayerTankController tank)
    {
        // 获取随机有效位置（实体大小默认为1.0）
        FixRect? randomPos = GetRandomValidPosition((Fix64)1.0f);

        if (randomPos.HasValue)
        {
            SpawnMapEntity(autoTurretPrefab, tank, randomPos.Value);
        }
        else
        {
            Debug.LogWarning("Failed to spawn AutoTurret: no valid position found");
        }
    }

    public void InitLandmine(PlayerTankController tank)
    {
        FixRect? randomPos = GetRandomValidPosition((Fix64)1.0f);
        if (randomPos.HasValue)
        {
            SpawnMapEntity(landminePrefab, tank, randomPos.Value);
        }
        else
        {
            Debug.LogWarning("Failed to spawn Landmine: no valid position found");
        }
    }

    public void InitHealingGarden(PlayerTankController tank)
    {
        FixRect? randomPos = GetRandomValidPosition((Fix64)1.0f);
        if (randomPos.HasValue)
        {
            SpawnMapEntity(healingGardenPrefab, tank, randomPos.Value);
        }
        else
        {
            Debug.LogWarning("Failed to spawn HealingGarden: no valid position found");
        }
    }

    public void InitSpikeTrap(PlayerTankController tank, Vector2Int pos, int durationFrame)
    {
        // var spikeTrap = SpawnMapEntity(spikeTrapPrefab, tank, tank.myFixRect);
        // spikeTrap.durationFrame = durationFrame;
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

        // var ghostGuardLeft = SpawnMapEntity(ghostGuardPrefab, tank, tank.myFixRect);
        // ghostGuardLeft.moveDirection = Direction.Left;
        // ghostGuardLeft.spriteRenderer.color = transparentColor;
        // ghostGuardLeft.transform.rotation = Quaternion.Euler(0, 0, 90);
        //
        // var ghostGuardRight = SpawnMapEntity(ghostGuardPrefab, tank, tank.myFixRect);
        // ghostGuardRight.moveDirection = Direction.Right;
        // ghostGuardRight.spriteRenderer.color = transparentColor;
        // ghostGuardRight.transform.rotation = Quaternion.Euler(0, 0, -90);
        //
        // var ghostGuardUp1 = SpawnMapEntity(ghostGuardPrefab, tank, tank.myFixRect);
        // ghostGuardUp1.moveDirection = Direction.Up;
        // ghostGuardUp1.spriteRenderer.color = transparentColor;
        //
        // var ghostGuardUp2 = SpawnMapEntity(ghostGuardPrefab, tank, tank.myFixRect);
        // ghostGuardUp2.moveDirection = Direction.Up;
        // ghostGuardUp2.spriteRenderer.color = transparentColor;
    }

    public void InitAlmightyTurret(PlayerTankController tank)
    {
        FixRect? randomPos = GetRandomValidPosition((Fix64)1.0f);
        if (randomPos.HasValue)
        {
            SpawnMapEntity(almightyTurretPrefab, tank, randomPos.Value);
        }
        else
        {
            Debug.LogWarning("Failed to spawn AlmightyTurret: no valid position found");
        }
    }

    public void InitAlmightyTurret(PlayerTankController tank, FixRect fixRect)
    {
        // 如果提供了固定位置，直接使用（可能是特殊逻辑）
        if (IsPositionValid(fixRect))
        {
            SpawnMapEntity(almightyTurretPrefab, tank, fixRect);
        }
        else
        {
            Debug.LogWarning("Provided position for AlmightyTurret is invalid, trying random position");
            InitAlmightyTurret(tank); // 回退到随机位置
        }
    }

    public void InitPulseTurret(PlayerTankController tank)
    {
        FixRect? randomPos = GetRandomValidPosition((Fix64)1.0f);
        if (randomPos.HasValue)
        {
            SpawnMapEntity(pulseTurretPrefab, tank, randomPos.Value);
        }
        else
        {
            Debug.LogWarning("Failed to spawn PulseTurret: no valid position found");
        }
    }

    public void InitTankCharge(PlayerTankController tank, bool isCanBullet)
    {
        int a = tank.random.Next(4);
        Direction dir = Direction.Up;
        Vector2Int spawnPos = Vector2Int.zero;
        Vector3 rot = Vector3.forward;
        switch (a)
        {
            case 0:
                dir = Direction.Up;
                spawnPos = new Vector2Int(tank.random.Next(MapManager.Instance.mapWidth - 2), 0);
                rot = new Vector3(0, 0, 0);
                break;
            case 1:
                dir = Direction.Left;
                spawnPos = new Vector2Int(MapManager.Instance.mapWidth - 2,
                    tank.random.Next(MapManager.Instance.mapHeight - 2));
                rot = new Vector3(0, 0, 90);

                break;
            case 2:
                dir = Direction.Down;
                spawnPos = new Vector2Int(tank.random.Next(MapManager.Instance.mapWidth - 2),
                    MapManager.Instance.mapHeight - 2);
                rot = new Vector3(0, 0, 180);

                break;
            case 3:
                dir = Direction.Right;
                spawnPos = new Vector2Int(0, tank.random.Next(MapManager.Instance.mapHeight - 2));
                rot = new Vector3(0, 0, -90);
                break;
        }

        Color tankColor = tank.playerColor;
        Color transparentColor = new Color(tankColor.r, tankColor.g, tankColor.b, 0.5f); // 半透明白色

        // var tankCharge = SpawnMapEntity(tankChargePrefab, tank, tank.myFixRect);
        // tankCharge.transform.rotation = Quaternion.Euler(rot);
        // tankCharge.moveDirection = dir;
        // tankCharge.isCanBullet = isCanBullet;
        // tankCharge.GetComponent<SpriteRenderer>().color = transparentColor;
    }

    public void InitTankCharge(PlayerTankController tank, bool isCanBullet, Vector2Int pos)
    {
        int a = tank.random.Next(4);
        Direction dir = Direction.Up;
        Vector3 rot = Vector3.forward;
        switch (a)
        {
            case 0:
                dir = Direction.Up;
                rot = new Vector3(0, 0, 0);
                break;
            case 1:
                dir = Direction.Left;
                rot = new Vector3(0, 0, 90);

                break;
            case 2:
                dir = Direction.Down;
                rot = new Vector3(0, 0, 180);

                break;
            case 3:
                dir = Direction.Right;
                rot = new Vector3(0, 0, -90);
                break;
        }

        Color tankColor = tank.playerColor;
        Color transparentColor = new Color(tankColor.r, tankColor.g, tankColor.b, 0.5f); // 半透明白色

        // var tankCharge = SpawnMapEntity(tankChargePrefab, tank, tank.myFixRect);
        // tankCharge.transform.rotation = Quaternion.Euler(rot);
        // tankCharge.moveDirection = dir;
        // tankCharge.isCanBullet = isCanBullet;
        // tankCharge.GetComponent<SpriteRenderer>().color = transparentColor;
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

    public void InitTankChargeMachine(PlayerTankController tank)
    {
        SpawnUIEntity(tankChargeMachinePrefab, tank);
    }

    public void InitTimeStormController(PlayerTankController tank)
    {
        SpawnUIEntity(timeStormControllerPrefab, tank);
    }

    #endregion
}