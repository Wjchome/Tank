using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public enum Direction
{
    Up,
    Left,
    Down,
    Right
}


public enum MapType
{
    error = -1,
    floor,
    wall,
    breakableWall,
    river,
    tree,
    ice,
    home
}

public class MapManager : SingletonMono<MapManager>
{
    [Header("Map Settings")] public int mapWidth = 20;
    public int mapHeight = 20;
    public float gridSize = 1f;


    [Header("Prefabs")] public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject breakableWallPrefab;
    public GameObject riverPrefab;
    public GameObject treePrefab;
    public GameObject icePrefab;
    public GameObject homePrefab;
    public Transform wallsParent;

    private MapType[,] map;
    private Dictionary<MapType, GameObject> typeToPrefab;
    private GameObject[,] gridObjects; // 记录每个格子的实例

    public long ironWallEndFrames;
    public bool isChange;
    
    private void Awake()
    {
        typeToPrefab = new Dictionary<MapType, GameObject>()
        {
            { MapType.floor, floorPrefab },
            { MapType.wall, wallPrefab },
            { MapType.breakableWall, breakableWallPrefab },
            { MapType.river, riverPrefab }, // 河流
            { MapType.tree, treePrefab }, // 树荫
            { MapType.ice, icePrefab }, // 树荫
            { MapType.home, homePrefab }, // 树荫
        };
    }

    public void UpdateFrame()
    {
        //处理道具
        if (isChange && NetworkManager.Instance.currentFrame >= ironWallEndFrames)
        {
            SetHomeWall(MapType.breakableWall);
            isChange = false;
        }
    }

    public void SetHomeWall(MapType mapType)
    {
        List<Vector2Int> pos = new List<Vector2Int>
        {
            new Vector2Int((mapWidth - 1) / 2, 1),
            new Vector2Int(mapWidth / 2, 1),
            new Vector2Int((mapWidth - 1) / 2, 2),
            new Vector2Int(mapWidth / 2, 2),
        };
        for (int i = mapWidth / 2 - 3; i <= mapWidth / 2 + 2; i++)
        {
            for (int j = 1; j <= 4; j++)
            {
                Vector2Int pos1 = new Vector2Int(i, j);
                if (!pos.Contains(pos1))
                    SetWallType(i, j, mapType);
            }
        }
    }

    public bool HasTankInMapTypes(int x, int y, List<MapType> types)
    {
        MapType type = GetWallType(x, y);
        if (types.Contains(type))
            return true;
        type = GetWallType(x, y + 1);
        if (types.Contains(type))
            return true;
        type = GetWallType(x + 1, y + 1);
        if (types.Contains(type))
            return true;
        type = GetWallType(x + 1, y);
        if (types.Contains(type))
            return true;
        return false;
    }


    // 获取指定位置的墙类型
    public MapType GetWallType(int x, int y)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
        {
            return map[x, y];
        }

        return MapType.error; // 超出边界
    }

    // 设置指定位置的墙类型
    public void SetWallType(int x, int y, MapType wallType)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
        {
            map[x, y] = wallType;

            if (gridObjects[x, y] != null)
            {
                Destroy(gridObjects[x, y]);
                gridObjects[x, y] = null;
            }

            GameObject prefab = typeToPrefab[wallType];

            GameObject obj = Instantiate(prefab, new Vector2(x, y), Quaternion.identity, wallsParent);
            gridObjects[x, y] = obj;
        }
    }

    // 清除地图
    public void ClearMap()
    {
        if (wallsParent == null) return;

        // 清除所有子对象
        while (wallsParent.childCount > 0)
        {
            DestroyImmediate(wallsParent.GetChild(0).gameObject);
        }

        // 重置地图数据
        if (map != null)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    map[x, y] = MapType.floor;

                    gridObjects[x, y] = null;
                }
            }
        }

        homePrefab.SetActive(false);
    }


    // 加载关卡
    public void LoadLevel(Level level)
    {
        if (level == null) return;

        // 设置地图尺寸
        mapWidth = level.width;
        mapHeight = level.height;
        //复原buff
        ironWallEndFrames = 0;
        isChange = false;


        // 解析地图数据
        LoadMapFromString(level.mapData);
        homePrefab.SetActive(true);
        homePrefab.transform.position = new Vector2((mapWidth - 1f) / 2, 1.5f);
    }

    // 从字符串加载地图
    void LoadMapFromString(string mapData)
    {
        // 重新初始化数组
        map = new MapType[mapWidth, mapHeight];
        gridObjects = new GameObject[mapWidth, mapHeight];
        string[] rows = mapData.Split('\n');
        // 解析瓦片数据
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if ((x == (mapWidth - 1) / 2 || x == mapWidth / 2) && (y == 1 || y == 2))
                {
                    map[x, y] = MapType.home;

                    continue;
                }

                char tileChar = rows[y][x];
                if (int.TryParse(tileChar.ToString(), out int tileType))
                {
                    if (tileType >= 0 && tileType <= 6)
                    {
                        SetWallType(x, y, (MapType)tileType);
                    }
                }
            }
        }
    }

    // 检查区域是否可通行,检测地形和坦克碰撞
    public bool IsAreaWalkable(int startX, int startY, int width, int height, string selfID,List<MapType> allowTypes)
    {
        // 检查区域内的每个格子
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                // 检查边界
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    return false;

                // 检查地形
                MapType wallType = GetWallType(x, y);
                if (!allowTypes.Contains(wallType))
                    return false;
                
                // 新检查坦克碰撞
                if(EntityManager.Instance.tankPositionCache.TryGetValue(new Vector2Int(x,y),out var tankOn))
                {
                    if (tankOn != null&&tankOn.tankID!=selfID)
                    {
                        return false;
                    }
                }
                // 检查坦克碰撞
             /*   foreach (var tank in EntityManager.Instance.allTanks)
                {
                    if (tank.tankID == selfID) continue;
                    if (IsRectOverlap(startX, startY, width, height,
                            tank.Pos.x, tank.Pos.y, 2, 2))
                    {
                        return false;
                    }
                }*/


            }
        }

        return true;
    }




    // 检查2x2区域子弹是否可通过
    public bool IsAreaBulletPassable(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    return false;

                MapType wallType = GetWallType(x, y);
                if (wallType != MapType.floor && wallType != MapType.river
                                              && wallType != MapType.tree && wallType != MapType.ice)
                    return false;
            }
        }

        return true;
    }

    //从全局地图中获取制定类型地图的网格
    public List<Vector2Int> GetMapTypePos(List<MapType> mapTypes)
    {
        HashSet<MapType> mapHash = new HashSet<MapType>(mapTypes);
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapHash.Contains(map[x, y]))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        return candidates;
    }

    //从全局地图中获取制定类型地图2*2的网格
    public List<Vector2Int> GetTwoMapTypePos(List<MapType> mapTypes)
    {
        HashSet<MapType> mapHash = new HashSet<MapType>(mapTypes);
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < mapWidth - 1; x++)
        {
            for (int y = 0; y < mapHeight - 1; y++)
            {
                if (mapHash.Contains(map[x, y]) && mapHash.Contains(map[x + 1, y]) &&
                    mapHash.Contains(map[x, y + 1]) && mapHash.Contains(map[x + 1, y + 1]))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        return candidates;
    }

    public List<Vector2Int> GetAllTankPos()
    {
        List<Vector2Int> res = new List<Vector2Int>();

        foreach (var tank in EntityManager.Instance.allTanks)
        {
            res.Add(tank.Pos);
            res.Add(tank.Pos + new Vector2Int(1, 0));
            res.Add(tank.Pos + new Vector2Int(1, 1));
            res.Add(tank.Pos + new Vector2Int(0, 1));
        }

 

        return res;
    }
    

    public bool IsHintHome(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    return false;

                MapType wallType = GetWallType(x, y);
                if (wallType == MapType.home)
                    return true;
            }
        }

        return false;
    }

    // 获取区域内的坦克
    public List<TankController> GetTankInArea(int startX, int startY, int width, int height)
    {
        List<TankController> tanks = new List<TankController>();
        HashSet<TankController> tankSet = new HashSet<TankController>();
        
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;
                // 使用缓存检查坦克碰撞
                if(EntityManager.Instance.tankPositionCache.TryGetValue(new Vector2Int(x,y),out var tankOn))
                {
                    if (tankOn != null && tankSet.Add(tankOn)) // HashSet.Add返回true表示成功添加（之前不存在）
                    {
                        tanks.Add(tankOn);
                    }
                }
            }
        }

        return tanks;
    }

    public List<PlayerTankController> GetPlayerTankInArea(int startX, int startY, int width, int height)
    {
        List<PlayerTankController> tanks = new List<PlayerTankController>();
        HashSet<PlayerTankController> tankSet = new HashSet<PlayerTankController>();
        
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;
                // 使用缓存检查坦克碰撞
                if(EntityManager.Instance.tankPositionCache.TryGetValue(new Vector2Int(x,y),out var tankOn))
                {
                    if (tankOn != null && tankOn is PlayerTankController playerTank && tankSet.Add(playerTank))
                    {
                        tanks.Add(playerTank);
                    }
                }
            }
        }

        return tanks;
    }

    public List<EnemyTankController> GetEnemyTankInArea(int startX, int startY, int width, int height)
    {
        List<EnemyTankController> tanks = new List<EnemyTankController>();
        HashSet<EnemyTankController> tankSet = new HashSet<EnemyTankController>();
        
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;
                // 使用缓存检查坦克碰撞
                if (EntityManager.Instance.tankPositionCache.TryGetValue(new Vector2Int(x, y), out var tankOn))
                {
                    if (tankOn != null && tankOn is EnemyTankController enemyTank && tankSet.Add(enemyTank))
                    {
                        tanks.Add(enemyTank);
                    }
                }
            }
        }

        return tanks;
    }


    // 检查两个矩形是否重叠
    private bool IsRectOverlap(int x1, int y1, int w1, int h1,
        int x2, int y2, int w2, int h2)
    {
        return !(x1 + w1 <= x2 || x2 + w2 <= x1 ||
                 y1 + h1 <= y2 || y2 + h2 <= y1);
    }


    public bool IsVailePos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < mapWidth && pos.y < mapHeight;
    }
}