using System;
using System.Collections.Generic;
using System.Linq;

namespace QuadTreeV2
{
  
    /// <summary>
    /// List 极简去重拓展方法（帧同步友好）
    /// 核心：按元素唯一标识去重，去重后按标识升序排列（保证多端结果一致）
    /// </summary>
    public static class ListDistinctExtensions
    {
        /// <summary>
        /// 通用去重：按自定义唯一键去重（比如 ObjectID），去重后按键升序排列
        /// </summary>
        /// <typeparam name="T">List 元素类型</typeparam>
        /// <typeparam name="TKey">唯一键类型（如 int/string，必须可排序）</typeparam>
        /// <param name="list">要去重的列表（直接修改原列表）</param>
        /// <param name="getUniqueKey">提取元素唯一键的逻辑（比如 obj => obj.ObjectID）</param>
        public static List<T> DistinctBy<T, TKey>(this List<T> list, Func<T, TKey> getUniqueKey) 
            where TKey : IComparable<TKey>
        {
            
            if (list == null)
                return null;
            if(list.Count <= 1) 
                return list.ToList() ;
            List<T>  result = new List<T>(list);
            // 步骤1：按唯一键排序（保证相同元素相邻，且多端排序结果一致）
            result.Sort((a, b) => getUniqueKey(a).CompareTo(getUniqueKey(b)));

            // 步骤2：双指针就地去重（不创建新列表，性能最优）
            int writeIndex = 1;
            TKey lastKey = getUniqueKey(result[0]);
        
            for (int readIndex = 1; readIndex < result.Count; readIndex++)
            {
                TKey currentKey = getUniqueKey(result[readIndex]);
                // 仅当键不同时，保留当前元素
                if (currentKey.CompareTo(lastKey) != 0)
                {
                    result[writeIndex] = result[readIndex];
                    lastKey = currentKey;
                    writeIndex++;
                }
            }

            // 移除重复的尾部元素
            result.RemoveRange(writeIndex, result.Count - writeIndex);
            return result;
        }
    }
}