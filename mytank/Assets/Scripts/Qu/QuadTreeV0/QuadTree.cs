using System;
using System.Collections.Generic;
using UnityEngine;


namespace QuadTreeV0
{


// 四叉树存储的物体封装（包含位置、尺寸、原始物体引用）
    public class QuadTreeObject
    {
        public Rect Bounds; // 物体的矩形边界（位置+尺寸）
        public GameObject Target; // 对应的Unity物体

        public QuadTreeObject(Rect bounds, GameObject target)
        {
            Bounds = bounds;
            Target = target;
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

        // 重写GetHashCode，配合Equals使用
        public override int GetHashCode()
        {
            return Target.GetInstanceID();
        }
    }

// 四叉树核心类
    public class QuadTree : MonoBehaviour
    {
        public static QuadTree ins;

        [Header("四叉树配置")] [Tooltip("根节点覆盖的区域（整个地图范围）")]
        public Rect RootRect = new Rect(0, 0, 100, 100);

        [Tooltip("每个节点最大存储物体数（超过则分裂）")] public int MaxObjectsPerNode = 4;
        [Tooltip("最大递归深度（防止无限分裂）")] public int MaxDepth = 5; //4^5 *4

        private QuadTreeNode _rootNode; // 四叉树根节点

        // private List<QuadTreeObject> _objects = new List<QuadTreeObject>();

        private void Awake()
        {
            ins = this;
        }

        private void Start()
        {
            // 初始化根节点
            _rootNode = new QuadTreeNode(RootRect, 0, MaxObjectsPerNode, MaxDepth);
        }

        // 对外接口：添加物体到四叉树
        public void AddObjectToQuadTree(Rect bounds, GameObject target)
        {
            var quadObj = new QuadTreeObject(bounds, target);
            _rootNode.Add(quadObj);
            // _objects.Add(quadObj);
        }

        public void RemoveObjectFromQuadTree(GameObject target)
        {
            _rootNode.Remove(target);
            //_objects.Remove(target);

        }

        public void UpdateObjectPosition(GameObject target, Rect newBounds)
        {
            // 1. 先移除旧位置的物体
            RemoveObjectFromQuadTree(target);
            // 2. 再添加新位置的物体
            AddObjectToQuadTree(newBounds, target);
        }

        // 对外接口：查询指定区域内的所有物体
        public List<GameObject> GetObjectsInArea(Rect area)
        {
            var result = new List<QuadTreeObject>();
            _rootNode.GetObjectsInArea(area, result);
            // 转换为GameObject列表返回
            return result.ConvertAll(obj => obj.Target);
        }

        // 【调试用】在Scene视图绘制四叉树区域（红色=当前节点，蓝色=子节点）
        private void OnDrawGizmos()
        {
            if (_rootNode == null) return;
            //DrawObject();
            DrawNodeGizmos(_rootNode);
        }

        // private void DrawObject()
        // {
        //     foreach (var obj in _objects)
        //     {
        //         Rect targetRect = obj.Bounds;
        //         Vector3 center = new Vector3(
        //             targetRect.x + targetRect.width / 2, // Rect中心X
        //             targetRect.y + targetRect.height / 2, // Rect中心Y
        //             0
        //         );
        //         Vector3 size = new Vector3(targetRect.width, targetRect.height, 0.1f);
        //         Gizmos.color = Color.blue;
        //         Gizmos.DrawWireCube(center, size);
        //     }
        // }

        private void DrawNodeGizmos(QuadTreeNode node)
        {
            // 绘制当前节点的矩形边界
            Gizmos.color = node.IsSplit ? Color.blue : Color.red;
            Vector2[] vector2s = new Vector2[4]
            {
                new Vector2(node.Rect.x, node.Rect.y),
                new Vector2(node.Rect.x + node.Rect.width, node.Rect.y),
                new Vector2(node.Rect.x + node.Rect.width, node.Rect.y + node.Rect.height),
                new Vector2(node.Rect.x, node.Rect.y + node.Rect.height),
            };
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(vector2s[i], vector2s[(i + 1) % 4]);
            }

            // 递归绘制子节点
            if (node.NW != null) DrawNodeGizmos(node.NW);
            if (node.NE != null) DrawNodeGizmos(node.NE);
            if (node.SW != null) DrawNodeGizmos(node.SW);
            if (node.SE != null) DrawNodeGizmos(node.SE);
        }
    }

// 四叉树节点类（内部逻辑，不对外暴露）
    public class QuadTreeNode
    {
        public Rect Rect; // 当前节点的矩形区域
        private int _currentDepth; // 当前节点深度
        private int _maxObjects; // 节点最大存储物体数
        private int _maxDepth; // 最大递归深度

        private List<QuadTreeObject> _objects; // 当前节点存储的物体

        // 四个子节点（NW=左上，NE=右上，SW=左下，SE=右下）
        public QuadTreeNode NW, NE, SW, SE;
        public bool IsSplit => NW != null; // 是否已分裂为子节点

        public QuadTreeNode(Rect rect, int currentDepth, int maxObjects, int maxDepth)
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
            float halfWidth = Rect.width / 2;
            float halfHeight = Rect.height / 2;
            float x = Rect.x;
            float y = Rect.y;

            // 创建四个子节点（左上、右上、左下、右下）
            NW = new QuadTreeNode(new Rect(x, y + halfHeight, halfWidth, halfHeight), _currentDepth + 1, _maxObjects,
                _maxDepth);
            NE = new QuadTreeNode(new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight), _currentDepth + 1,
                _maxObjects, _maxDepth);
            SW = new QuadTreeNode(new Rect(x, y, halfWidth, halfHeight), _currentDepth + 1, _maxObjects, _maxDepth);
            SE = new QuadTreeNode(new Rect(x + halfWidth, y, halfWidth, halfHeight), _currentDepth + 1, _maxObjects,
                _maxDepth);
        }

        // 将物体添加到子节点（仅添加到与物体重叠的子节点）
        private void AddToChildNodes(QuadTreeObject obj)
        {
            if (NW.Rect.Overlaps(obj.Bounds)) NW.Add(obj);
            if (NE.Rect.Overlaps(obj.Bounds)) NE.Add(obj);
            if (SW.Rect.Overlaps(obj.Bounds)) SW.Add(obj);
            if (SE.Rect.Overlaps(obj.Bounds)) SE.Add(obj);
        }

        // 查询指定区域内的所有物体
        public void GetObjectsInArea(Rect area, List<QuadTreeObject> result)
        {
            // 1. 如果当前节点与查询区域无重叠，直接返回
            if (!Rect.Overlaps(area)) return;

            // 2. 未分裂则检查当前节点的物体
            if (!IsSplit)
            {
                foreach (var obj in _objects)
                {
                    if (obj.Bounds.Overlaps(area))
                    {
                        result.Add(obj);
                    }
                }

                return;
            }

            // 3. 已分裂则递归查询子节点
            NW.GetObjectsInArea(area, result);
            NE.GetObjectsInArea(area, result);
            SW.GetObjectsInArea(area, result);
            SE.GetObjectsInArea(area, result);
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

            // 2. 已分裂节点：递归查询子节点
            bool removed = NW.Remove(target) | NE.Remove(target) | SW.Remove(target) | SE.Remove(target);

            if (removed)
            {
                TryMergeNodes();
            }

            return removed;
        }

        private void TryMergeNodes()
        {
            if (!IsSplit) return; // 未分裂无需合并

            // 步骤1：用HashSet收集唯一物体（自动去重，依赖QuadTreeObject的Equals/GetHashCode）
            HashSet<QuadTreeObject> uniqueChildObjects = new HashSet<QuadTreeObject>();

            // 遍历所有子节点，将物体加入HashSet（自动去重）
            if (NW != null)
            {
                foreach (var obj in NW._objects)
                {
                    uniqueChildObjects.Add(obj);
                }
            }

            if (NE != null)
            {
                foreach (var obj in NE._objects)
                {
                    uniqueChildObjects.Add(obj);
                }
            }

            if (SW != null)
            {
                foreach (var obj in SW._objects)
                {
                    uniqueChildObjects.Add(obj);
                }
            }

            if (SE != null)
            {
                foreach (var obj in SE._objects)
                {
                    uniqueChildObjects.Add(obj);
                }
            }

            // 步骤2：统计唯一物体数（真实数量，无重复）
            int totalUniqueObjects = uniqueChildObjects.Count;

            // 步骤3：只有唯一物体数 ≤ 最大物体数，才合并
            if (totalUniqueObjects <= _maxObjects)
            {
                // 核心：清空当前节点objects，添加去重后的唯一物体
                _objects.Clear();
                _objects.AddRange(uniqueChildObjects);

                // 销毁所有子节点（恢复未分裂状态）
                NW = NE = SW = SE = null;

                // 调试日志：打印去重后的真实数量
                Debug.Log(
                    $"节点合并完成，去重后物体数：{_objects.Count}（原重复统计数：{NW?._objects.Count ?? 0 + NE?._objects.Count ?? 0 + SW?._objects.Count ?? 0 + SE?._objects.Count ?? 0}）");
            }
        }
    }
}