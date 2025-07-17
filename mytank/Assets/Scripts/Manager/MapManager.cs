
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
            // 在运行时从wallsParent读取地图数据
            LoadMapFromWallsParent();
            typeToPrefab = new Dictionary<MapType, GameObject>()
            {
                { MapType.floor, floorPrefab },
                { MapType.wall, wallPrefab },
                { MapType.breakableWall, breakableWallPrefab },
                { MapType.river, riverPrefab },      // 河流
                { MapType.tree, treePrefab },       // 树荫
            };
            gridObjects = new GameObject[mapWidth, mapHeight];
        }
        
        void LoadMapFromWallsParent()
        {
            if (wallsParent == null)
            {
                Debug.LogWarning("WallsParent is null! Cannot load map.");
                return;
            }
            
            map = new MapType[mapWidth, mapHeight];
            if (gridObjects == null)
                gridObjects = new GameObject[mapWidth, mapHeight];
            // 清理原有实例
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (gridObjects[x, y] != null)
                    {
                        Destroy(gridObjects[x, y]);
                        gridObjects[x, y] = null;
                    }
                }
            }
            
            // 初始化地图为空
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    map[x, y] = 0;
                }
            }
            
            // 从wallsParent读取所有子对象
            foreach (Transform child in wallsParent)
            {
                Vector2 position = child.position;
                int x = Mathf.RoundToInt(position.x);
                int y = Mathf.RoundToInt(position.y);
                
                // 检查位置是否在地图范围内
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                {
                    if (child.CompareTag("Wall"))
                    {
                        map[x, y] = MapType.wall; // 不可破坏墙
                        gridObjects[x, y] = child.gameObject;
                    }
                    else if (child.CompareTag("BreakableWall"))
                    {
                        map[x, y] = MapType.breakableWall; // 可破坏墙
                        gridObjects[x, y] = child.gameObject;
                    }
                    else if (child.CompareTag("Floor"))
                    {
                        map[x, y] = MapType.floor;//空地
                        gridObjects[x, y] = child.gameObject;
                    }
                    else if (child.CompareTag("River"))
                    {
                        map[x, y] = MapType.river; // 河流
                        gridObjects[x, y] = child.gameObject;
                    }
                    else if (child.CompareTag("Tree"))
                    {
                        map[x, y] = MapType.tree; // 树荫
                        gridObjects[x, y] = child.gameObject;
                    }
                }
            }
            
            Debug.Log("Map loaded from WallsParent successfully!");
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
                if (tank.isLocalPlayer)
                {
                    continue;
                }

                if (tank.Pos.x == x && tank.Pos.y == y)
                {
                    return false;
                }
            }

            MapType wallType = GetWallType(x, y);
            
            return wallType == MapType.floor|| wallType == MapType.tree;
        }

        
       
        
       
    }
