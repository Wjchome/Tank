using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPoolManager : SingletonMono<ObjectPoolManager>
{
    [System.Serializable]
    public class PoolInfo
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("对象池配置")]
    public PoolInfo[] poolInfos;
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;
    private Dictionary<string, Transform> poolParentDictionary;

    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();
        poolParentDictionary = new Dictionary<string, Transform>();

        foreach (PoolInfo poolInfo in poolInfos)
        {
            CreatePool(poolInfo.tag, poolInfo.prefab, poolInfo.size);
        }
    }

    private void CreatePool(string tag, GameObject prefab, int size)
    {
        // 创建父物体
        GameObject poolParent = new GameObject($"Pool_{tag}");
        poolParent.transform.SetParent(transform);
        poolParentDictionary[tag] = poolParent.transform;

        // 创建对象池
        Queue<GameObject> objectPool = new Queue<GameObject>();
        prefabDictionary[tag] = prefab;

        for (int i = 0; i < size; i++)
        {
            GameObject obj = CreateNewObject(tag, prefab);
            objectPool.Enqueue(obj);
        }

        poolDictionary[tag] = objectPool;
    }

    private GameObject CreateNewObject(string tag, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(poolParentDictionary[tag]);
        obj.SetActive(false);
        
        // 添加池化组件
        PooledObject pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = obj.AddComponent<PooledObject>();
        }
        pooledObject.Initialize(tag);
        
        return obj;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = null;

        // 如果池中有可用对象，直接使用
        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }
        else
        {
            // 池中没有可用对象，创建新的
            objectToSpawn = CreateNewObject(tag, prefabDictionary[tag]);
        }

        // 设置位置和旋转
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // 调用OnSpawnFromPool事件
        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        poolable?.OnSpawnFromPool();

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        PooledObject pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            Debug.LogWarning("Tried to return object to pool that doesn't have PooledObject component.");
            return;
        }

        string tag = pooledObject.PoolTag;
        
        // 调用OnReturnToPool事件
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();

        // 重置对象状态
        obj.SetActive(false);
        obj.transform.SetParent(poolParentDictionary[tag]);
        
        // 放回池中
        poolDictionary[tag].Enqueue(obj);
    }

    public void ReturnToPool(GameObject obj, float delay)
    {
        StartCoroutine(ReturnToPoolDelayed(obj, delay));
    }

    private System.Collections.IEnumerator ReturnToPoolDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(obj);
    }

    // 清空所有池
    public void ClearAllPools()
    {
        foreach (var kvp in poolDictionary)
        {
            Queue<GameObject> pool = kvp.Value;
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
        }
        poolDictionary.Clear();
    }

    // 获取池中对象数量
    public int GetPoolSize(string tag)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            return poolDictionary[tag].Count;
        }
        return 0;
    }
}

// 池化对象组件
public class PooledObject : MonoBehaviour
{
    public string PoolTag { get; private set; }

    public void Initialize(string tag)
    {
        PoolTag = tag;
    }
}

// 池化对象接口
public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
} 