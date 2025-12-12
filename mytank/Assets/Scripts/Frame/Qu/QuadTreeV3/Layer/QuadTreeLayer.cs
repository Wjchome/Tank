using System;

namespace QuadTreeV3
{
    /// <summary>
    /// 四叉树层系统（用于碰撞过滤）
    /// 使用位掩码（BitMask）实现，类似 Unity 的 LayerMask
    /// 支持最多 32 层（使用 int 的 32 位）
    /// </summary>
    [Serializable]
    public struct QuadTreeLayer
    {
        /// <summary>
        /// 层位掩码（每个位代表一个层）
        /// </summary>
        public int value;

        public QuadTreeLayer(int layerMask)
        {
            value = layerMask;
        }

        /// <summary>
        /// 检查指定层是否在掩码中
        /// </summary>
        /// <param name="layer">层索引（0-31）</param>
        /// <returns>如果该层在掩码中返回 true</returns>
        public bool Contains(int layer)
        {
            if (layer < 0 || layer >= 32)
                return false;
            return (value & (1 << layer)) != 0;
        }

        /// <summary>
        /// 检查两个层掩码是否有交集
        /// </summary>
        public bool Intersects(QuadTreeLayer other)
        {
            return (value & other.value) != 0;
        }

        /// <summary>
        /// 添加层到掩码
        /// </summary>
        public void AddLayer(int layer)
        {
            if (layer >= 0 && layer < 32)
            {
                value |= (1 << layer);
            }
        }

        /// <summary>
        /// 从掩码中移除层
        /// </summary>
        public void RemoveLayer(int layer)
        {
            if (layer >= 0 && layer < 32)
            {
                value &= ~(1 << layer);
            }
        }

        /// <summary>
        /// 创建包含所有层的掩码
        /// </summary>
        public static QuadTreeLayer Everything => new QuadTreeLayer(-1);

        /// <summary>
        /// 创建不包含任何层的掩码
        /// </summary>
        public static QuadTreeLayer Nothing => new QuadTreeLayer(0);

        /// <summary>
        /// 创建只包含指定层的掩码
        /// </summary>
        public static QuadTreeLayer GetLayer(int layer)
        {
            if (layer >= 0 && layer < 32)
            {
                return new QuadTreeLayer(1 << layer);
            }
            return Nothing;
        }

        public static implicit operator int(QuadTreeLayer layer)
        {
            return layer.value;
        }

        public static implicit operator QuadTreeLayer(int layerMask)
        {
            return new QuadTreeLayer(layerMask);
        }

        public override string ToString()
        {
            return $"LayerMask: {value}";
        }
    }
}

