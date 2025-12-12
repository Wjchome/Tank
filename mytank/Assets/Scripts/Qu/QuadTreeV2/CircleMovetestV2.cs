using System;
using UnityEngine;
using Random = UnityEngine.Random;
using FixMath.NET;

namespace QuadTreeV2
{
    public class CircleMovetestV2 : MonoBehaviour
    {
        public bool isMove = false;

        public FixVector2 velocity;
        public FixVector2 acceleration;

        public float factor1;
        public float factor2;


        public FixCircle bounds => new FixCircle(
            (Fix64)(pos.x),
            (Fix64)(pos.y),
            (Fix64)(size));

        public FixVector2 pos;
        public Fix64 size;

        public void Init(Vector2 pos,float size)
        {
            this.pos = new FixVector2(pos);
            this.size = (Fix64)size;
            // 添加物体到四叉树（边界为物体中心±size/2）
            QuadTreeV2.ins.AddObjectToQuadTree(bounds, gameObject);
        }

        public void UpdateFrame()
        {
            if (isMove)
            {
                acceleration += new FixVector2((Fix64)Random.Range(-1f, 1), (Fix64)Random.Range(-1f, 1));
                velocity += acceleration * (Fix64)(Time.deltaTime * factor1);
                pos += velocity * (Fix64)(Time.deltaTime * factor2);
                pos.x = ConstrainCoordinate(pos.x, QuadTreeV2.ins.RootRect.X,
                    QuadTreeV2.ins.RootRect.X + QuadTreeV2.ins.RootRect.Width);
                pos.y = ConstrainCoordinate(pos.y, QuadTreeV2.ins.RootRect.Y,
                    QuadTreeV2.ins.RootRect.Y + QuadTreeV2.ins.RootRect.Height);
                transform.position = (Vector2)pos;
                QuadTreeV2.ins.UpdateObjectPosition(gameObject, bounds.GetBoundingRect());
            }
        }

        private Fix64 ConstrainCoordinate(Fix64 value, Fix64 min, Fix64 max)
        {
            Fix64 range = max - min;
            if (range <= Fix64.Zero) return min; // 防止除数为0

            // 核心取模约束逻辑
            value = (value - min) % range;
            if (value < Fix64.Zero) value += range;
            value += min;

            return value;
        }
    }
}