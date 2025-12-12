using System.Collections.Generic;
using UnityEngine;

namespace QuadTreeV2
{
    /// <summary>
    /// 帧同步ID分配器（核心前提：所有客户端注册GameObject的顺序/时机完全一致）
    /// 极简设计：仅用自增整数，无哈希/无路径依赖，性能拉满
    /// </summary>
    public static class DeterministicIdGenerator
    {
        // 全局自增ID（初始种子可配置，所有客户端必须用相同值）
        private static int _nextId;
        // GameObject → ID 映射（避免重复注册）
        private static readonly Dictionary<GameObject, int> _objectToId = new Dictionary<GameObject, int>();
        // ID → GameObject 反向映射（可选，方便根据ID查对象）
        private static readonly Dictionary<int, GameObject> _idToObject = new Dictionary<int, GameObject>();

        /// <summary>
        /// 初始化ID生成器（帧同步开始前，所有客户端必须调用且传入相同seed）
        /// </summary>
        /// <param name="initialSeed">初始种子（比如1000，避免从0开始）</param>
        public static void Initialize(int initialSeed = 1000)
        {
            _nextId = initialSeed;
            _objectToId.Clear();
            _idToObject.Clear();
        }

        /// <summary>
        /// 为GameObject分配ID（注册顺序一致则多端ID完全相同）
        /// </summary>
        public static int GetOrGenerateId(GameObject target)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            
            // 已注册则直接返回
            if (_objectToId.TryGetValue(target, out int id))
            {
                return id;
            }

            // 未注册则分配下一个自增ID（核心：依赖注册顺序一致）
            id = _nextId++;
            _objectToId[target] = id;
            _idToObject[id] = target;
            return id;
        }

        /// <summary>
        /// 根据ID获取GameObject（可选）
        /// </summary>
        public static GameObject GetObjectById(int id)
        {
            _idToObject.TryGetValue(id, out var obj);
            return obj;
        }

        /// <summary>
        /// 销毁对象时清理映射（避免内存泄漏）
        /// </summary>
        public static void RemoveId(GameObject target)
        {
            if (_objectToId.TryGetValue(target, out int id))
            {
                _objectToId.Remove(target);
                _idToObject.Remove(id);
            }
        }

    }
}