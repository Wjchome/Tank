# 松散四叉树（Loose QuadTree）优化分析

## 当前实现：普通四叉树

### 特点
- **严格边界**：节点边界 = 子节点边界之和（精确分割）
- **严格包含**：物体必须与节点边界重叠才能存储
- **频繁切换**：物体在边界附近移动时，会在多个节点间频繁切换

### 当前代码关键点
```csharp
// Split() - 严格分割
LeftUp = new QuadTreeNode(new FixRect(x, y + halfHeight, halfWidth, halfHeight), ...);
// 子节点边界 = 父节点的 1/4，严格拼接

// AddToChildNodes() - 严格检查
if (LeftUp.Rect.Overlaps(boundsForTree)) LeftUp.Add(obj);
// 只有完全重叠才添加
```

## 松散四叉树（Loose QuadTree）

### 核心思想
- **松散边界**：每个节点有两个边界
  - **实际边界（Rect）**：用于查询和分裂
  - **松散边界（LooseRect）**：通常是实际边界的2倍大小，用于存储判断
- **减少切换**：物体只需要与松散边界重叠就可以存储，减少边界附近的频繁移动

### 优势
1. **减少更新频率**：物体在边界附近移动时，不会频繁切换节点
2. **提高性能**：减少 `UpdateObject` 的调用次数
3. **更适合动态物体**：特别适合移动频繁的游戏对象

### 劣势
1. **查询可能稍慢**：需要检查更多节点（但通常影响不大）
2. **内存稍多**：每个节点需要存储两个边界

## 需要修改的地方

### 1. QuadTreeNode 类

#### 需要添加的字段
```csharp
public class QuadTreeNode
{
    public FixRect Rect;           // 实际边界（用于查询和分裂）
    public FixRect LooseRect;      // 松散边界（用于存储判断，通常是2倍大小）
    
    // ... 其他字段保持不变
}
```

#### 需要修改的方法

**1.1 构造函数**
```csharp
// 当前：
public QuadTreeNode(FixRect rect, int currentDepth, int maxObjects, int maxDepth)
{
    Rect = rect;
    // ...
}

// 修改为：
public QuadTreeNode(FixRect rect, int currentDepth, int maxObjects, int maxDepth, Fix64 looseFactor = default)
{
    Rect = rect;
    // 计算松散边界（默认2倍，可配置）
    if (looseFactor == default) looseFactor = new Fix64(2);
    Fix64 looseWidth = rect.Width * looseFactor;
    Fix64 looseHeight = rect.Height * looseFactor;
    Fix64 looseX = rect.CenterX - looseWidth / new Fix64(2);
    Fix64 looseY = rect.CenterY - looseHeight / new Fix64(2);
    LooseRect = new FixRect(looseX, looseY, looseWidth, looseHeight);
    // ...
}
```

**1.2 Split() 方法**
```csharp
// 当前：子节点使用严格边界
LeftUp = new QuadTreeNode(new FixRect(x, y + halfHeight, halfWidth, halfHeight), ...);

// 修改为：子节点也使用松散边界
// 注意：子节点的实际边界仍然是严格分割，但松散边界会重叠
LeftUp = new QuadTreeNode(
    new FixRect(x, y + halfHeight, halfWidth, halfHeight), 
    _currentDepth + 1, 
    _maxObjects, 
    _maxDepth,
    looseFactor  // 传递松散因子
);
```

**1.3 AddToChildNodes() 方法**
```csharp
// 当前：使用严格边界检查
if (LeftUp.Rect.Overlaps(boundsForTree)) LeftUp.Add(obj);

// 修改为：使用松散边界检查
if (LeftUp.LooseRect.Overlaps(boundsForTree)) LeftUp.Add(obj);
```

**1.4 GetObjectsInArea() 方法**
```csharp
// 当前：使用实际边界检查
if (!Rect.Overlaps(area)) return;

// 修改为：可以使用松散边界或实际边界
// 选项1：使用实际边界（更精确，但可能漏查）
if (!Rect.Overlaps(area)) return;

// 选项2：使用松散边界（更安全，但可能多查）
// if (!LooseRect.Overlaps(area)) return;
```

### 2. QuadTreeV3 类

#### 需要添加的配置
```csharp
[Tooltip("松散因子（松散边界 = 实际边界 * 松散因子，默认2.0）")]
public float LooseFactor = 2.0f;
```

#### 需要修改的方法

**2.1 Init() 方法**
```csharp
// 需要将 LooseFactor 传递给根节点
_rootNode = new QuadTreeNode(
    RootRect, 
    0, 
    MaxObjectsPerNode, 
    MaxDepth,
    (Fix64)LooseFactor  // 传递松散因子
);
```

### 3. 关键区别总结

| 方面 | 普通四叉树 | 松散四叉树 |
|------|-----------|-----------|
| **节点边界** | 1个（Rect） | 2个（Rect + LooseRect） |
| **存储判断** | `Rect.Overlaps()` | `LooseRect.Overlaps()` |
| **查询判断** | `Rect.Overlaps()` | `Rect.Overlaps()`（可选LooseRect） |
| **边界切换** | 频繁（边界附近） | 减少（松散边界缓冲） |
| **适用场景** | 静态/慢速物体 | 动态/快速物体 |

### 4. 性能影响

**优势场景**：
- 物体频繁移动（如坦克、子弹）
- 物体在边界附近移动
- 需要频繁调用 `UpdateObject`

**劣势场景**：
- 大量静态物体（松散边界意义不大）
- 查询性能可能略降（但通常可忽略）

### 5. 实现建议

1. **可配置的松散因子**：允许在 Inspector 中调整（1.5-3.0 之间）
2. **保持向后兼容**：默认松散因子为 2.0
3. **测试验证**：对比优化前后的 `UpdateObject` 调用次数

## 示例对比

### 场景：物体在边界附近移动

**普通四叉树**：
```
物体位置 (1.0, 1.0) → 节点A
物体移动 (1.1, 1.0) → 节点B（边界切换）
物体移动 (1.0, 1.0) → 节点A（又切换回来）
```

**松散四叉树**（松散因子=2.0）：
```
物体位置 (1.0, 1.0) → 节点A（松散边界包含）
物体移动 (1.1, 1.0) → 节点A（仍在松散边界内，不切换）
物体移动 (1.0, 1.0) → 节点A（不切换）
```

## 注意事项

1. **确定性**：松散边界计算必须使用 Fix64，确保跨平台一致性
2. **查询精度**：查询时仍使用实际边界，保证结果准确
3. **内存开销**：每个节点多存储一个 FixRect（4个Fix64 = 32字节）


