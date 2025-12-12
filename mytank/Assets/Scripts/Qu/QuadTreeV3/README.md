# QuadTreeV3 - 帧同步四叉树碰撞检测系统

## 概述

QuadTreeV3 是一个专为帧同步设计的空间分区系统，支持高效的碰撞检测和查询。系统使用固定点数（Fix64）确保确定性，适合多人在线游戏。

## 主要特性

- ✅ **确定性**：使用 Fix64 固定点数，确保跨平台一致性
- ✅ **高效查询**：四叉树空间分区，O(log n) 查询复杂度
- ✅ **层系统**：支持最多 32 层，可过滤不必要的碰撞检测
- ✅ **多种形状**：支持矩形（含旋转）和圆形碰撞体
- ✅ **自动优化**：节点自动分裂和合并

## 快速开始

### 1. 初始化四叉树

在场景中创建 GameObject，添加 `QuadTreeV3` 组件：

```csharp
// 通过单例访问
QuadTreeV3.Instance.AddObject(bounds, gameObject, QuadTreeLayer.GetLayer(0));
```

### 2. 添加物体

```csharp
// 添加矩形物体（第0层）
FixRect bounds = new FixRect((Fix64)10, (Fix64)20, (Fix64)5, (Fix64)3);
QuadTreeV3.Instance.AddObject(bounds, gameObject, QuadTreeLayer.GetLayer(0));

// 添加圆形物体（第1层）
FixCircle circle = new FixCircle((Fix64)15, (Fix64)25, (Fix64)2);
QuadTreeV3.Instance.AddObject(circle, gameObject, QuadTreeLayer.GetLayer(1));
```

### 3. 查询物体

```csharp
// 查询矩形区域内的所有物体
FixRect queryArea = new FixRect((Fix64)0, (Fix64)0, (Fix64)100, (Fix64)100);
List<QuadTreeObject> results = QuadTreeV3.Instance.Query(queryArea);

// 只查询特定层的物体（例如只查询第0层和第2层）
QuadTreeLayer layerMask = QuadTreeLayer.GetLayer(0) | QuadTreeLayer.GetLayer(2);
List<QuadTreeObject> filteredResults = QuadTreeV3.Instance.Query(queryArea, layerMask);
```

### 4. 更新物体位置

```csharp
// 物体移动后更新位置
FixRect newBounds = new FixRect((Fix64)newX, (Fix64)newY, (Fix64)width, (Fix64)height);
QuadTreeV3.Instance.UpdateObject(gameObject, newBounds);
```

### 5. 移除物体

```csharp
QuadTreeV3.Instance.RemoveObject(gameObject);
```

## 层系统使用

### 创建层掩码

```csharp
// 单个层
QuadTreeLayer layer0 = QuadTreeLayer.GetLayer(0);
QuadTreeLayer layer1 = QuadTreeLayer.GetLayer(1);

// 多个层（使用位或运算）
QuadTreeLayer multipleLayers = QuadTreeLayer.GetLayer(0) | QuadTreeLayer.GetLayer(2) | QuadTreeLayer.GetLayer(5);

// 所有层
QuadTreeLayer allLayers = QuadTreeLayer.Everything;

// 无层
QuadTreeLayer noLayers = QuadTreeLayer.Nothing;
```

### 层过滤查询

```csharp
// 只查询玩家层（第0层）
QuadTreeLayer playerLayer = QuadTreeLayer.GetLayer(0);
List<QuadTreeObject> players = QuadTreeV3.Instance.Query(area, playerLayer);

// 查询玩家和敌人层（第0层和第1层）
QuadTreeLayer playerAndEnemy = QuadTreeLayer.GetLayer(0) | QuadTreeLayer.GetLayer(1);
List<QuadTreeObject> combatants = QuadTreeV3.Instance.Query(area, playerAndEnemy);

// 查询所有层（默认行为）
List<QuadTreeObject> all = QuadTreeV3.Instance.Query(area); // 等同于 Query(area, QuadTreeLayer.Everything)
```

### 层的最佳实践

1. **定义层常量**：在项目中定义层常量，避免硬编码
   ```csharp
   public static class GameLayers
   {
       public const int Player = 0;
       public const int Enemy = 1;
       public const int Bullet = 2;
       public const int Wall = 3;
       public const int Pickup = 4;
   }
   ```

2. **使用层过滤**：在碰撞检测时使用层过滤，避免不必要的计算
   ```csharp
   // 玩家只检测敌人和墙壁
   QuadTreeLayer playerCollisionMask = 
       QuadTreeLayer.GetLayer(GameLayers.Enemy) | 
       QuadTreeLayer.GetLayer(GameLayers.Wall);
   List<QuadTreeObject> collisions = QuadTreeV3.Instance.Query(playerBounds, playerCollisionMask);
   ```

3. **性能优化**：层过滤在查询时进行，可以显著减少不必要的碰撞检测

## API 参考

### QuadTreeV3

#### 属性
- `Instance` - 单例实例
- `RootRect` - 根节点矩形区域

#### 方法
- `AddObject(FixRect bounds, GameObject target, QuadTreeLayer layer)` - 添加矩形物体
- `AddObject(FixCircle circle, GameObject target, QuadTreeLayer layer)` - 添加圆形物体
- `RemoveObject(GameObject target)` - 移除物体
- `UpdateObject(GameObject target, FixRect newBounds)` - 更新物体位置
- `Query(FixRect area, QuadTreeLayer layerMask)` - 查询矩形区域
- `Query(FixCircle circle, QuadTreeLayer layerMask)` - 查询圆形区域

### QuadTreeLayer

#### 静态方法
- `GetLayer(int layer)` - 创建单个层掩码
- `Everything` - 所有层
- `Nothing` - 无层

#### 方法
- `Contains(int layer)` - 检查层是否在掩码中
- `Intersects(QuadTreeLayer other)` - 检查是否有交集
- `AddLayer(int layer)` - 添加层
- `RemoveLayer(int layer)` - 移除层

## 注意事项

1. **确定性**：所有计算使用 Fix64，确保跨平台一致性
2. **单例**：确保场景中只有一个 QuadTreeV3 实例
3. **更新频率**：物体移动后必须调用 `UpdateObject` 更新位置
4. **层索引**：层索引范围是 0-31（32 层）
5. **性能**：层过滤在查询时进行，不会影响存储结构


