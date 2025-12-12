using System;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

namespace QuadTreeV3
{
    /// <summary>
    /// 四叉树存储的物体封装（包含位置、尺寸、原始物体引用、层信息）
    /// </summary>
    public class QuadTreeObject : IComparable<QuadTreeObject>
    {
        /// <summary>
        /// 物体的矩形边界（位置+尺寸）
        /// </summary>
        public FixRect Bounds;
        
        /// <summary>
        /// 物体的圆形边界（可选，如果为null则使用矩形）
        /// </summary>
        public FixCircle? Circle;
        
        /// <summary>
        /// 对应的Unity物体
        /// </summary>
        public GameObject Target;
        
        /// <summary>
        /// 确定性ID（用于帧同步去重和排序）
        /// </summary>
        public int DeterministicId;
        
        /// <summary>
        /// 物体所在的层（位掩码，支持多层）
        /// </summary>
        public QuadTreeLayer Layer;



        /// <summary>
        /// 矩形物体构造函数
        /// </summary>
        /// <param name="bounds">矩形边界</param>
        /// <param name="target">Unity物体</param>
        /// <param name="layer">物体所在的层（默认为第0层）</param>
        public QuadTreeObject(FixRect bounds, GameObject target, QuadTreeLayer layer = default)
        {
            Bounds = bounds; // 存储原始 bounds（包含旋转信息）
            Circle = null;
            Target = target;
            Layer = layer.value == 0 ? QuadTreeLayer.GetLayer(0) : layer; // 默认第0层
            // 生成确定性ID
            DeterministicId = DeterministicIdGenerator.GetOrGenerateId(target);
        }

        /// <summary>
        /// 圆形物体构造函数
        /// </summary>
        /// <param name="circle">圆形边界</param>
        /// <param name="target">Unity物体</param>
        /// <param name="layer">物体所在的层（默认为第0层）</param>
        public QuadTreeObject(FixCircle circle, GameObject target, QuadTreeLayer layer = default)
        {
            Circle = circle;
            Bounds = circle.GetBoundingRect(); // 使用包围矩形用于四叉树存储
            Target = target;
            Layer = layer.value == 0 ? QuadTreeLayer.GetLayer(0) : layer; // 默认第0层
            // 生成确定性ID
            DeterministicId = DeterministicIdGenerator.GetOrGenerateId(target);
        }

        // 判断物体是否与矩形区域重叠
        public bool OverlapsRect(FixRect rect)
        {
            if (Circle.HasValue)
            {
                return Circle.Value.Overlaps(rect);
            }

            return Bounds.Overlaps(rect);
        }

        // 判断物体是否与圆形区域重叠
        public bool OverlapsCircle(FixCircle circle)
        {
            if (Circle.HasValue)
            {
                return Circle.Value.Overlaps(circle);
            }

            // 矩形与圆形重叠：检查矩形是否与圆形重叠
            return circle.Overlaps(Bounds);
        }

        // 重写Equals，确保按Target匹配物体（避免重复/误删）
        public override bool Equals(object obj)
        {
            if (obj is QuadTreeObject other)
            {
                return other.Target == this.Target;
            }

            return false;
        }

        // 重写GetHashCode，配合Equals使用（使用确定性ID）
        public override int GetHashCode()
        {
            return DeterministicId;
        }

        // 实现IComparable，用于DistinctBy去重（帧同步需要确定性排序）
        public int CompareTo(QuadTreeObject other)
        {
            return DeterministicId.CompareTo(other.DeterministicId);
        }
    }
}