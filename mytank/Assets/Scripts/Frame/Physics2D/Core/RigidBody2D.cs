using System;
using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;

namespace Physics2D
{
    /// <summary>
    /// 2D刚体（不考虑旋转，只处理平移运动）
    /// 基于定点数，用于帧同步
    /// </summary>
    public class RigidBody2D : IComparable<RigidBody2D>, IEquatable<RigidBody2D>
    {

        /// <summary>
        /// 是否是触发器
        /// </summary>
        public bool IsTrigger { get; set; }

        /// <summary>
        /// 位置（世界坐标）
        /// </summary>
        public FixVector2 Position { get; set; }
        

        /// <summary>
        /// 速度（单位：单位/秒）
        /// </summary>
        public FixVector2 Velocity { get; set; }

        /// <summary>
        /// 质量（单位：千克）
        /// 质量为0或负数表示静态物体（不受力影响）
        /// </summary>
        public Fix64 Mass { get; set; }

        /// <summary>
        /// 是否受重力影响
        /// </summary>
        public bool UseGravity { get; set; } = true;

        /// <summary>
        /// 是否为动态物体（false表示静态/运动学物体）
        /// </summary>
        public bool IsDynamic => !IsStatic && Mass > Fix64.Zero;

        /// <summary>
        /// 碰撞形状（圆形或矩形）
        /// </summary>
        public CollisionShape2D Shape { get; set; }

        /// <summary>
        /// 所属的物理世界
        /// </summary>
        public PhysicsWorld2D World { get; internal set; }

        public bool IsStatic { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public QuadTreeLayer Layer { get; set; }

        /// <summary>
        /// 弹性系数（0-1，0表示完全非弹性，1表示完全弹性）
        /// </summary>
        public Fix64 Restitution { get; set; } = Fix64.Zero;

        /// <summary>
        /// 摩擦系数（0-1）
        /// </summary>
        public Fix64 Friction { get; set; } = (Fix64)0.5m;

        /// <summary>
        /// 力累加器（每帧累积所有力，在Update中统一处理）
        /// </summary>
        internal FixVector2 ForceAccumulator { get; set; } = FixVector2.Zero;
        
        
        public GameObject gameObject;

        public int id;

        /// <summary>
        /// 脏标记：是否需要更新四叉树（位置改变）
        /// </summary>
        internal bool QuadTreeDirty { get; set; } = true;

        /// <summary>
        /// 是否在四叉树中
        /// </summary>
        internal bool InQuadTree { get; set; } = false;

        /// <summary>
        /// 上一帧的AABB（用于快速比较，避免不必要的更新）
        /// </summary>
        internal FixRect PreviousAABB { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <param name="mass">质量（0或负数表示静态物体）</param>
        /// <param name="shape">碰撞形状</param>
        public RigidBody2D(FixVector2 position, Fix64 mass, CollisionShape2D shape)
        {
            Position = position;
            Mass = mass;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Velocity = FixVector2.Zero;
        }

        /// <summary>
        /// 应用力（累积到力累加器中，不会立即改变速度）
        /// 力会在物理世界的Update循环中统一处理
        /// </summary>
        /// <param name="force">力向量</param>
        public void ApplyForce(FixVector2 force)
        {
            if (!IsDynamic) return;
            // 累积力，不直接修改速度
            ForceAccumulator += force;
        }

        /// <summary>
        /// 清除力累加器（每帧开始时调用）
        /// </summary>
        internal void ClearForces()
        {
            ForceAccumulator = FixVector2.Zero;
        }

        /// <summary>
        /// 应用冲量（直接改变速度）
        /// </summary>
        /// <param name="impulse">冲量向量</param>
        public void ApplyImpulse(FixVector2 impulse)
        {
            if (!IsDynamic) return;

            // J = mv => Δv = J/m
            Velocity += impulse / Mass;
        }

        #region 接口实现

        /// <summary>
        /// IComparable接口实现，使用id进行比较，确保确定性排序
        /// </summary>
        public int CompareTo(RigidBody2D other)
        {
            return id.CompareTo(other.id);
        }

        /// <summary>
        /// IEquatable接口实现，使用id进行比较，确保确定性相等判断
        /// </summary>
        public bool Equals(RigidBody2D other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RigidBody2D);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        #endregion


        #region 供外部适用

        public List<RigidBody2D> LastRigidBody2D=new List<RigidBody2D>();
        public List<RigidBody2D> CurrentRigidBody2D =new List<RigidBody2D>();
        
        public List<RigidBody2D> Enter;
        public List<RigidBody2D> Stay;
        public List<RigidBody2D> Exit;

        #endregion

        public override string ToString()
        {
            return $"RigidBody2D: {id} name:{gameObject}";
        }
    }
}


