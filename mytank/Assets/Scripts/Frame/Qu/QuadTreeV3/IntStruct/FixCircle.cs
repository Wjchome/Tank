using System;
using FixMath.NET;

namespace QuadTreeV3
{
    /// <summary>
    /// 整数圆形结构（用于帧同步，保证确定性）
    /// </summary>
    public struct FixCircle : IEquatable<FixCircle>,IComparable<FixCircle>
    {
        public Fix64 CenterX; // 圆心X坐标
        public Fix64 CenterY; // 圆心Y坐标
        public Fix64 Radius; // 半径（非负整数）

        public FixCircle(Fix64 centerX,Fix64 centerY,Fix64 radius)
        {
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
        }

        /// <summary>
        /// 判断圆形是否与矩形重叠（用于四叉树查询）
        /// </summary>
        public bool Overlaps(FixRect rect)
        {    
            // 找到矩形上距离圆心最近的点
            Fix64 closestX = Fix64.Max(rect.X, Fix64.Min(CenterX, rect.Right));
            Fix64 closestY = Fix64.Max(rect.Y, Fix64.Min(CenterY, rect.Top));
            
            // 计算最近点到圆心的距离的平方（避免开方，提高性能）
            Fix64 dx = CenterX - closestX;
            Fix64 dy = CenterY - closestY;
            Fix64 distanceSquared = dx * dx + dy * dy;
            
            // 如果距离平方 <= 半径平方，则重叠
            return distanceSquared <= Radius * Radius;
        }

        /// <summary>
        /// 判断两个圆形是否重叠
        /// </summary>
        public bool Overlaps(FixCircle other)
        {
            // 计算圆心距离的平方
            Fix64 dx = CenterX - other.CenterX;
            Fix64 dy = CenterY - other.CenterY;
            Fix64 distanceSquared = dx * dx + dy * dy;
            
            // 如果距离 <= 两圆半径之和，则重叠
            Fix64 radiusSum = Radius + other.Radius;
            return distanceSquared <= radiusSum * radiusSum;
        }

        /// <summary>
        /// 判断点是否在圆形内（包含边界）
        /// </summary>
        public bool Contains(Fix64 x, Fix64 y)
        {
            Fix64 dx = x - CenterX;
            Fix64 dy = y - CenterY;
            Fix64 distanceSquared = dx * dx + dy * dy;
            return distanceSquared <= Radius * Radius;
        }

        /// <summary>
        /// 获取圆形的包围矩形（用于四叉树存储）
        /// </summary>
        public FixRect GetBoundingRect()
        {
            return new FixRect(
                CenterX - Radius,
                CenterY - Radius,
                Radius * new Fix64(2),
                Radius * new Fix64(2)
            );
        }

        // 帧同步必备：重写Equals
        public bool Equals(FixCircle other)
        {
            return CenterX == other.CenterX && CenterY == other.CenterY && Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            return obj is FixCircle other && Equals(other);
        }

        // 帧同步必备：确定性哈希
        public override int GetHashCode()
        {
            return HashCode.Combine(CenterX, CenterY, Radius);
        }

        // 方便调试
        public override string ToString()
        {
            return $"Center:({CenterX}, {CenterY}), Radius:{Radius}";
        }
        public int CompareTo(FixCircle other)
        {
            if(this.Equals(other))
                return 0;
            return CenterX.CompareTo(other.CenterX);
        }
    }
}

