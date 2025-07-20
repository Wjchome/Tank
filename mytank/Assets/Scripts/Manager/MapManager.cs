
    using UnityEngine;
    using System.Collections.Generic;

    public enum MapType
    {
        error=-1,
        floor,
        wall,
        breakableWall,
        river,
        tree,
    }
   
    public class MapManager : SingletonMono<MapManager>
    {
        [Header("Map Settings")]
        public int mapWidth = 20;
        public int mapHeight = 20;
        public float gridSize = 1f;
        private MapType[,] map;
        
        [Header("Level Data")]
        public List<Level> availableLevels = new List<Level>();
        public int currentLevel = 0;
        
        [Header("Prefabs")]
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject breakableWallPrefab;
        public GameObject riverPrefab;    // 新增：河流预制体
        public GameObject treePrefab;     // 新增：树荫预制体
        public Transform wallsParent;
        
        
        public Dictionary<MapType,GameObject> typeToPrefab;
        
        private GameObject[,] gridObjects; // 记录每个格子的实例
        
        private void Awake()
        {
            typeToPrefab = new Dictionary<MapType, GameObject>()
            {
                { MapType.floor, floorPrefab },
                { MapType.wall, wallPrefab },
                { MapType.breakableWall, breakableWallPrefab },
                { MapType.river, riverPrefab },      // 河流
                { MapType.tree, treePrefab },       // 树荫
            };
            
            
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
        public void SetWallType(int x, int y,MapType wallType)
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
                        if (gridObjects[x, y] != null)
                        {
                            gridObjects[x, y] = null;
                        }
                    }
                }
            }
            
            Debug.Log("Map cleared successfully!");
        }

        public TankController GetTankController(int x, int y)
        {
            foreach (var kv in GameStateManager.Instance.playerTanks)
            {
                var tank = kv.Value;
                

                if (tank.Pos.x == x && tank.Pos.y == y)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        // 检查子弹是否可以通过（子弹不能过墙，但可以过河流和树荫）
        public bool IsBulletPassable(int x, int y)
        {
            MapType wallType = GetWallType(x, y);
            return wallType == MapType.floor || wallType == MapType.river || wallType == MapType.tree; // 空地、河流、树荫可通过
        }
        
        // 检查位置是否可通行（保持原有方法兼容性）
        public bool IsWalkable(int x, int y)
        {
            foreach (var kv in GameStateManager.Instance.playerTanks)
            {
                var tank = kv.Value;
                

                if (tank.Pos.x == x && tank.Pos.y == y)
                {
                    return false;
                }
            }

            MapType wallType = GetWallType(x, y);
            
            return wallType == MapType.floor|| wallType == MapType.tree;
        }

        // 加载关卡
        public void LoadLevel(Level level)
        {
            if (level == null) return;
            
            // 设置地图尺寸
            mapWidth = level.width;
            mapHeight = level.height;
            
            // 清除现有地图
            ClearMap();
            
            // 解析地图数据
            LoadMapFromString(level.mapData);
            
            Debug.Log($"Level {level.levelName} loaded successfully!");
        }
        
        // 从字符串加载地图
        public void LoadMapFromString(string mapData)
        {
            if (string.IsNullOrEmpty(mapData))
            {
                Debug.LogError("Map data is empty!");
                return;
            }
            
            // 解析地图数据
            string[] rows = mapData.Split('\n');
            if (rows.Length == 0)
            {
                Debug.LogError("Invalid map data format!");
                return;
            }
            
            int height = rows.Length;
            int width = rows[0].Length;
            
            // 检查数据一致性
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].Length != width)
                {
                    Debug.LogError($"Row {i+1} has inconsistent length!");
                    return;
                }
            }
            
            // 更新地图尺寸
            mapWidth = width;
            mapHeight = height;
            
            // 重新初始化数组
            map = new MapType[mapWidth, mapHeight];
            gridObjects = new GameObject[mapWidth, mapHeight];
            
            // 解析瓦片数据
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char tileChar = rows[y][x];
                    if (int.TryParse(tileChar.ToString(), out int tileType))
                    {
                        if (tileType >= 0 && tileType <= 4)
                        {
                            SetWallType(x, y, (MapType)tileType);
                        }
                    }
                }
            }
        }
        
        // 获取当前关卡
        public Level GetCurrentLevel()
        {
            if (currentLevel >= 0 && currentLevel < availableLevels.Count)
            {
                return availableLevels[currentLevel];
            }
            return null;
        }
        
        // 设置当前关卡
        public void SetCurrentLevel(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < availableLevels.Count)
            {
                currentLevel = levelIndex;
                LoadLevel(availableLevels[currentLevel]);
            }
        }
        
        // 获取出生点
        public Vector2Int GetSpawnPoint(int playerIndex)
        {
            // 默认出生点（四个角落）
            Vector2Int[] defaultSpawnPoints = new Vector2Int[]
            {
                new Vector2Int(1, 1),                    // 左下角
                new Vector2Int(mapWidth - 2, 1),         // 右下角
                new Vector2Int(1, mapHeight - 2),        // 左上角
                new Vector2Int(mapWidth - 2, mapHeight - 2) // 右上角
            };
            
            int index = playerIndex % defaultSpawnPoints.Length;
            return defaultSpawnPoints[index];
        }
    }
