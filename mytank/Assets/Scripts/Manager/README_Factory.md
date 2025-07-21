# 工厂系统使用说明

## 概述

这个工厂系统使用对象池模式来管理坦克和子弹的创建和复用，提高性能并减少内存分配。

## 系统架构

```
ObjectPoolManager (对象池管理器)
├── TankFactory (坦克工厂)
├── BulletFactory (子弹工厂)
└── FactoryManager (统一工厂管理器)
```

## 主要组件

### 1. ObjectPoolManager
- 核心对象池管理器
- 管理所有对象池的创建、回收和复用
- 支持延迟回收功能

### 2. TankFactory
- 坦克创建和回收
- 支持玩家坦克和AI坦克
- 自动清理坦克状态

### 3. BulletFactory
- 子弹创建和回收
- 管理活跃子弹列表
- 支持批量操作

### 4. FactoryManager
- 统一管理所有工厂
- 提供游戏初始化和清理功能
- 统计和调试功能

## 使用方法

### 1. 设置预制体
在Unity编辑器中：
1. 创建空物体，添加 `ObjectPoolManager` 组件
2. 创建空物体，添加 `TankFactory` 组件
3. 创建空物体，添加 `BulletFactory` 组件
4. 创建空物体，添加 `FactoryManager` 组件
5. 在 `FactoryManager` 中设置引用关系

### 2. 配置对象池
在 `ObjectPoolManager` 中配置：
```csharp
poolInfos = new PoolInfo[]
{
    new PoolInfo { tag = "Tank", prefab = tankPrefab, size = 20 },
    new PoolInfo { tag = "Bullet", prefab = bulletPrefab, size = 50 }
};
```

### 3. 创建坦克
```csharp
// 创建玩家坦克
TankController playerTank = TankFactory.Instance.CreatePlayerTank(
    "player1", "Player1", 1, 1, Color.blue, 0
);

// 创建AI坦克
TankController aiTank = TankFactory.Instance.CreateAITank(
    "ai1", 5, 5, 0
);
```

### 4. 创建子弹
```csharp
BulletController bullet = BulletFactory.Instance.CreateBullet(
    "bullet1", Direction.Up, "player1", true
);
```

### 5. 回收对象
```csharp
// 立即回收
TankFactory.Instance.RecycleTank(tank);
BulletFactory.Instance.RecycleBullet(bullet);

// 延迟回收（用于动画）
TankFactory.Instance.RecycleTankDelayed(tank, 2.0f);
BulletFactory.Instance.RecycleBulletDelayed(bullet, 0.5f);
```

### 6. 游戏管理
```csharp
// 游戏开始
FactoryManager.Instance.InitializeGame(gameStartData);

// 游戏结束
FactoryManager.Instance.ClearAllObjects();

// 获取统计信息
GameStats stats = FactoryManager.Instance.GetGameStats();
```

## 接口实现

### IPoolable 接口
所有池化对象都需要实现 `IPoolable` 接口：

```csharp
public interface IPoolable
{
    void OnSpawnFromPool();    // 从池中取出时调用
    void OnReturnToPool();     // 返回池中时调用
}
```

### 示例实现
```csharp
public class TankController : MonoBehaviour, IPoolable
{
    public void OnSpawnFromPool()
    {
        // 重置状态
        isDead = false;
        isMoving = false;
        // ...
    }

    public void OnReturnToPool()
    {
        // 清理状态
        transform.DOKill();
        // ...
    }
}
```

## 性能优化

### 1. 对象池大小
- 坦克池：建议 10-20 个
- 子弹池：建议 30-50 个

### 2. 延迟回收
对于有动画的对象，使用延迟回收：
```csharp
// 坦克死亡动画
TankFactory.Instance.RecycleTankDelayed(tank, deathAnimTime);

// 子弹爆炸动画
BulletFactory.Instance.RecycleBulletDelayed(bullet, explosionTime);
```

### 3. 批量操作
```csharp
// 批量创建坦克
TankFactory.Instance.CreateTanksForGame(gameStartData);

// 批量清理
FactoryManager.Instance.ClearAllObjects();
```

## 调试功能

### 1. 查看池状态
```csharp
FactoryManager.Instance.LogPoolStatus();
```

### 2. 获取统计信息
```csharp
GameStats stats = FactoryManager.Instance.GetGameStats();
Debug.Log($"Active Tanks: {stats.activeTanks}");
Debug.Log($"Active Bullets: {stats.activeBullets}");
```

### 3. 检查池大小
```csharp
int tankPoolSize = ObjectPoolManager.Instance.GetPoolSize("Tank");
int bulletPoolSize = ObjectPoolManager.Instance.GetPoolSize("Bullet");
```

## 注意事项

1. **初始化顺序**：确保 `ObjectPoolManager` 在其他工厂之前初始化
2. **预制体设置**：确保预制体上有正确的组件（TankController、BulletController）
3. **状态清理**：在 `OnReturnToPool` 中正确清理对象状态
4. **引用管理**：避免在池化对象中保持对场景对象的引用
5. **动画处理**：使用 `DOTween.Kill()` 停止动画，避免内存泄漏

## 扩展功能

### 1. 添加新的对象类型
1. 创建新的工厂类（如 `EnemyFactory`）
2. 在 `ObjectPoolManager` 中配置新的池
3. 实现 `IPoolable` 接口

### 2. 自定义回收策略
```csharp
// 自定义回收条件
if (bullet.IsOutOfBounds())
{
    BulletFactory.Instance.RecycleBullet(bullet);
}
```

### 3. 池大小动态调整
```csharp
// 根据游戏状态调整池大小
if (gameState.isHighIntensity)
{
    // 增加子弹池大小
}
``` 