using System;

namespace QuadTreeV1
{
    public struct IntRect : IEquatable<IntRect>,IComparable<IntRect>
    {
        // 核心字段：全部为int，无float
        public int X; // 左边界
        public int Y; // 下边界（和Unity Rect坐标系一致）
        public int Width; // 宽度（正数）
        public int Height; // 高度（正数）

        // 派生属性（只读，纯整数计算）
        public int Right => X + Width; // 右边界
        public int Top => Y + Height; // 上边界
        public int CenterX => X + Width / 2; // 中心X
        public int CenterY => Y + Height / 2; // 中心Y

        // 构造函数（强制校验宽度/高度为正，避免无效数据）
        public IntRect(int x, int y, int width, int height)
        {
            if (width < 0) throw new ArgumentException("宽度不能为负", nameof(width));
            if (height < 0) throw new ArgumentException("高度不能为负", nameof(height));

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 判断当前矩形是否与另一个矩形重叠（纯整数计算，确定性）
        /// </summary>
        public bool Overlaps(IntRect other)
        {
            // 核心逻辑：不重叠的4种情况取反（无浮点、无舍入）
            return !(Right < other.X // 当前矩形在对方左侧
                     || X > other.Right // 当前矩形在对方右侧
                     || Top < other.Y // 当前矩形在对方下侧
                     || Y > other.Top); // 当前矩形在对方上侧
        }

        /// <summary>
        /// 判断点是否在矩形内（包含边界，纯整数）
        /// </summary>
        public bool Contains(int x, int y)
        {
            return x >= X && x <= Right
                          && y >= Y && y <= Top;
        }

        /// <summary>
        /// 生成圆形的包围矩形（适配圆形检测）
        /// </summary>
        public static IntRect FromCircle(int centerX, int centerY, int radius)
        {
            return new IntRect(
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2
            );
        }

        // 帧同步必备：重写Equals，避免引用比较
        public bool Equals(IntRect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is IntRect other && Equals(other);
        }

        // 帧同步必备：确定性哈希（基于整数字段）
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        // 方便调试：输出整数格式
        public override string ToString()
        {
            return $"X:{X}, Y:{Y}, W:{Width}, H:{Height}, Right:{Right}, Top:{Top}";
        }
        
        public int CompareTo(IntRect other)
        {
            if(this.Equals(other))
                return 0;
            return X.CompareTo(other.X);
        }
    }
}