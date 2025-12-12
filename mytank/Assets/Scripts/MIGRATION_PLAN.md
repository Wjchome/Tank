# 定点数和四叉树迁移计划

## 项目概述
将现有的格子游戏系统逐步迁移到确定性定点数（Fix64）和四叉树碰撞检测系统，以支持更精确的帧同步和连续移动。

## 当前系统
- **位置系统**：使用 `Vector2Int` 格子坐标，每个坦克占据 2x2 格子
- **碰撞检测**：基于格子的 `MapManager.IsAreaWalkable` 等方法
- **移动系统**：离散格子移动，每帧移动一个格子
- **数值计算**：使用 `float` 进行位置计算和动画

## 目标系统
- **位置系统**：使用 `FixVector2` 连续坐标
- **碰撞检测**：使用 `QuadTreeV3` 四叉树系统
- **移动系统**：连续移动，支持任意方向和速度
- **数值计算**：全部使用 `Fix64` 定点数，确保跨平台确定性

## 迁移阶段

### 阶段1：基础准备 ✅
- [x] 确认 Fix64 和 FixVector2 已实现
- [x] 确认 QuadTreeV3 已实现
- [ ] 创建位置转换工具类（Fix64 <-> Unity Vector2）
- [ ] 在实体类中添加 Fix64 位置字段（双轨制）
- [ ] 创建游戏层常量定义

### 阶段2：定点数迁移
- [ ] 将位置系统从 Vector2Int 迁移到 FixVector2
- [ ] 将移动速度、距离等计算改为 Fix64
- [ ] 保持格子系统作为显示层（用于地图渲染）
- [ ] 测试定点数计算的正确性

### 阶段3：四叉树集成
- [ ] 初始化 QuadTreeV3 到游戏场景
- [ ] 将坦克、子弹等实体添加到四叉树
- [ ] 实现双轨制碰撞检测（格子+四叉树）
- [ ] 逐步将碰撞检测从格子改为四叉树查询

### 阶段4：连续移动
- [ ] 从格子移动改为连续移动
- [ ] 使用 Fix64 进行精确的位置计算
- [ ] 实现基于速度的移动系统
- [ ] 保持帧同步的确定性

### 阶段5：完全迁移
- [ ] 移除格子系统（保留作为显示层）
- [ ] 完全使用定点数和四叉树
- [ ] 优化性能
- [ ] 清理冗余代码

## 技术细节

### 层系统定义
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

### 位置转换
- 格子坐标 -> FixVector2: `new FixVector2(gridX, gridY)`
- FixVector2 -> Unity Vector2: `(Vector2)fixPos`
- Unity Vector2 -> FixVector2: `(FixVector2)unityPos`

### 碰撞检测迁移
- 旧：`MapManager.IsAreaWalkable(x, y, w, h)`
- 新：`QuadTreeV3.Instance.Query(bounds, layerMask)`

## 注意事项
1. **确定性**：所有计算必须使用 Fix64，避免浮点数
2. **双轨制**：迁移过程中保持新旧系统并行运行
3. **测试**：每个阶段完成后进行充分测试
4. **性能**：注意四叉树的更新频率，避免每帧重建

