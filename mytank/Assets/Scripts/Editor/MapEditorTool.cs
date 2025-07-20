using UnityEngine;
using UnityEditor;

public class MapEditorTool : EditorWindow
{
    private MapManager mapManager;
    private int selectedTool = 0; // 0清除, 1=不可破坏墙, 2=可破坏墙, 3=河流, 4=树荫
    private string[] toolNames = { "清除", "不可破坏墙", "可破坏墙", "河流", "树荫" };
    private bool isEditing = false;
    
    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorTool>("地图编辑器");
    }
    
    private void OnEnable()
    {
        // 查找场景中的MapManager
        mapManager = Object.FindFirstObjectByType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogWarning("场景中没有找到MapManager！");
        }
        
        // 注册Scene视图事件
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable()
    {
        // 取消注册Scene视图事件
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void OnGUI()
    {
        if (mapManager == null)
        {
            EditorGUILayout.HelpBox("请先在场景中添加MapManager组件！", MessageType.Warning);
            if (GUILayout.Button("查找MapManager"))
            {
                mapManager = Object.FindFirstObjectByType<MapManager>();
            }
            return;
        }
        
        EditorGUILayout.BeginVertical();
        
        // 编辑模式开关
        EditorGUILayout.LabelField("编辑模式", EditorStyles.boldLabel);
        isEditing = EditorGUILayout.Toggle("启用地图编辑", isEditing);
        
        if (isEditing)
        {
            EditorGUILayout.HelpBox("编辑模式已启用！在Scene视图中点击并拖动网格进行编辑。", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // 工具选择
        EditorGUILayout.LabelField("编辑工具", EditorStyles.boldLabel);
        selectedTool = GUILayout.Toolbar(selectedTool, toolNames);
        
        EditorGUILayout.Space();
        
        // 地图设置
        EditorGUILayout.LabelField("地图设置", EditorStyles.boldLabel);
        mapManager.mapWidth = EditorGUILayout.IntField("地图宽度", mapManager.mapWidth);
        mapManager.mapHeight = EditorGUILayout.IntField("地图高度", mapManager.mapHeight);
        
        EditorGUILayout.Space();
        
        // 预制体设置
        EditorGUILayout.LabelField("预制体设置", EditorStyles.boldLabel);
        mapManager.wallPrefab = (GameObject)EditorGUILayout.ObjectField("墙预制体", mapManager.wallPrefab, typeof(GameObject), false);
        mapManager.breakableWallPrefab = (GameObject)EditorGUILayout.ObjectField("可破坏墙预制体", mapManager.breakableWallPrefab, typeof(GameObject), false);
        mapManager.floorPrefab = (GameObject)EditorGUILayout.ObjectField("地面", mapManager.floorPrefab, typeof(GameObject), false);
        mapManager.riverPrefab = (GameObject)EditorGUILayout.ObjectField("河流", mapManager.riverPrefab, typeof(GameObject), false);
        mapManager.treePrefab = (GameObject)EditorGUILayout.ObjectField("树荫", mapManager.treePrefab, typeof(GameObject), false);
        mapManager.wallsParent = (Transform)EditorGUILayout.ObjectField("墙父对象", mapManager.wallsParent, typeof(Transform), true);
        
        EditorGUILayout.Space();
        
        // 操作按钮
        EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("生成地图边界"))
        {
            CreateSimpleMap();
        }
        
        if (GUILayout.Button("清除地图"))
        {
            ClearMap();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("导出地图"))
        {
            ExportMap();
        }
        
        if (GUILayout.Button("导入地图"))
        {
            ImportMap();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("导出为Level"))
        {
            ExportAsLevel();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 使用说明
        EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. 启用编辑模式\n" +
            "2. 选择编辑工具\n" +
            "3. 在Scene视图中点击网格进行编辑\n" +
            "4. 使用导出地图将当前地图复制到剪贴板\n" +
            "5. 使用导入地图从剪贴板加载地图数据\n" +
            "6. 使用导出为Level创建ScriptableObject资源\n\n" +
            "数据格式说明:\n" +
            "0=空地, 1=墙, 2=可破坏墙, 3=河流, 4=树荫\n\n" +
            "示例:\n" +
            "1111111111\n" +
            "1000000001\n" +
            "1000000001\n" +
            "1111111111",
            MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isEditing || mapManager == null) return;
        
        // 绘制网格
        DrawGrid();
        
        // 处理鼠标事件
        HandleMouseInput();
        
        // 强制重绘Scene视图
        sceneView.Repaint();
    }
    readonly Color lineColor = new Color(1f, 0.2f, 0.8f, 0.5f); 
    
    
    private void DrawGrid()
    {
        if (mapManager == null) return;
    
        
        
        // 绘制网格线 - 修正坐标偏移，使网格线正确对齐
        for (int x = 0; x <= mapManager.mapWidth; x++)
        {
            Vector2 startPos = new Vector2(x-mapManager.gridSize/2 , -mapManager.gridSize/2);
            Vector2 endPos = new Vector2(x -mapManager.gridSize/2, mapManager.mapHeight -mapManager.gridSize/2);
            Handles.color = lineColor;
            Handles.DrawLine(startPos, endPos,0.2f);
        }
        
        for (int y = 0; y <= mapManager.mapHeight; y++)
        {
            Vector2 startPos = new Vector2(-mapManager.gridSize/2, y-mapManager.gridSize/2 );
            Vector2 endPos = new Vector2(mapManager.mapWidth-mapManager.gridSize/2 , y -mapManager.gridSize/2);
            Handles.color = lineColor;
            Handles.DrawLine(startPos, endPos,0.2f);
        }
        
       
    }
    
    private bool isDragging = false;
    private Vector2Int lastDragPosition;
    
    private void HandleMouseInput()
    {
        Event e = Event.current;
        
        // 获取鼠标在Scene视图中的世界坐标
        Vector2 mousePosition = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        
        // 对于2D项目，我们假设Z=0
        Vector3 worldPosition = ray.origin;
        worldPosition.z = 0;
        
        // 转换为网格坐标
        int gridX = Mathf.RoundToInt(worldPosition.x / mapManager.gridSize);
        int gridY = Mathf.RoundToInt(worldPosition.y / mapManager.gridSize);
        Vector2Int currentGridPosition = new Vector2Int(gridX, gridY);
        
        // 检查是否在地图范围内
        bool isInMapBounds = gridX >= 0 && gridX < mapManager.mapWidth && gridY >= 0 && gridY < mapManager.mapHeight;
        
        // 鼠标按下
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (isInMapBounds)
            {
                isDragging = true;
                lastDragPosition = currentGridPosition;
                PlaceWall(gridX, gridY,selectedTool);
                e.Use();
            }
        }
        // 鼠标拖动
        else if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
        {
            if (isInMapBounds && currentGridPosition != lastDragPosition)
            {
                lastDragPosition = currentGridPosition;
                PlaceWall(gridX, gridY,selectedTool);
                e.Use();
            }
        }
        // 鼠标释放
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            isDragging = false;
            e.Use();
        }
        
        // 显示当前鼠标位置的调试信息
        if (isInMapBounds )
        {
            Handles.color = Color.red;
            Vector3 debugPos = new Vector3(gridX, gridY, 0);
            Handles.DrawWireCube(debugPos, Vector3.one * 0.8f);
            
            // 显示坐标信息
            Handles.Label(debugPos + Vector3.up * 1.2f, $"({gridX}, {gridY})");
        }
        
        // 显示拖动状态
        if (isDragging && isInMapBounds)
        {
            Handles.color = Color.green;
            Vector3 dragPos = new Vector3(gridX, gridY, 0);
            Handles.DrawWireCube(dragPos, Vector3.one * 0.9f);
        }
    }
    
 
    
    private void PlaceWall(int x, int y,int type)
    {
        if (mapManager.wallsParent == null)
        {
            Debug.LogError("WallsParent is null!");
            return;
        }
        
        Vector2Int placePosition = new Vector2Int(x, y);
        
        // 先清除该位置的现有墙
        ClearWallAtPosition(placePosition);
        
        // 根据选择的工具放置墙
        GameObject prefab = null;
        string tag = "";
        
        switch (type)
        {
            case 1: // 不可破坏墙
                prefab = mapManager.wallPrefab;
                tag = "Wall";
                break;
            case 2: // 可破坏墙
                prefab = mapManager.breakableWallPrefab;
                tag = "BreakableWall";
                break;
            case 3: // 河流
                prefab = mapManager.riverPrefab;
                tag = "River";
                break;
            case 4: // 树荫
                prefab = mapManager.treePrefab;
                tag = "Tree";
                break;
            case 0: // 清除
                prefab = mapManager.floorPrefab;
                tag = "Floor";
                break;
            default:
                return; // 已经清除了，直接返回
        }
        
        if (prefab != null)
        {
            GameObject wall = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            wall.transform.position = new Vector3(placePosition.x * mapManager.gridSize, placePosition.y * mapManager.gridSize, 0);
            wall.transform.SetParent(mapManager.wallsParent);
            wall.tag = tag;
            // 标记场景为已修改
            EditorUtility.SetDirty(wall);
            EditorUtility.SetDirty(mapManager.wallsParent);
            
            // 记录撤销操作
            Undo.RegisterCreatedObjectUndo(wall, "Place Wall");
        }
    }
    
    private void ClearWallAtPosition(Vector2Int placePosition)
    {
        if (mapManager.wallsParent == null) return;
        
        for (int i = mapManager.wallsParent.childCount - 1; i >= 0; i--)
        {
            Transform child = mapManager.wallsParent.GetChild(i);
            Vector2 childPos = new Vector2(child.position.x, child.position.y);
            Vector2 targetPos = new Vector2(placePosition.x * mapManager.gridSize, placePosition.y * mapManager.gridSize);
            if (Vector2.Distance(childPos, targetPos) < mapManager.gridSize * 0.1f)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }
    
 
    
  
    private void ClearMap()
    {
        if (mapManager.wallsParent == null) return;
        
        while (mapManager.wallsParent.childCount > 0)
        {
            DestroyImmediate(mapManager.wallsParent.GetChild(0).gameObject);
        }
        
        Debug.Log("地图已清除！");
    }

    private void CreateSimpleMap()
    {
        ClearMap();
        for (int x = 0; x < mapManager.mapWidth; x++)
        {
            for (int y = 0; y < mapManager.mapHeight; y++)
            {
                if (x == 0 || y == 0 || x == mapManager.mapWidth - 1 || y == mapManager.mapHeight - 1)
                {
                    PlaceWall(x,y,1);
                    
                }
                else
                {
                    PlaceWall(x,y,0);
                }
            }
        }
    }
    
    // 导出地图数据为字符串
    private void ExportMap()
    {
        if (mapManager == null || mapManager.wallsParent == null) return;
        
        string mapData = GetMapDataString();
        
        // 复制到剪贴板
        EditorGUIUtility.systemCopyBuffer = mapData;
        
        // 显示导出结果
        Debug.Log($"地图已导出到剪贴板！\n地图尺寸: {mapManager.mapWidth}x{mapManager.mapHeight}\n数据:\n{mapData}");
        
        // 显示对话框
        EditorUtility.DisplayDialog("地图导出成功", 
            $"地图数据已复制到剪贴板！\n\n地图尺寸: {mapManager.mapWidth}x{mapManager.mapHeight}\n\n数据格式说明:\n0=空地, 1=墙, 2=可破坏墙, 3=河流, 4=树荫", 
            "确定");
    }
    
    // 导入地图数据
    private void ImportMap()
    {
        if (mapManager == null) return;
        
        // 从剪贴板读取数据
        string mapData = EditorGUIUtility.systemCopyBuffer;
        
        if (string.IsNullOrEmpty(mapData))
        {
            EditorUtility.DisplayDialog("导入失败", "剪贴板中没有地图数据！", "确定");
            return;
        }
        
        // 解析数据
        string[] rows = mapData.Split('\n');
        if (rows.Length == 0)
        {
            EditorUtility.DisplayDialog("导入失败", "地图数据格式错误！", "确定");
            return;
        }
        
        int height = rows.Length;
        int width = rows[0].Length;
        
        // 检查数据一致性
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Length != width)
            {
                EditorUtility.DisplayDialog("导入失败", $"第{i+1}行长度不一致！", "确定");
                return;
            }
        }
        
        // 更新地图尺寸
        mapManager.mapWidth = width;
        mapManager.mapHeight = height;
        
        // 清除现有地图
        ClearMap();
        
        // 导入数据
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char tileChar = rows[y][x];
                if (int.TryParse(tileChar.ToString(), out int tileType))
                {
                    if (tileType >= 0 && tileType <= 4)
                    {
                        PlaceWall(x, y, tileType);
                    }
                }
            }
        }
        
        Debug.Log($"地图导入成功！\n地图尺寸: {width}x{height}");
        EditorUtility.DisplayDialog("地图导入成功", 
            $"地图数据导入成功！\n\n地图尺寸: {width}x{height}\n\n数据格式说明:\n0=空地, 1=墙, 2=可破坏墙, 3=河流, 4=树荫", 
            "确定");
    }
    
    // 导出为Level ScriptableObject
    private void ExportAsLevel()
    {
        if (mapManager == null || mapManager.wallsParent == null) return;
        
        // 获取地图数据字符串
        string mapData = GetMapDataString();
        
        // 创建Level资源
        Level level = ScriptableObject.CreateInstance<Level>();
        level.levelName = $"Level_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        level.width = mapManager.mapWidth;
        level.height = mapManager.mapHeight;
        level.mapData = mapData;
        
        // 保存资源
        string path = EditorUtility.SaveFilePanelInProject(
            "保存Level资源", 
            level.levelName, 
            "asset", 
            "选择保存位置"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Level资源已保存到: {path}");
          
        }
    }
    
    // 获取地图数据字符串
    private string GetMapDataString()
    {
        // 创建临时地图数组（一维数组）
        MapType[] tempMap = new MapType[mapManager.mapWidth * mapManager.mapHeight];
        
        // 初始化为空地
        for (int i = 0; i < tempMap.Length; i++)
        {
            tempMap[i] = MapType.floor;
        }
        
        // 从wallsParent读取所有子对象
        foreach (Transform child in mapManager.wallsParent)
        {
            Vector2 position = child.position;
            int x = Mathf.RoundToInt(position.x / mapManager.gridSize);
            int y = Mathf.RoundToInt(position.y / mapManager.gridSize);
            
            // 检查位置是否在地图范围内
            if (x >= 0 && x < mapManager.mapWidth && y >= 0 && y < mapManager.mapHeight)
            {
                int index = y * mapManager.mapWidth + x;
                if (child.CompareTag("Wall"))
                {
                    tempMap[index] = MapType.wall;
                }
                else if (child.CompareTag("BreakableWall"))
                {
                    tempMap[index] = MapType.breakableWall;
                }
                else if (child.CompareTag("Floor"))
                {
                    tempMap[index] = MapType.floor;
                }
                else if (child.CompareTag("River"))
                {
                    tempMap[index] = MapType.river;
                }
                else if (child.CompareTag("Tree"))
                {
                    tempMap[index] = MapType.tree;
                }
            }
        }
        
        // 生成导出字符串
        string mapData = "";
        for (int y = 0; y < mapManager.mapHeight; y++)
        {
            string row = "";
            for (int x = 0; x < mapManager.mapWidth; x++)
            {
                int index = y * mapManager.mapWidth + x;
                row += ((int)tempMap[index]).ToString();
            }
            mapData += row;
            if (y < mapManager.mapHeight - 1)
            {
                mapData += "\n";
            }
        }
        
        return mapData;
    }
} 