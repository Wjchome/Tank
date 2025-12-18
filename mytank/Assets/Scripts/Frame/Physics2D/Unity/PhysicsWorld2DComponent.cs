using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

namespace Physics2D
{
    /// <summary>
    /// Unity组件：物理世界管理器
    /// </summary>
    public class PhysicsWorld2DComponent : SingletonMono<PhysicsWorld2DComponent>
    {
        /// <summary>
        /// 物理世界实例
        /// </summary>
        public PhysicsWorld2D World { get; private set; }

        /// <summary>
        /// 重力（Unity单位）
        /// </summary>
        public Vector2 gravity = new Vector2(0, -9.81f);
        
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int iterations = 8;

        //"每个节点最大存储物体数（超过则分裂）
        public int MaxObjectsPerNode;
        
        //"最大递归深度（防止无限分裂）
        public int MaxDepth;
        private void Awake()
        {
            // 创建物理世界
            World = new PhysicsWorld2D();
            World.Gravity = new FixVector2((Fix64)gravity.x, (Fix64)gravity.y);
            World.Iterations = iterations;
            World.quadTree.MaxDepth = MaxDepth;
            World.quadTree.MaxObjectsPerNode = MaxObjectsPerNode;

            World.IgnoreLayerCollision(PhysicsLayer.GetLayer((int)QuadTreeLayerType.TankEnemy),
                PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy));
            World.IgnoreLayerCollision(PhysicsLayer.GetLayer((int)QuadTreeLayerType.TankFriend),
                PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletFriend));
            World.IgnoreLayerCollision(PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy),
                PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletEnemy));
            World.IgnoreLayerCollision(PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletFriend),
                PhysicsLayer.GetLayer((int)QuadTreeLayerType.BulletFriend));
        }


        public void AddRigidBody(RigidBody2DComponent rigidBody, FixVector2 pos,PhysicsLayer layer)
        {
            rigidBody.Init(pos,layer);
        }

        public void UpdateFrame()
        {
            // 更新物理世界
            if (World != null)
            {
                World.Update();
                //Test.Instance.UpdateFrame();
            }
        }

        private void OnDestroy()
        {
            if (World != null)
            {
                World.Clear();
            }
        }

        /// <summary>
        /// 在Scene视图中绘制四叉树节点（调试用）
        /// </summary>
        private void OnDrawGizmos()
        {
            if (World == null || World.quadTree == null) return;

            World.quadTree.DrawGizmos();
        }
    }
}