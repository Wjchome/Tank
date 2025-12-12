using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuadTreeV1
{
    public class CubeMovetestV1 : MonoBehaviour
    {
        public bool isMove = false;

        public Vector2 velocity;
        public Vector2 acceleration;

        public float factor1;
        public float factor2;


        public IntRect bounds => new IntRect(
            (int)((transform.position.x - transform.localScale.x / 2)*QuadTreeConfig.BIGMULTIPLIER),
            (int)((transform.position.y - transform.localScale.y / 2)*QuadTreeConfig.BIGMULTIPLIER),
            (int)(transform.localScale.x*QuadTreeConfig.BIGMULTIPLIER),
            (int)(transform.localScale.y*QuadTreeConfig.BIGMULTIPLIER));
        private void Update()
        {
            if (isMove)
            {
                acceleration += new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1));
                velocity += acceleration * (Time.deltaTime * factor1);
                transform.position += (Vector3)velocity * (Time.deltaTime * factor2);
                QuadTreeV1.ins.UpdateObjectPosition(gameObject, bounds);
                var pos = transform.position;
                pos.x = ConstrainCoordinate(pos.x, QuadTreeV1.ins.Bounds.x,QuadTreeV1.ins.Bounds.x+QuadTreeV1.ins.Bounds.width);
                pos.y = ConstrainCoordinate(pos.y, QuadTreeV1.ins.Bounds.y,QuadTreeV1.ins.Bounds.y+QuadTreeV1.ins.Bounds.height);
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