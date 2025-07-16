
    using UnityEngine;
    using System.Collections.Generic;

    public class MapManager : SingletonMono<MapManager>
    {
        [Header("Map Settings")]
        public int mapWidth = 20;
        public int mapHeight = 20;
        public float gridSize = 1f;
        private int[,] map; // 0=空格, 1=不可破坏墙, 2=可破坏墙
        
        [Header("Prefabs")]
        public GameObject wallPrefab;
        public GameObject breakableWallPrefab;
        public GameObject floorPrefab;
        public Transform wallsParent;
        
        
       
        
        private void Awake()
        {
            // 在运行时从wallsParent读取地图数据
            LoadMapFromWallsParent();
        }
        
        void LoadMapFromWallsParent()
        {
            if (wallsParent == null)
            {
                Debug.LogWarning("WallsParent is null! Cannot load map.");
                return;
            }
            
            map = new int[mapWidth, mapHeight];
            
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
                        map[x, y] = 1; // 不可破坏墙
                    }
                    else if (child.CompareTag("BreakableWall"))
                    {
                        map[x, y] = 2; // 可破坏墙
                    }
                    else if (child.CompareTag("Floor"))
                    {
                        map[x, y] = 0;//空地
                    }
                }
            }
            
            Debug.Log("Map loaded from WallsParent successfully!");
        }
        
        // 获取指定位置的墙类型
        public int GetWallType(int x, int y)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                return map[x, y];
            }
            return -1; // 超出边界
        }
        
        // 设置指定位置的墙类型
        public void SetWallType(int x, int y, int wallType)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                map[x, y] = wallType;
            }
        }
        
        // 检查位置是否可通行
        public bool IsWalkable(int x, int y)
        {
            foreach (var kv in GameManager.Instance.tanks)
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

          
            
            return GetWallType(x, y) == 0;
        }
        
       
        
       
    }
