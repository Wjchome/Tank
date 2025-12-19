using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using UnityEngine;

namespace Physics2D
{
    /// <summary>
    /// 2D物理世界（管理所有物理体，执行物理模拟）
    /// </summary>
    public class PhysicsWorld2D
    {
        /// <summary>
        /// 所有物理体列表
        /// </summary>
        private List<RigidBody2D> bodies = new List<RigidBody2D>();

        /// <summary>
        /// 四叉树（用于宽相位碰撞检测，优化性能）
        /// </summary>
        public QuadTree quadTree;

        /// <summary>
        /// 重力加速度（默认无）
        /// </summary>
        public FixVector2 Gravity { get; set; } = FixVector2.Zero;


        /// <summary>
        /// 迭代次数（用于碰撞分离，提高稳定性）
        /// </summary>
        public int Iterations { get; set; } = 8;

        /// <summary>
        /// 子步迭代次数（将一个时间步分成多个子步，提高物理模拟稳定性）
        /// 建议值：2-4，值越大越稳定但性能开销也越大
        /// 子步迭代可以有效处理高速移动物体，避免穿透等问题
        /// </summary>
        public int SubSteps { get; set; } = 2;


        public int nextId = 0;

        /// <summary>
        /// 碰撞矩阵（用于定义哪些Layer之间应该忽略碰撞）
        /// Key: (layerA.value, layerB.value) 的元组，Value: 是否忽略碰撞（true表示忽略）
        /// 使用双向存储，确保无论顺序如何都能快速查找
        /// </summary>
        private Dictionary<(int, int), bool> _collisionMatrix = new Dictionary<(int, int), bool>();

        /// <summary>
        /// 子步迭代状态缓存（用于在子步之间保存和恢复状态）
        /// </summary>
        private Dictionary<int, (FixVector2 position, FixVector2 velocity)> _subStepStateCache = new Dictionary<int, (FixVector2, FixVector2)>();

        public PhysicsWorld2D()
        {
            quadTree = new QuadTree();
        }

        /// <summary>
        /// 添加物理体到世界
        /// </summary>
        public void AddBody(RigidBody2D body)
        {
            if (body == null)
            {
                throw new System.Exception("物理不存在");
            }

            if (body.World != null && body.World != this)
            {
                throw new System.Exception("物理体已属于其他世界");
            }


            if (!bodies.Contains(body))
            {
                bodies.Add(body);
                body.World = this;
                body.id = ++nextId;

                // 标记为脏，下次更新时添加到四叉树
                body.QuadTreeDirty = true;
                body.InQuadTree = false;
            }
            else
            {
                Debug.LogError("物体已经在这个世界");
            }
        }

        /// <summary>
        /// 从世界移除物理体
        /// </summary>
        public void RemoveBody(RigidBody2D body)
        {
            if (body == null) return;
            if (bodies.Remove(body))
            {
                if (body.InQuadTree)
                {
                    quadTree.RemoveObject(body);
                    body.InQuadTree = false;
                }

                body.World = null;
            }
        }


        /// <summary>
        /// 增量更新四叉树（只更新移动的物体，支持自动扩容）
        /// 优化：静态物体不需要频繁更新
        /// </summary>
        private void UpdateQuadTreeIncremental()
        {
            // 1. 检查是否需要扩容（检测物体是否超出边界）
            if (quadTree.CheckAndExpand(bodies))
            {
                // 扩容后已经重建，清除所有脏标记
                foreach (var body in bodies)
                {
                    body.QuadTreeDirty = false;
                    body.InQuadTree = true;
                    body.PreviousAABB = body.Shape.GetBounds(body.Position);
                }

                return;
            }

            // 2. 增量更新：只更新脏标记的物体
            foreach (var body in bodies)
            {
                if (body.QuadTreeDirty)
                {
                    FixRect currentAABB = body.Shape.GetBounds(body.Position);

                    if (!body.InQuadTree)
                    {
                        // 新物体：添加到四叉树
                        quadTree.AddObject(body);
                        body.InQuadTree = true;
                    }
                    else
                    {
                        // 检查位置是否改变（使用AABB比较）
                        if (!currentAABB.Equals(body.PreviousAABB))
                        {
                            // 位置改变：更新四叉树
                            quadTree.UpdateObject(body);
                        }
                    }

                    body.PreviousAABB = currentAABB;
                    body.QuadTreeDirty = false;
                }
            }
        }
        public void Update()
        {
            // 子步迭代：将时间步长分成多个子步
            Fix64 deltaTime = Fix64.One;
            Fix64 subStepDeltaTime = deltaTime / (Fix64)SubSteps;

            // 执行每个子步
            for (int subStep = 0; subStep < SubSteps; subStep++)
            {
                // 如果不是第一个子步，恢复上一个子步的状态
                if (subStep > 0)
                {
                    RestoreState();
                }

                // 执行单个子步
                UpdateSingleStep(subStepDeltaTime);

                // 如果不是最后一个子步，保存当前状态用于下一个子步
                if (subStep < SubSteps - 1)
                {
                    SaveState();
                }
            }

            // 最后处理碰撞回调（只在所有子步完成后处理一次）
            ProcessAllBody();
        }

        /// <summary>
        /// 执行单个子步的物理更新
        /// </summary>
        /// <param name="deltaTime">子步的时间步长</param>
        private void UpdateSingleStep(Fix64 deltaTime)
        {
            // 2. 收集所有常态力（重力等）
            CollectForces();

            // 3. 计算加速度并更新速度 更新位置（积分）
            UpdatePositions(deltaTime);

            // 4. 在迭代前更新一次四叉树（优化：避免在每次迭代中重复更新）
            UpdateQuadTreeIncremental();

            // 5. 碰撞检测和响应（迭代多次以提高稳定性）
            for (int i = 0; i < Iterations; i++)
            {
                ResolveCollisions();
            }

            // 1. 清除所有物体的力累加器
            ClearForces();
        }

        /// <summary>
        /// 清除所有物体的力累加器
        /// </summary>
        private void ClearForces()
        {
            foreach (var body in bodies)
            {
                if (body.IsDynamic)
                {
                    body.ClearForces();
                }
            }
        }

        /// <summary>
        /// 收集所有力（重力、用户施加的力等）
        /// </summary>
        private void CollectForces()
        {
            foreach (var body in bodies)
            {
                if (body.IsDynamic && body.UseGravity)
                {
                    // 重力 = 质量 * 重力加速度
                    FixVector2 gravityForce = Gravity * body.Mass;
                    body.ApplyForce(gravityForce);
                }
            }
        }

        /// <summary>
        /// 计算加速度并更新速度
        /// 根据累积的力计算加速度：a = F/m
        /// 然后更新速度：v = v + a*dt
        /// </summary>
        /// <param name="deltaTime">时间步长（用于子步迭代）</param>
        private void UpdatePositions(Fix64 deltaTime = default)
        {
            // 如果deltaTime为0，使用默认值1（兼容旧代码）
            if (deltaTime == Fix64.Zero)
            {
                deltaTime = Fix64.One;
            }

            foreach (var body in bodies)
            {
                if (body.IsDynamic)
                {
                    // F = ma => a = F/m
                    FixVector2 acceleration = body.ForceAccumulator / body.Mass;

                    // 更新速度：v = v + a*dt
                    body.Velocity += acceleration * deltaTime;

                    FixVector2 oldPosition = body.Position;
                    // 简单欧拉积分：x = x + v * dt
                    body.Position += body.Velocity * deltaTime;

                    // 标记为脏（位置改变，需要更新四叉树）
                    // 优化：静态物体不会移动，不需要标记
                    if (oldPosition != body.Position)
                    {
                        body.QuadTreeDirty = true;
                    }

                    // 应用线性阻尼（在空地上减速）
                    // 注意：阻尼应该在每个子步都应用，但需要根据deltaTime调整
                    if (body.LinearDamping > Fix64.Zero)
                    {
                        // 阻尼公式：v = v * (1 - damping * dt)
                        // 对于子步，需要根据deltaTime调整
                        Fix64 dampingFactor = Fix64.One - Fix64.Clamp(body.LinearDamping * deltaTime, Fix64.Zero, Fix64.One);
                        body.Velocity *= dampingFactor;
                    }
                }
            }
        }
        

        /// <summary>
        /// 使用四叉树优化的碰撞检测（O(n log n)）
        /// 优化：四叉树只做AABB快速筛选，精确检测在物理系统中进行，避免重复计算SAT
        /// </summary>
        private HashSet<(int, int)> _checkedPairsCache = new HashSet<(int, int)>();

        private void ResolveCollisions()
        {
            // 使用HashSet避免重复检测同一对物体（重用缓存，减少GC）
            _checkedPairsCache.Clear();

            for (int i = 0; i < bodies.Count; i++)
            {
                RigidBody2D bodyA = bodies[i];
                // 只检测动态物体或触发器（触发器需要检测碰撞用于回调）
                if (!bodyA.IsDynamic)
                {
                    continue;
                }

                FixRect aabbA = bodyA.Shape.GetBounds(bodyA.Position);
                // 宽相位：使用AABB快速筛选候选对（不进行精确检测）
                var candidates = quadTree.Query(aabbA);
                foreach (var bodyB in candidates)
                {
                    if (bodyB.Equals(bodyA)) continue;
                    
                    // 跳过静态-静态碰撞对（静态物体不会相互碰撞）
                    if (!bodyA.IsDynamic && !bodyB.IsDynamic) continue;
                    
                    var pair = bodyA.id < bodyB.id ? (bodyA.id, bodyB.id) : (bodyB.id, bodyA.id);
                    if (_checkedPairsCache.Contains(pair)) continue;
                    _checkedPairsCache.Add(pair);

                    // 检查Layer碰撞矩阵（是否忽略了这两个Layer之间的碰撞）
                    if (ShouldIgnoreCollision(bodyA.Layer, bodyB.Layer))
                    {
                        continue; // 忽略碰撞，跳过物理响应和碰撞记录
                    }

                    // 窄相位：精确碰撞检测（这里会使用FixRect.Overlaps()的SAT，只计算一次）
                    if (CollisionShape2D.CheckCollision(
                            bodyA.Shape, bodyA.Position,
                            bodyB.Shape, bodyB.Position,
                            out Contact2D contact))
                    {
                        Record(bodyA, bodyB);
                        ResolveContact(bodyA, bodyB, contact);
                    }
                }
            }
        }

        private void Record(RigidBody2D bodyA, RigidBody2D bodyB)
        {
            bodyA.CurrentRigidBody2D.Add(bodyB);
            bodyB.CurrentRigidBody2D.Add(bodyA);
        }


        /// <summary>
        /// 处理碰撞响应（分离物体并修正速度）
        /// 触发器只记录碰撞，不进行物理响应
        /// </summary>
        private void ResolveContact(RigidBody2D bodyA, RigidBody2D bodyB, Contact2D contact)
        {
            // 如果至少有一个是触发器，只记录碰撞，不进行物理响应
            if (bodyA.IsTrigger || bodyB.IsTrigger)
            {
                // 触发器：只记录碰撞，不分离、不修正速度
                return;
            }
            // 如果两个都是静态物体，不需要分离
            if (!bodyA.IsDynamic && !bodyB.IsDynamic)
            {
                return;
            }
            // 1. 分离重叠的物体
            SeparateBodies(bodyA, bodyB, contact);

            // 2. 修正速度（碰撞响应）
            ResolveVelocity(bodyA, bodyB, contact);
        }

        /// <summary>
        /// 分离重叠的物体
        /// </summary>
        private void SeparateBodies(RigidBody2D bodyA, RigidBody2D bodyB, Contact2D contact)
        {
            

            // 计算需要移动的距离（根据质量分配）
            Fix64 totalMass = bodyA.Mass + bodyB.Mass;

            // 质量越大，移动距离越小
            Fix64 moveA = bodyB.Mass / totalMass;
            Fix64 moveB = bodyA.Mass / totalMass;

            // 如果某个物体是静态的，只移动另一个
            if (!bodyA.IsDynamic)
            {
                moveA = Fix64.Zero;
                moveB = Fix64.One;
            }
            else if (!bodyB.IsDynamic)
            {
                moveA = Fix64.One;
                moveB = Fix64.Zero;
            }

            // 分离向量
            FixVector2 separation = contact.Normal * contact.Penetration;

            // 移动物体
            bodyA.Position -= separation * moveA;
            bodyB.Position += separation * moveB;

            // 位置改变，标记为脏（需要更新四叉树）
            if (moveA != Fix64.Zero) bodyA.QuadTreeDirty = true;
            if (moveB != Fix64.Zero) bodyB.QuadTreeDirty = true;
        }

        /// <summary>
        /// 修正速度（碰撞响应）
        /// </summary>
        private void ResolveVelocity(RigidBody2D bodyA, RigidBody2D bodyB, Contact2D contact)
        {
            // 计算相对速度
            FixVector2 relativeVelocity = bodyB.Velocity - bodyA.Velocity;
            if (relativeVelocity != FixVector2.Zero)
            {
                int a = 1;
            }
            // 计算沿法向量方向的相对速度
            Fix64 velocityAlongNormal = FixVector2.Dot(relativeVelocity, contact.Normal);

            // 如果物体正在分离，不需要处理
            if (velocityAlongNormal > Fix64.Zero)
                return;

            // 计算恢复系数（弹性）
            Fix64 restitution = Fix64.Min(bodyA.Restitution, bodyB.Restitution);

            // 计算冲量大小
            // j = -(1 + e) * v_rel · n / (1/mA + 1/mB)
            Fix64 invMassA = bodyA.IsDynamic ? Fix64.One / bodyA.Mass : Fix64.Zero;
            Fix64 invMassB = bodyB.IsDynamic ? Fix64.One / bodyB.Mass : Fix64.Zero;
            Fix64 invMassSum = invMassA + invMassB;

            // 防止除零（两个静态物体不应该进入这里，但添加保护）
            if (invMassSum == Fix64.Zero)
            {
                return;
            }

            Fix64 impulseMagnitude = -(Fix64.One + restitution) * velocityAlongNormal / invMassSum;

            // 应用冲量
            FixVector2 impulse = contact.Normal * impulseMagnitude;
            bodyA.ApplyImpulse(-impulse);
            bodyB.ApplyImpulse(impulse);

            // 处理摩擦力（简化版本，只处理切向速度）
            FixVector2 tangent = relativeVelocity - contact.Normal * velocityAlongNormal;
            Fix64 tangentLength = tangent.Magnitude();
            if (tangentLength > Fix64.Zero)
            {
                FixVector2 tangentDir = tangent / tangentLength;
                
                // 计算切向速度大小（沿切向方向的相对速度）
                Fix64 tangentVelocity = FixVector2.Dot(relativeVelocity, tangentDir);
                
                // 计算摩擦力冲量（尝试消除切向速度）
                // 注意：这里需要除以invMassSum来得到正确的冲量
                Fix64 frictionImpulse = -tangentVelocity / invMassSum;

                // 限制摩擦力（库仑摩擦：摩擦力不能超过法向力乘以摩擦系数）
                Fix64 frictionCoeff = Fix64.Sqrt(bodyA.Friction * bodyB.Friction);
                Fix64 maxFriction = Fix64.Abs(impulseMagnitude) * frictionCoeff;
                
                // 限制摩擦力大小（库仑摩擦定律）
                frictionImpulse = Fix64.Clamp(frictionImpulse, -maxFriction, maxFriction);

                FixVector2 friction = tangentDir * frictionImpulse;
                bodyA.ApplyImpulse(-friction);
                bodyB.ApplyImpulse(friction);
            }
        }

        private void ProcessAllBody()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                RigidBody2D bodyA = bodies[i];


                bodyA.CurrentRigidBody2D = bodyA.CurrentRigidBody2D.GetUniqueList();
                bodyA.Enter = bodyA.CurrentRigidBody2D.UniqueExcept(bodyA.LastRigidBody2D);
                bodyA.Stay = bodyA.CurrentRigidBody2D.UniqueIntersect(bodyA.LastRigidBody2D);
                bodyA.Exit = bodyA.LastRigidBody2D.UniqueExcept(bodyA.CurrentRigidBody2D);
                bodyA.LastRigidBody2D = bodyA.CurrentRigidBody2D.ToList();
                bodyA.CurrentRigidBody2D.Clear();
            }
        }


        /// <summary>
        /// 清除所有物理体
        /// </summary>
        public void Clear()
        {
            foreach (var body in bodies)
            {
                body.World = null;
            }

            bodies.Clear();
            _subStepStateCache.Clear();
        }

        #region 子步迭代状态管理

        /// <summary>
        /// 保存当前所有物体的状态（位置和速度）
        /// 用于子步迭代之间的状态传递
        /// </summary>
        private void SaveState()
        {
            _subStepStateCache.Clear();
            foreach (var body in bodies)
            {
                if (body.IsDynamic)
                {
                    _subStepStateCache[body.id] = (body.Position, body.Velocity);
                }
            }
        }

        /// <summary>
        /// 恢复所有物体的状态（位置和速度）
        /// 用于子步迭代之间的状态传递
        /// 在子步开始时调用，从上一步的结果继续
        /// </summary>
        private void RestoreState()
        {
            foreach (var body in bodies)
            {
                if (body.IsDynamic && _subStepStateCache.TryGetValue(body.id, out var state))
                {
                    body.Position = state.position;
                    body.Velocity = state.velocity;
                    body.QuadTreeDirty = true; // 位置改变，标记为脏
                }
            }
        }



        #endregion

        #region Layer碰撞矩阵管理（类似Unity的Physics.IgnoreCollision）

        /// <summary>
        /// 检查两个Layer之间是否应该忽略碰撞
        /// </summary>
        /// <param name="layerA">Layer A</param>
        /// <param name="layerB">Layer B</param>
        /// <returns>如果应该忽略碰撞返回true，否则返回false</returns>
        private bool ShouldIgnoreCollision(PhysicsLayer layerA, PhysicsLayer layerB)
        {
            // 如果两个Layer都没有设置（默认值），不忽略
            if (layerA.value == 0 && layerB.value == 0)
            {
                return false;
            }

            // 检查碰撞矩阵（双向检查）
            var key1 = (layerA.value, layerB.value);
            var key2 = (layerB.value, layerA.value);

            if (_collisionMatrix.TryGetValue(key1, out bool ignore1))
            {
                return ignore1;
            }

            if (_collisionMatrix.TryGetValue(key2, out bool ignore2))
            {
                return ignore2;
            }

            // 默认不忽略
            return false;
        }

        /// <summary>
        /// 忽略两个Layer之间的碰撞（双向忽略）
        /// 类似Unity的Physics.IgnoreCollision，但基于Layer而不是具体的GameObject
        /// </summary>
        /// <param name="layerA">Layer A</param>
        /// <param name="layerB">Layer B</param>
        /// <example>
        /// // 忽略玩家子弹和玩家之间的碰撞
        /// world.IgnoreLayerCollision(PhysicsLayer.GetLayer(PlayerLayer), PhysicsLayer.GetLayer(PlayerBulletLayer));
        /// 
        /// // 忽略敌人子弹和敌人之间的碰撞
        /// world.IgnoreLayerCollision(PhysicsLayer.GetLayer(EnemyLayer), PhysicsLayer.GetLayer(EnemyBulletLayer));
        /// </example>
        public void IgnoreLayerCollision(PhysicsLayer layerA, PhysicsLayer layerB)
        {
            if (layerA.value == 0 && layerB.value == 0)
            {
                return; // 两个都是默认Layer，不需要设置
            }

            // 双向存储，确保无论顺序如何都能快速查找
            var key1 = (layerA.value, layerB.value);
            var key2 = (layerB.value, layerA.value);

            _collisionMatrix[key1] = true;
            _collisionMatrix[key2] = true;
        }

        /// <summary>
        /// 恢复两个Layer之间的碰撞（双向恢复）
        /// </summary>
        /// <param name="layerA">Layer A</param>
        /// <param name="layerB">Layer B</param>
        public void ResumeLayerCollision(PhysicsLayer layerA, PhysicsLayer layerB)
        {
            var key1 = (layerA.value, layerB.value);
            var key2 = (layerB.value, layerA.value);

            _collisionMatrix.Remove(key1);
            _collisionMatrix.Remove(key2);
        }

        /// <summary>
        /// 清除所有Layer碰撞忽略设置
        /// </summary>
        public void ClearLayerCollisionMatrix()
        {
            _collisionMatrix.Clear();
        }

        #endregion
    }
}