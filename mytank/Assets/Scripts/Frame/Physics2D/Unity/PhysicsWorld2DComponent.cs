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
        /// 时间步长（秒）
        /// </summary>
        public float timeStep = 1f / 60f;

        /// <summary>
        /// 迭代次数
        /// </summary>
        public int iterations = 8;


        private void Awake()
        {
            // 创建物理世界
            World = new PhysicsWorld2D();
            World.Gravity = new FixVector2((Fix64)gravity.x, (Fix64)gravity.y);
            World.TimeStep = (Fix64)timeStep;
            World.Iterations = iterations;

        }


        public void AddRigidBody(RigidBody2DComponent rigidBody, FixVector2 pos)
        {
            rigidBody.Init(pos);
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