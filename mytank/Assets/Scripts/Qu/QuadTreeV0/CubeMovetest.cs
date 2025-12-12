using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuadTreeV0
{
    public class CubeMovetest : MonoBehaviour
    {
        public bool isMove = false;

        public Vector2 velocity;
        public Vector2 acceleration;

        public float factor1;
        public float factor2;

        private Rect bounds => new Rect(transform.position.x - transform.localScale.x / 2,
            transform.position.y - transform.localScale.y / 2, transform.localScale.x, transform.localScale.y);

        private void Update()
        {
            if (isMove)
            {
                acceleration += new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1));
                velocity += acceleration * (Time.deltaTime * factor1);
                transform.position += (Vector3)velocity * (Time.deltaTime * factor2);
                QuadTree.ins.UpdateObjectPosition(gameObject, bounds);
                var pos = transform.position;
                pos.x = ConstrainCoordinate(pos.x, -8, 8);
                pos.y = ConstrainCoordinate(pos.y, -3, 5);
                transform.position = pos;
            }
        }

        private float ConstrainCoordinate(float value, float min, float max)
        {
            float range = max - min;
            if (range <= 0) return min; // 防止除数为0

            // 核心取模约束逻辑
            value = (value - min) % range;
            if (value < 0) value += range;
            value += min;

            return value;
        }
    }
}