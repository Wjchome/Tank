using System;
using UnityEngine;
using Random = UnityEngine.Random;
using FixMath.NET;

namespace QuadTreeV3
{
    public class CubeMovetestV3 : MonoBehaviour
    {
        public bool isMove = false;

        public FixVector2 velocity;
        public FixVector2 acceleration;

        public float factor1;
        public float factor2;

        public Fix64 rotation;
        public FixRect bounds=>new FixRect(
            (Fix64)(pos.x - size.x / new Fix64(2)),
            (Fix64)(pos.y - size.y / new Fix64(2)),
            (Fix64)(size.x),
            (Fix64)(size.y),rotation);

        public FixVector2 pos;
        public FixVector2 size;
        [Header("层设置")]
        [Tooltip("物体所在的层（0-31）")]
        public int layer = 0;

        public void Init(Vector2 pos, Vector2 size)
        {
            this.pos = new FixVector2(pos);
            this.size = new FixVector2(size);
            // 添加物体到四叉树（边界为物体中心±size/2）
            if (QuadTreeV3.Instance != null)
            {
                QuadTreeV3.Instance.AddObject(bounds, gameObject, QuadTreeLayer.GetLayer(layer));
            }
        }
        
        public void UpdateFrame()
        {
            if (isMove && QuadTreeV3.Instance != null)
            {
                acceleration += new FixVector2((Fix64)Random.Range(-1f, 1), (Fix64)Random.Range(-1f, 1));
                velocity += acceleration * (Fix64)(Time.deltaTime * factor1);
                pos += velocity * (Fix64)(Time.deltaTime * factor2);
                pos.x = ConstrainCoordinate(pos.x, QuadTreeV3.Instance.RootRect.X, QuadTreeV3.Instance.RootRect.X + QuadTreeV3.Instance.RootRect.Width);
                pos.y = ConstrainCoordinate(pos.y, QuadTreeV3.Instance.RootRect.Y, QuadTreeV3.Instance.RootRect.Y + QuadTreeV3.Instance.RootRect.Height);
                rotation += (Fix64)(Time.deltaTime * factor1);
                QuadTreeV3.Instance.UpdateObject(gameObject, bounds);
                
                transform.position = (Vector2)pos;
                transform.rotation = Quaternion.Euler(0, 0, (float)bounds.Rotate);

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