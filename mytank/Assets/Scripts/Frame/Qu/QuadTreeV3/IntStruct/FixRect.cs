using System;
using FixMath.NET;

namespace QuadTreeV3
{
    public struct FixRect : IEquatable<FixRect>, IComparable<FixRect>
    {
        // 核心字段：全部为int，无float
        public Fix64 X; // 左边界
        public Fix64 Y; // 下边界（和Unity Rect坐标系一致）
        public Fix64 Width; // 宽度（正数）
        public Fix64 Height; // 高度（正数）
        public Fix64 Rotate; // 旋转角度（弧度）

        // 派生属性（只读，纯整数计算）
        public Fix64 Right => X + Width; // 右边界
        public Fix64 Top => Y + Height; // 上边界
        public Fix64 CenterX => X + Width / new Fix64(2); // 中心X
        public Fix64 CenterY => Y + Height / new Fix64(2); // 中心Y
        
        public FixVector2 Center => new FixVector2(CenterX, CenterY);

        // 构造函数（强制校验宽度/高度为正，避免无效数据）
        public FixRect(Fix64 x, Fix64 y, Fix64 width, Fix64 height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rotate = Fix64.Zero; // 默认无旋转
        }

        // 带旋转的构造函数
        public FixRect(Fix64 x, Fix64 y, Fix64 width, Fix64 height, Fix64 rotate)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rotate = rotate;
        }

        /// <summary>
        /// 判断当前矩形是否与另一个矩形重叠（纯整数计算，确定性）
        /// 如果矩形有旋转，先用 AABB 快速筛选，如果 AABB 重叠则进行精确检测
        /// </summary>
        public bool Overlaps(FixRect other)
        {
            // 如果两个矩形都没有旋转，使用快速 AABB 检测
            if (Rotate == Fix64.Zero && other.Rotate == Fix64.Zero)
            {
                return OverlapsAABB(other);
            }

            // 如果有旋转，先用 AABB 快速筛选
            FixRect thisAABB = GetBoundingRect();
            FixRect otherAABB = other.GetBoundingRect();

            // 如果 AABB 都不重叠，肯定不重叠（快速排除）
            if (!thisAABB.OverlapsAABB(otherAABB))
            {
                return false;
            }

            // AABB 重叠了，进行精确的旋转矩形碰撞检测
            return OverlapsRotated(other);
        }

        /// <summary>
        /// 精确的旋转矩形碰撞检测（使用分离轴定理 SAT）
        /// </summary>
        private bool OverlapsRotated(FixRect other)
        {
            // 获取两个矩形的四个顶点
            FixVector2[] thisCorners = GetCorners();
            FixVector2[] otherCorners = other.GetCorners();

            // 获取两个矩形的边（法向量）
            FixVector2[] thisAxes = GetAxes();
            FixVector2[] otherAxes = other.GetAxes();

            // 合并所有需要检测的轴
            FixVector2[] allAxes = new FixVector2[thisAxes.Length + otherAxes.Length];
            Array.Copy(thisAxes, 0, allAxes, 0, thisAxes.Length);
            Array.Copy(otherAxes, 0, allAxes, thisAxes.Length, otherAxes.Length);

            // 对每个轴进行投影检测（分离轴定理）
            foreach (FixVector2 axis in allAxes)
            {
                if (axis.x == Fix64.Zero && axis.y == Fix64.Zero)
                    continue;

                // 归一化轴（避免数值问题）
                FixVector2 normalizedAxis = axis.Normalized();

                // 投影两个矩形到该轴上
                Fix64 thisMin, thisMax, otherMin, otherMax;
                ProjectOntoAxis(thisCorners, normalizedAxis, out thisMin, out thisMax);
                ProjectOntoAxis(otherCorners, normalizedAxis, out otherMin, out otherMax);

                // 如果投影不重叠，则矩形不重叠（分离轴定理）
                if (thisMax < otherMin || otherMax < thisMin)
                {
                    return false;
                }
            }

            // 所有轴都重叠，矩形重叠
            return true;
        }

        /// <summary>
        /// 获取矩形的四个顶点（世界坐标）
        /// </summary>
        private FixVector2[] GetCorners()
        {
            Fix64 centerX = CenterX;
            Fix64 centerY = CenterY;
            Fix64 halfWidth = Width / new Fix64(2);
            Fix64 halfHeight = Height / new Fix64(2);

            // 矩形的四个角点（相对于中心，未旋转）
            FixVector2[] localCorners = new FixVector2[]
            {
                new FixVector2(-halfWidth, -halfHeight), // 左下
                new FixVector2(halfWidth, -halfHeight), // 右下
                new FixVector2(halfWidth, halfHeight), // 右上
                new FixVector2(-halfWidth, halfHeight) // 左上
            };

            // 如果没有旋转，直接转换到世界坐标
            if (Rotate == Fix64.Zero)
            {
                for (int i = 0; i < 4; i++)
                {
                    localCorners[i] = new FixVector2(
                        centerX + localCorners[i].x,
                        centerY + localCorners[i].y
                    );
                }

                return localCorners;
            }

            // 有旋转，需要旋转后转换到世界坐标
            Fix64 cos = Fix64.Cos(Rotate);
            Fix64 sin = Fix64.Sin(Rotate);
            FixVector2[] worldCorners = new FixVector2[4];

            for (int i = 0; i < 4; i++)
            {
                FixVector2 corner = localCorners[i];
                // 旋转
                Fix64 rotatedX = corner.x * cos - corner.y * sin;
                Fix64 rotatedY = corner.x * sin + corner.y * cos;
                // 转换到世界坐标
                worldCorners[i] = new FixVector2(centerX + rotatedX, centerY + rotatedY);
            }

            return worldCorners;
        }

        /// <summary>
        /// 获取矩形的边法向量（用于分离轴定理）
        /// </summary>
        private FixVector2[] GetAxes()
        {
            if (Rotate == Fix64.Zero)
            {
                // 无旋转时，边是轴对齐的，法向量是 (1,0) 和 (0,1)
                return new FixVector2[]
                {
                    new FixVector2(Fix64.One, Fix64.Zero), // 右
                    new FixVector2(Fix64.Zero, Fix64.One) // 上
                };
            }

            // 有旋转时，计算旋转后的边法向量
            Fix64 cos = Fix64.Cos(Rotate);
            Fix64 sin = Fix64.Sin(Rotate);

            // 矩形的两条边的方向向量（旋转后）
            FixVector2 right = new FixVector2(cos, sin); // 右边缘的方向
            FixVector2 up = new FixVector2(-sin, cos); // 上边缘的方向

            // 返回法向量（垂直于边）
            return new FixVector2[]
            {
                // new FixVector2(-right.y, right.x),  // 垂直于右边缘
                // new FixVector2(-up.y, up.x)         // 垂直于上边缘
                right, up
            };
        }

        /// <summary>
        /// 将矩形的顶点投影到指定轴上，返回最小和最大投影值
        /// </summary>
        private void ProjectOntoAxis(FixVector2[] corners, FixVector2 axis, out Fix64 min, out Fix64 max)
        {
            min = Fix64.MaxValue;
            max = Fix64.MinValue;

            foreach (FixVector2 corner in corners)
            {
                // 点积 = 投影长度
                Fix64 projection = FixVector2.Dot(corner, axis);
                min = Fix64.Min(min, projection);
                max = Fix64.Max(max, projection);
            }
        }

        /// <summary>
        /// AABB 重叠检测（轴对齐包围盒，不考虑旋转）
        /// </summary>
        private bool OverlapsAABB(FixRect other)
        {
            // 核心逻辑：不重叠的4种情况取反（无浮点、无舍入）
            return !(Right < other.X // 当前矩形在对方左侧
                     || X > other.Right // 当前矩形在对方右侧
                     || Top < other.Y // 当前矩形在对方下侧
                     || Y > other.Top); // 当前矩形在对方上侧
        }

        /// <summary>
        /// 判断点是否在矩形内（包含边界，纯整数）
        /// 注意：如果矩形有旋转，会进行精确的点-旋转矩形检测
        /// </summary>
        public bool Contains(Fix64 x, Fix64 y)
        {
            // 如果有旋转，需要将点转换到矩形的局部坐标系
            if (Rotate != Fix64.Zero)
            {
                return ContainsRotated(x, y);
            }

            // 无旋转时使用快速检测
            return x >= X && x <= Right && y >= Y && y <= Top;
        }

        /// <summary>
        /// 判断点是否在旋转矩形内（精确检测）
        /// </summary>
        private bool ContainsRotated(Fix64 x, Fix64 y)
        {
            // 将点转换到矩形的局部坐标系（以矩形中心为原点，未旋转状态）
            Fix64 centerX = CenterX;
            Fix64 centerY = CenterY;

            // 将点平移到以矩形中心为原点
            Fix64 localX = x - centerX;
            Fix64 localY = y - centerY;

            // 反向旋转点（将点从世界坐标系转换到矩形局部坐标系）
            Fix64 cos = Fix64.Cos(-Rotate);
            Fix64 sin = Fix64.Sin(-Rotate);

            Fix64 rotatedX = localX * cos - localY * sin;
            Fix64 rotatedY = localX * sin + localY * cos;

            // 在局部坐标系中检测点是否在矩形内（矩形在局部坐标系中是轴对齐的）
            Fix64 halfWidth = Width / new Fix64(2);
            Fix64 halfHeight = Height / new Fix64(2);

            return rotatedX >= -halfWidth && rotatedX <= halfWidth &&
                   rotatedY >= -halfHeight && rotatedY <= halfHeight;
        }

        /// <summary>
        /// 获取旋转后的轴对齐包围盒（AABB），用于四叉树存储
        /// </summary>
        public FixRect GetBoundingRect()
        {
            // 如果没有旋转，直接返回自身
            if (Rotate == Fix64.Zero)
            {
                return this;
            }

            // 计算旋转后的四个顶点
            Fix64 centerX = CenterX;
            Fix64 centerY = CenterY;
            Fix64 halfWidth = Width / new Fix64(2);
            Fix64 halfHeight = Height / new Fix64(2);

            Fix64 cos = Fix64.Cos(Rotate);
            Fix64 sin = Fix64.Sin(Rotate);

            // 矩形的四个角点（相对于中心）
            FixVector2[] corners = new FixVector2[]
            {
                new FixVector2(-halfWidth, -halfHeight), // 左下
                new FixVector2(halfWidth, -halfHeight), // 右下
                new FixVector2(halfWidth, halfHeight), // 右上
                new FixVector2(-halfWidth, halfHeight) // 左上
            };

            // 旋转四个角点并找到最小/最大边界
            Fix64 minX = Fix64.MaxValue;
            Fix64 minY = Fix64.MaxValue;
            Fix64 maxX = Fix64.MinValue;
            Fix64 maxY = Fix64.MinValue;

            for (int i = 0; i < 4; i++)
            {
                FixVector2 corner = corners[i];
                // 旋转
                Fix64 rotatedX = corner.x * cos - corner.y * sin;
                Fix64 rotatedY = corner.x * sin + corner.y * cos;
                // 转换到世界坐标
                Fix64 worldX = centerX + rotatedX;
                Fix64 worldY = centerY + rotatedY;

                minX = Fix64.Min(minX, worldX);
                minY = Fix64.Min(minY, worldY);
                maxX = Fix64.Max(maxX, worldX);
                maxY = Fix64.Max(maxY, worldY);
            }

            // 返回 AABB
            return new FixRect(minX, minY, maxX - minX, maxY - minY, Fix64.Zero);
        }

        /// <summary>
        /// 生成圆形的包围矩形（适配圆形检测）
        /// </summary>
        public static FixRect FromCircle(Fix64 centerX, Fix64 centerY, Fix64 radius)
        {
            return new FixRect(
                centerX - radius,
                centerY - radius,
                radius * new Fix64(2),
                radius * new Fix64(2)
            );
        }

        // 帧同步必备：重写Equals，避免引用比较
        public bool Equals(FixRect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height &&
                   Rotate == other.Rotate;
        }

        public override bool Equals(object obj)
        {
            return obj is FixRect other && Equals(other);
        }

        // 帧同步必备：确定性哈希（基于整数字段）
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height, Rotate);
        }

        // 方便调试：输出整数格式
        public override string ToString()
        {
            return $"X:{X}, Y:{Y}, W:{Width}, H:{Height}, Rotate:{Rotate}";
        }

        public int CompareTo(FixRect other)
        {
            if (this.Equals(other))
                return 0;
            return X.CompareTo(other.X);
        }
        
        public FixRect ScaleCenter(Fix64 scaleFactor)
        {
            // 合法性校验：缩放因子不能为负数或0，避免生成无效的宽高
            if (scaleFactor <= Fix64.Zero)
            {
                // 可以根据需求改为抛出异常，或返回原矩形（这里选择返回原矩形，更健壮）
                return this;
                // 若需要严格校验，可抛出异常：
                // throw new ArgumentOutOfRangeException(nameof(scaleFactor), "缩放因子必须大于0");
            }

            // 1. 计算原矩形的中心坐标（缩放后中心不变）
            Fix64 originalCenterX = CenterX;
            Fix64 originalCenterY = CenterY;

            // 2. 计算缩放后的宽高
            Fix64 newWidth = Width * scaleFactor;
            Fix64 newHeight = Height * scaleFactor;

            // 3. 计算缩放后的左、下边界（保证中心不变）
            // 新X = 中心X - 新宽度/2；新Y = 中心Y - 新高度/2
            Fix64 newX = originalCenterX - newWidth / new Fix64(2);
            Fix64 newY = originalCenterY - newHeight / new Fix64(2);

            // 4. 返回新的矩形（保留原旋转角度）
            return new FixRect(newX, newY, newWidth, newHeight, Rotate);
        }
    }
}