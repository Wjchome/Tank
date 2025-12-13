using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FixMath.NET;

namespace QuadTreeV3
{
    /// <summary>
    /// 四叉树空间分区系统（用于帧同步碰撞检测）
    /// 支持矩形和圆形碰撞体，支持旋转矩形，支持层过滤
    /// </summary>
    public class QuadTreeV3 : SingletonMono<QuadTreeV3>
    {
        
        [Header("四叉树配置")] 

        [Tooltip("每个节点最大存储物体数（超过则分裂）")] public int MaxObjectsPerNode = 4;

        [Tooltip("最大递归深度（防止无限分裂）")] public int MaxDepth = 5;

        /// <summary>
        /// 根节点的矩形区域（固定点）
        /// </summary>
        public FixRect RootRect { get; private set; }

        private QuadTreeNode _rootNode; // 四叉树根节点

        public void Init(FixRect rootRect)
        {
            RootRect = rootRect;
            DeterministicIdGenerator.Initialize();

            // 初始化根节点
            _rootNode = new QuadTreeNode(RootRect, 0, MaxObjectsPerNode, MaxDepth);
        }

  

        #region 添加物体

        /// <summary>
        /// 添加矩形物体到四叉树
        /// </summary>
        /// <param name="bounds">矩形边界</param>
        /// <param name="target">Unity物体</param>
        /// <param name="layer">物体所在的层（默认为第0层）</param>
        public void AddObject(FixRect bounds, GameObject target, QuadTreeLayer layer = default)
        {
            if (target == null)
            {
                Debug.LogWarning("尝试添加空物体到四叉树");
                return;
            }

            var quadObj = new QuadTreeObject(bounds, target, layer);
            _rootNode.Add(quadObj);
        }

        /// <summary>
        /// 添加圆形物体到四叉树
        /// </summary>
        /// <param name="circle">圆形边界</param>
        /// <param name="target">Unity物体</param>
        /// <param name="layer">物体所在的层（默认为第0层）</param>
        public void AddObject(FixCircle circle, GameObject target, QuadTreeLayer layer = default)
        {
            if (target == null)
            {
                Debug.LogWarning("尝试添加空物体到四叉树");
                return;
            }

            var quadObj = new QuadTreeObject(circle, target, layer);
            _rootNode.Add(quadObj);
        }

        #endregion

        #region 移除物体

        /// <summary>
        /// 从四叉树中移除物体
        /// </summary>
        /// <param name="target">要移除的Unity物体</param>
        public QuadTreeObject RemoveObject(GameObject target)
        {
            return _rootNode.Remove(target);
        }

        #endregion

        #region 更新物体位置

        /// <summary>
        /// 更新物体在四叉树中的位置
        /// </summary>
        /// <param name="target">Unity物体</param>
        /// <param name="newBounds">新的边界</param>
        public void UpdateObject(GameObject target, FixRect newBounds)
        {
            // 1. 先移除旧位置的物体
            var obj = RemoveObject(target);
            if (obj == null) return;
            // 2. 再添加新位置的物体（保持原有层信息）
            AddObject(newBounds, target, obj.Layer);
        }

        #endregion

        #region 查询物体

        /// <summary>
        /// 查询指定矩形区域内的所有物体
        /// </summary>
        /// <param name="area">查询区域</param>
        /// <param name="layerMask">层掩码（只返回匹配的层，默认返回所有层）</param>
        /// <returns>匹配的物体列表（已排序，确定性）</returns>
        public List<QuadTreeObject> Query(FixRect area, QuadTreeLayer layerMask = default)
        {
            layerMask = QuadTreeLayer.Everything;
            var result = new HashSet<QuadTreeObject>();
            _rootNode.GetObjectsInArea(area, result, layerMask);
            var list = result.ToList();
            list.Sort(); // 确定性排序
            return list;
        }

        /// <summary>
        /// 查询指定圆形区域内的所有物体
        /// </summary>
        /// <param name="circle">查询圆形区域</param>
        /// <param name="layerMask">层掩码（只返回匹配的层，默认返回所有层）</param>
        /// <returns>匹配的物体列表（已排序，确定性）</returns>
        public List<QuadTreeObject> Query(FixCircle circle, QuadTreeLayer layerMask = default)
        {
            var result = new HashSet<QuadTreeObject>();
            _rootNode.GetObjectsInCircle(circle, result, layerMask);
            var list = result.ToList();
            list.Sort(); // 确定性排序
            return list;
        }

        #endregion


        // 【调试用】在Scene视图绘制四叉树区域（红色=当前节点，蓝色=子节点）
        private void OnDrawGizmos()
        {
            if (_rootNode == null) return;
            DrawNodeGizmos(_rootNode);
        }


        private void DrawNodeGizmos(QuadTreeNode node)
        {
            // 绘制当前节点的矩形边界
            Gizmos.color = Color.red;
            Vector2[] vector2s = new Vector2[4]
            {
                new Vector2((float)node.Rect.X, (float)node.Rect.Y),
                new Vector2((float)node.Rect.X + (float)node.Rect.Width, (float)node.Rect.Y),
                new Vector2((float)node.Rect.X + (float)node.Rect.Width, (float)node.Rect.Y + (float)node.Rect.Height),
                new Vector2((float)node.Rect.X, (float)node.Rect.Y + (float)node.Rect.Height),
            };
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(vector2s[i],
                    vector2s[(i + 1) % 4]);
            }

            // 递归绘制子节点
            if (node.LeftUp != null) DrawNodeGizmos(node.LeftUp);
            if (node.RightUp != null) DrawNodeGizmos(node.RightUp);
            if (node.LeftDown != null) DrawNodeGizmos(node.LeftDown);
            if (node.RightDown != null) DrawNodeGizmos(node.RightDown);
        }
    }

// 四叉树节点类（内部逻辑，不对外暴露）
    public class QuadTreeNode
    {
        public FixRect Rect; // 当前节点的矩形区域
        private int _currentDepth; // 当前节点深度
        private int _maxObjects; // 节点最大存储物体数
        private int _maxDepth; // 最大递归深度

        private List<QuadTreeObject> _objects; // 当前节点存储的物体

        public QuadTreeNode LeftUp, RightUp, LeftDown, RightDown;
        public bool IsSplit => LeftUp != null; // 是否已分裂为子节点

        public QuadTreeNode(FixRect rect, int currentDepth, int maxObjects, int maxDepth)
        {
            Rect = rect;
            _currentDepth = currentDepth;
            _maxObjects = maxObjects;
            _maxDepth = maxDepth;
            _objects = new List<QuadTreeObject>();
        }

        // 添加物体到节点
        public void Add(QuadTreeObject obj)
        {
            // 1. 如果节点已分裂，尝试添加到子节点
            if (IsSplit)
            {
                AddToChildNodes(obj);
                return;
            }

            // 2. 未分裂则添加到当前节点
            _objects.Add(obj);

            // 3. 检查是否需要分裂（物体数超上限 + 未到最大深度）
            if (_objects.Count > _maxObjects && _currentDepth < _maxDepth)
            {
                Split(); // 分裂为4个子节点
                // 把当前节点的物体迁移到子节点
                foreach (var o in _objects)
                {
                    AddToChildNodes(o);
                }

                _objects.Clear(); // 清空当前节点的物体
            }
        }

        // 分裂为4个子节点
        private void Split()
        {
            Fix64 halfWidth = Rect.Width / new Fix64(2);
            Fix64 halfHeight = Rect.Height / new Fix64(2);
            Fix64 x = Rect.X;
            Fix64 y = Rect.Y;

            // 创建四个子节点（左上、右上、左下、右下）
            LeftUp = new QuadTreeNode(new FixRect(x, y + halfHeight, halfWidth, halfHeight), _currentDepth + 1,
                _maxObjects,
                _maxDepth);
            RightUp = new QuadTreeNode(new FixRect(x + halfWidth, y + halfHeight, halfWidth, halfHeight),
                _currentDepth + 1,
                _maxObjects, _maxDepth);
            LeftDown = new QuadTreeNode(new FixRect(x, y, halfWidth, halfHeight), _currentDepth + 1, _maxObjects,
                _maxDepth);
            RightDown = new QuadTreeNode(new FixRect(x + halfWidth, y, halfWidth, halfHeight), _currentDepth + 1,
                _maxObjects,
                _maxDepth);
        }

        // 将物体添加到子节点（仅添加到与物体重叠的子节点）
        private void AddToChildNodes(QuadTreeObject obj)
        {
            // 如果矩形有旋转，使用 AABB 进行快速筛选（提高四叉树效率）
            FixRect boundsForTree = obj.Bounds.Rotate != Fix64.Zero ? obj.Bounds.GetBoundingRect() : obj.Bounds;

            if (LeftUp.Rect.Overlaps(boundsForTree)) LeftUp.Add(obj);
            if (RightUp.Rect.Overlaps(boundsForTree)) RightUp.Add(obj);
            if (LeftDown.Rect.Overlaps(boundsForTree)) LeftDown.Add(obj);
            if (RightDown.Rect.Overlaps(boundsForTree)) RightDown.Add(obj);
        }

        /// <summary>
        /// 查询指定区域内的所有物体（支持层过滤）
        /// </summary>
        public void GetObjectsInArea(FixRect area, HashSet<QuadTreeObject> result, QuadTreeLayer layerMask = default)
        {
            // 1. 如果当前节点与查询区域无重叠，直接返回
            if (!Rect.Overlaps(area)) return;

            // 2. 未分裂则检查当前节点的物体
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    // 层过滤
                    if (layerMask.value != 0 && !layerMask.Intersects(obj.Layer))
                        continue;

                    if (obj.OverlapsRect(area))
                    {
                        result.Add(obj);
                    }
                }

                return;
            }

            // 3. 已分裂则递归查询子节点
            LeftUp.GetObjectsInArea(area, result, layerMask);
            RightUp.GetObjectsInArea(area, result, layerMask);
            LeftDown.GetObjectsInArea(area, result, layerMask);
            RightDown.GetObjectsInArea(area, result, layerMask);
        }

        /// <summary>
        /// 查询指定圆形区域内的所有物体（支持层过滤）
        /// </summary>
        public void GetObjectsInCircle(FixCircle circle, HashSet<QuadTreeObject> result,
            QuadTreeLayer layerMask = default)
        {
            // 1. 如果当前节点与查询圆形无重叠，直接返回
            if (!circle.Overlaps(Rect)) return;

            // 2. 未分裂则检查当前节点的物体
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    // 层过滤
                    if (layerMask.value != 0 && !layerMask.Intersects(obj.Layer))
                        continue;

                    if (obj.OverlapsCircle(circle))
                    {
                        result.Add(obj);
                    }
                }

                return;
            }

            // 3. 已分裂则递归查询子节点
            LeftUp.GetObjectsInCircle(circle, result, layerMask);
            RightUp.GetObjectsInCircle(circle, result, layerMask);
            LeftDown.GetObjectsInCircle(circle, result, layerMask);
            RightDown.GetObjectsInCircle(circle, result, layerMask);
        }

        /// <summary>
        /// 获取所有物体（用于调试）
        /// </summary>
        public void GetAllObjects(HashSet<QuadTreeObject> result)
        {
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    result.Add(obj);
                }

                return;
            }

            LeftUp.GetAllObjects(result);
            RightUp.GetAllObjects(result);
            LeftDown.GetAllObjects(result);
            RightDown.GetAllObjects(result);
        }

        public QuadTreeObject Remove(GameObject target)
        {
            // 1. 未分裂节点：直接从当前列表移除
            if (!IsSplit)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Target == target)
                    {
                        var removedObj = _objects[i]; // 先保存要移除的对象
                        _objects.RemoveAt(i); // 再移除
                        return removedObj; // 直接返回，避免索引越界
                    }
                }

                return null; // 未找到，返回失败
            }


            // 2. 已分裂节点：递归查询子节点（必须检查所有子节点，因为物体可能存储在多个节点中）
            QuadTreeObject removed = null;
            QuadTreeObject temp;

            temp = LeftUp.Remove(target);
            if (temp != null)
            {
                removed = temp;
            }

            temp = RightUp.Remove(target);
            if (temp != null)
            {
                removed = temp;
            }

            temp = LeftDown.Remove(target);
            if (temp != null)
            {
                removed = temp;
            }

            temp = RightDown.Remove(target);
            if (temp != null)
            {
                removed = temp;
            }

            if (removed != null)
            {
                TryMergeNodes();
            }

            return removed; // 返回第一个找到的对象（所有子节点中的对象层信息相同）
        }

        private void TryMergeNodes()
        {
            if (!IsSplit) return; // 未分裂无需合并
            if (LeftUp.IsSplit || RightUp.IsSplit || LeftDown.IsSplit || RightDown.IsSplit)
            {
                //还有细分 无法合并
                return;
            }

            HashSet<QuadTreeObject> uniqueChildObjects = new HashSet<QuadTreeObject>();


            foreach (var obj in LeftUp._objects)
            {
                uniqueChildObjects.Add(obj);
            }

            foreach (var obj in RightUp._objects)
            {
                uniqueChildObjects.Add(obj);
            }

            foreach (var obj in LeftDown._objects)
            {
                uniqueChildObjects.Add(obj);
            }

            foreach (var obj in RightDown._objects)
            {
                uniqueChildObjects.Add(obj);
            }

            // 步骤2：统计唯一物体数（真实数量，无重复）
            int totalUniqueObjects = uniqueChildObjects.Count;

            // 步骤3：只有唯一物体数 ≤ 最大物体数，才合并
            if (totalUniqueObjects <= _maxObjects)
            {
                var list = uniqueChildObjects.ToList();
                list.Sort();
                _objects = list;

                // 销毁所有子节点（恢复未分裂状态）
                LeftUp = RightUp = LeftDown = RightDown = null;
            }
        }
    }
}