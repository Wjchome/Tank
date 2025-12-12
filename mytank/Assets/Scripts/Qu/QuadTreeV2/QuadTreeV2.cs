using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FixMath.NET;


namespace QuadTreeV2
{
// 四叉树存储的物体封装（包含位置、尺寸、原始物体引用）
    public class QuadTreeObject : IComparable<QuadTreeObject>
    {
        public FixRect Bounds; // 物体的矩形边界（位置+尺寸）
        public FixCircle? Circle; // 物体的圆形边界（可选，如果为null则使用矩形）
        public GameObject Target; // 对应的Unity物体
        public int DeterministicId; // 确定性ID（用于帧同步去重和排序）

        // 矩形物体构造函数
        public QuadTreeObject(FixRect bounds, GameObject target)
        {
            Bounds = bounds; // 存储原始 bounds（包含旋转信息）
            Circle = null;
            Target = target;
            // 生成确定性ID
            DeterministicId = DeterministicIdGenerator.GetOrGenerateId(target);
        }

        // 圆形物体构造函数
        public QuadTreeObject(FixCircle circle, GameObject target)
        {
            Circle = circle;
            Bounds = circle.GetBoundingRect(); // 使用包围矩形用于四叉树存储
            Target = target;
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
            if (other == null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            // 如果是同一个Target，直接返回0（相同物体）
            if (this.Target == other.Target) return 0;
            // 基于确定性ID比较（确定性，适合帧同步）
            return this.DeterministicId.CompareTo(other.DeterministicId);
        }
    }

// 四叉树核心类
    public class QuadTreeV2 : MonoBehaviour
    {
        public static QuadTreeV2 ins;

        [Header("四叉树配置")] [Tooltip("根节点覆盖的区域（整个地图范围）")]
        public Rect Bounds = Rect.zero;
        [Tooltip("每个节点最大存储物体数（超过则分裂）")] public int MaxObjectsPerNode = 4;
        [Tooltip("最大递归深度（防止无限分裂）")] public int MaxDepth = 5; //4^5 *4

        public FixRect RootRect;
        private QuadTreeNode _rootNode; // 四叉树根节点


        private void Awake()
        {
            ins = this;
            RootRect = new FixRect((Fix64)Bounds.x, (Fix64)(Bounds.y), (Fix64)(Bounds.width), (Fix64)(Bounds.height));
            DeterministicIdGenerator.Initialize();

            // 初始化根节点
            _rootNode = new QuadTreeNode(RootRect, 0, MaxObjectsPerNode, MaxDepth);
        }


        // 对外接口：添加物体到四叉树（矩形）
        public void AddObjectToQuadTree(FixRect bounds, GameObject target)
        {
            var quadObj = new QuadTreeObject(bounds, target);
            _rootNode.Add(quadObj);
        }

        // 对外接口：添加圆形物体到四叉树
        public void AddObjectToQuadTree(FixCircle circle, GameObject target)
        {
            var quadObj = new QuadTreeObject(circle, target);
            _rootNode.Add(quadObj);
        }

        public void RemoveObjectFromQuadTree(GameObject target)
        {
            _rootNode.Remove(target);
            // 清理ID映射（可选，如果对象被销毁）
            // DeterministicIdGenerator.RemoveId(target);
        }

        public void UpdateObjectPosition(GameObject target, FixRect newBounds)
        {
            // 1. 先移除旧位置的物体
            RemoveObjectFromQuadTree(target);
            // 2. 再添加新位置的物体
            AddObjectToQuadTree(newBounds, target);
        }

        // 对外接口：查询指定区域内的所有物体（矩形）
        public List<GameObject> GetObjectsInArea(FixRect area)
        {
            var result = new List<QuadTreeObject>();
            _rootNode.GetObjectsInArea(area, result);
            // 转换为GameObject列表返回
            return result.DistinctBy((q) => q).ConvertAll(obj => obj.Target);
        }

        // 对外接口：查询指定圆形区域内的所有物体
        public List<GameObject> GetObjectsInCircle(FixCircle circle)
        {
            Debug.Log("query circle " + circle.ToString());
            var result = new List<QuadTreeObject>();
            _rootNode.GetObjectsInCircle(circle, result);
            // 转换为GameObject列表返回，去重（基于确定性ID）
            return result.DistinctBy((q) => q).ConvertAll(obj => obj.Target);
        }

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
            LeftUp = new QuadTreeNode(new FixRect(x, y + halfHeight, halfWidth, halfHeight), _currentDepth + 1, _maxObjects,
                _maxDepth);
            RightUp = new QuadTreeNode(new FixRect(x + halfWidth, y + halfHeight, halfWidth, halfHeight), _currentDepth + 1,
                _maxObjects, _maxDepth);
            LeftDown = new QuadTreeNode(new FixRect(x, y, halfWidth, halfHeight), _currentDepth + 1, _maxObjects, _maxDepth);
            RightDown = new QuadTreeNode(new FixRect(x + halfWidth, y, halfWidth, halfHeight), _currentDepth + 1, _maxObjects,
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

        // 查询指定区域内的所有物体
        public void GetObjectsInArea(FixRect area, List<QuadTreeObject> result)
        {
            // 1. 如果当前节点与查询区域无重叠，直接返回
            if (!Rect.Overlaps(area)) return;

            // 2. 未分裂则检查当前节点的物体
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    if (obj.OverlapsRect(area))
                    {
                        result.Add(obj);
                    }
                }

                return;
            }

            // 3. 已分裂则递归查询子节点
            LeftUp.GetObjectsInArea(area, result);
            RightUp.GetObjectsInArea(area, result);
            LeftDown.GetObjectsInArea(area, result);
            RightDown.GetObjectsInArea(area, result);
        }

        // 查询指定圆形区域内的所有物体
        public void GetObjectsInCircle(FixCircle circle, List<QuadTreeObject> result)
        {
            // 1. 如果当前节点与查询圆形无重叠，直接返回
            if (!circle.Overlaps(Rect)) return;

            // 2. 未分裂则检查当前节点的物体
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    if (obj.OverlapsCircle(circle))
                    {
                        result.Add(obj);
                    }
                }

                return;
            }

            // 3. 已分裂则递归查询子节点
            LeftUp.GetObjectsInCircle(circle, result);
            RightUp.GetObjectsInCircle(circle, result);
            LeftDown.GetObjectsInCircle(circle, result);
            RightDown.GetObjectsInCircle(circle, result);
        }

        public bool Remove(GameObject target)
        {
            // 1. 未分裂节点：直接从当前列表移除
            if (!IsSplit)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Target == target)
                    {
                        _objects.RemoveAt(i);
                        return true; // 找到并移除，返回成功
                    }
                }

                return false; // 未找到，返回失败
            }

            // 2. 已分裂节点：递归查询子节点（必须检查所有子节点，因为物体可能存储在多个节点中）
            bool removed = LeftUp.Remove(target) | RightUp.Remove(target) | LeftDown.Remove(target) | RightDown.Remove(target);

            if (removed)
            {
                TryMergeNodes();
            }

            return removed;
        }

        private void TryMergeNodes()
        {
            if (!IsSplit) return; // 未分裂无需合并
            if (LeftUp.IsSplit || RightUp.IsSplit || LeftDown.IsSplit || RightDown.IsSplit)
            {
                //还有细分 无法合并
                return;
            }

            List<QuadTreeObject> uniqueChildObjects = new List<QuadTreeObject>();


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

            uniqueChildObjects = uniqueChildObjects.DistinctBy((q) => q);
            // 步骤2：统计唯一物体数（真实数量，无重复）
            int totalUniqueObjects = uniqueChildObjects.Count;

            // 步骤3：只有唯一物体数 ≤ 最大物体数，才合并
            if (totalUniqueObjects <= _maxObjects)
            {
                _objects = uniqueChildObjects;

                // 销毁所有子节点（恢复未分裂状态）
                LeftUp = RightUp = LeftDown = RightDown = null;
            }
        }
    }
}