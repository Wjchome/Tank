using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FixMath.NET;
using UnityEngine;

namespace QuadTreeV2
{
    /// <summary>
    /// 确定性碰撞检测测试
    /// 通过多次运行相同的输入，验证碰撞检测结果是否一致（帧同步关键）
    /// </summary>
    public class DeterministicCollisionTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("测试运行次数")]
        public int testRuns = 10;
        
        [Tooltip("每帧测试的物体数量")]
        public int objectCount = 20;
        
        [Tooltip("测试帧数")]
        public int testFrames = 100;
        
        [Tooltip("是否在每次运行后重置")]
        public bool resetBetweenRuns = true;
        
        [Tooltip("使用协程运行（较慢但不会卡顿）或立即运行（快速但可能卡顿）")]
        public bool useCoroutine = false;

        [Header("测试结果")]
        [Tooltip("是否显示详细日志")]
        public bool showDetailedLog = true;

        private List<string> _runResults = new List<string>();
        private bool _isRunning = false;
        private int _currentRun = 0;
        private int _currentFrame = 0;
        private List<TestObject> _testObjects = new List<TestObject>();
        private Dictionary<GameObject, TestObject> _gameObjectToTestObject = new Dictionary<GameObject, TestObject>();
        private List<GameObject> _testGameObjects = new List<GameObject>();
        private StringBuilder _currentRunLog = new StringBuilder();

        private class TestObject
        {
            public int Id;
            public FixRect Rect;
            public FixCircle? Circle;
            public FixVector2 Velocity;
            public GameObject GameObject; // 关联的 GameObject
        }

        private void Start()
        {
            // 不自动开始，需要手动触发
        }

        public void StartTest()
        {
            if (_isRunning)
            {
                Debug.LogWarning("测试已在运行中");
                return;
            }

            _isRunning = true;
            _currentRun = 0;
            _runResults.Clear();
            Debug.Log("========== 开始确定性碰撞检测测试 ==========");
            Debug.Log($"测试配置: {testRuns} 次运行, {objectCount} 个物体, {testFrames} 帧");
            
            if (useCoroutine)
            {
                StartCoroutine(RunTestsCoroutine());
            }
            else
            {
                RunTestsImmediate();
            }
        }

        private void RunTestsImmediate()
        {
            for (int run = 0; run < testRuns; run++)
            {
                _currentRun = run + 1;
                _currentFrame = 0;
                _currentRunLog.Clear();
                
                if (showDetailedLog)
                {
                    Debug.Log($"\n--- 第 {_currentRun}/{testRuns} 次运行 ---");
                }

                // 初始化测试物体（使用确定性随机种子）
                InitializeTestObjects(_currentRun);

                // 运行所有测试帧
                for (int frame = 0; frame < testFrames; frame++)
                {
                    _currentFrame = frame + 1;

                    // 更新物体位置（确定性更新）
                    foreach (var obj in _testObjects)
                    {
                        obj.Rect = new FixRect(
                            obj.Rect.X + obj.Velocity.x,
                            obj.Rect.Y + obj.Velocity.y,
                            obj.Rect.Width,
                            obj.Rect.Height,
                            obj.Rect.Rotate
                        );
                    }

                    // 执行碰撞检测（这是我们要测试的核心）
                    PerformCollisionDetection();
                }

                // 本次运行完成，记录结果
                string runResult = _currentRunLog.ToString();
                _runResults.Add(runResult);
                
                if (showDetailedLog)
                {
                    Debug.Log($"第 {_currentRun} 次运行完成，结果哈希: {runResult.GetHashCode()}");
                }
            }

            // 所有测试完成，分析结果
            AnalyzeResults();
            
            // 清理测试物体
            CleanupTestObjects();
            
            _isRunning = false;
        }

        private System.Collections.IEnumerator RunTestsCoroutine()
        {
            for (int run = 0; run < testRuns; run++)
            {
                _currentRun = run + 1;
                _currentFrame = 0;
                _currentRunLog.Clear();
                
                if (showDetailedLog)
                {
                    Debug.Log($"\n--- 第 {_currentRun}/{testRuns} 次运行 ---");
                }

                // 初始化测试物体（使用确定性随机种子）
                InitializeTestObjects(_currentRun);

                // 运行所有测试帧
                for (int frame = 0; frame < testFrames; frame++)
                {
                    _currentFrame = frame + 1;

                    // 更新物体位置（确定性更新）
                    foreach (var obj in _testObjects)
                    {
                        obj.Rect = new FixRect(
                            obj.Rect.X + obj.Velocity.x,
                            obj.Rect.Y + obj.Velocity.y,
                            obj.Rect.Width,
                            obj.Rect.Height,
                            obj.Rect.Rotate
                        );
                    }

                    // 执行碰撞检测（这是我们要测试的核心）
                    PerformCollisionDetection();
                }

                // 本次运行完成，记录结果
                string runResult = _currentRunLog.ToString();
                _runResults.Add(runResult);
                
                if (showDetailedLog)
                {
                    Debug.Log($"第 {_currentRun} 次运行完成，结果哈希: {runResult.GetHashCode()}");
                }

                // 每帧只运行一次测试，避免阻塞
                yield return null;
            }

            // 所有测试完成，分析结果
            AnalyzeResults();
            
            // 清理测试物体
            CleanupTestObjects();
            
            _isRunning = false;
        }

        private void InitializeTestObjects(int seed)
        {
            // 清理之前的测试物体
            CleanupTestObjects();
            
            _testObjects.Clear();
            _gameObjectToTestObject.Clear();
            _testGameObjects.Clear();
            
            // 使用确定性种子初始化物体
            for (int i = 0; i < objectCount; i++)
            {
                // 使用种子和索引生成确定性位置
                Fix64 x = (Fix64)(seed * 100 + i * 10);
                Fix64 y = (Fix64)(seed * 50 + i * 5);
                Fix64 width = (Fix64)2;
                Fix64 height = (Fix64)2;
                Fix64 rotate = (Fix64)0; // 可以添加旋转测试

                // 创建 GameObject
                GameObject testGameObject = new GameObject($"TestObject_{i}");
                testGameObject.transform.position = new Vector3((float)x, (float)y, 0);
                
                var testObj = new TestObject
                {
                    Id = i,
                    Rect = new FixRect(x, y, width, height, rotate),
                    Circle = null,
                    Velocity = new FixVector2(
                        (Fix64)((i % 3 - 1) * 0.1m), // 确定性速度
                        (Fix64)((i % 2) * 0.1m)
                    ),
                    GameObject = testGameObject
                };

                _testObjects.Add(testObj);
                _testGameObjects.Add(testGameObject);
                _gameObjectToTestObject[testGameObject] = testObj;
                
                // 添加到四叉树
                if (QuadTreeV2.ins != null)
                {
                    QuadTreeV2.ins.AddObjectToQuadTree(testObj.Rect, testGameObject);
                }
            }
        }

        private void CleanupTestObjects()
        {
            // 从四叉树移除所有测试物体
            if (QuadTreeV2.ins != null)
            {
                foreach (var go in _testGameObjects)
                {
                    if (go != null)
                    {
                        QuadTreeV2.ins.RemoveObjectFromQuadTree(go);
                        DestroyImmediate(go);
                    }
                }
            }
            
            _testGameObjects.Clear();
            _gameObjectToTestObject.Clear();
        }

        private void PerformCollisionDetection()
        {
            if (QuadTreeV2.ins == null)
            {
                Debug.LogError("QuadTreeV2.ins 未初始化！");
                return;
            }

            // 使用四叉树优化碰撞检测
            // 对每个物体，查询其周围区域内的其他物体，然后检测碰撞
            HashSet<(int, int)> detectedPairs = new HashSet<(int, int)>(); // 避免重复检测

            for (int i = 0; i < _testObjects.Count; i++)
            {
                TestObject obj1 = _testObjects[i];
                
                // 使用四叉树查询该物体周围的其他物体
                List<GameObject> nearbyObjects = QuadTreeV2.ins.GetObjectsInArea(obj1.Rect);
                
                // 检测与查询到的物体的碰撞
                foreach (var nearbyGameObject in nearbyObjects)
                {
                    // 通过映射找到对应的 TestObject
                    if (!_gameObjectToTestObject.TryGetValue(nearbyGameObject, out TestObject obj2))
                    {
                        continue;
                    }
                    
                    // 跳过自己
                    if (obj1.Id == obj2.Id)
                    {
                        continue;
                    }
                    
                    // 避免重复检测（只检测 id1 < id2 的情况）
                    int id1 = obj1.Id;
                    int id2 = obj2.Id;
                    if (id1 > id2)
                    {
                        (id1, id2) = (id2, id1);
                    }
                    
                    var pair = (id1, id2);
                    if (detectedPairs.Contains(pair))
                    {
                        continue;
                    }
                    detectedPairs.Add(pair);

                    bool isColliding = false;

                    // 执行精确碰撞检测
                    if (obj1.Circle.HasValue && obj2.Circle.HasValue)
                    {
                        isColliding = obj1.Circle.Value.Overlaps(obj2.Circle.Value);
                    }
                    else if (obj1.Circle.HasValue)
                    {
                        isColliding = obj1.Circle.Value.Overlaps(obj2.Rect);
                    }
                    else if (obj2.Circle.HasValue)
                    {
                        isColliding = obj2.Circle.Value.Overlaps(obj1.Rect);
                    }
                    else
                    {
                        isColliding = obj1.Rect.Overlaps(obj2.Rect);
                    }

                    // 记录碰撞结果（确定性格式）
                    if (isColliding)
                    {
                        _currentRunLog.Append($"F{_currentFrame}:O{id1}-O{id2};");
                    }
                }
            }
        }

        private void AnalyzeResults()
        {
            Debug.Log("\n========== 确定性测试结果分析 ==========");

            if (_runResults.Count == 0)
            {
                Debug.LogError("没有测试结果！");
                return;
            }

            // 检查所有结果是否相同
            bool allSame = true;
            string firstResult = _runResults[0];
            int firstHash = firstResult.GetHashCode();

            for (int i = 1; i < _runResults.Count; i++)
            {
                if (_runResults[i] != firstResult)
                {
                    allSame = false;
                    Debug.LogError($"❌ 发现不一致！第 1 次运行与第 {i + 1} 次运行结果不同");
                    
                    // 找出差异
                    FindDifferences(firstResult, _runResults[i], i + 1);
                    break;
                }
            }

            if (allSame)
            {
                Debug.Log($"✅ 所有 {_runResults.Count} 次运行结果完全一致！");
                Debug.Log($"结果哈希值: {firstHash}");
                Debug.Log($"结果长度: {firstResult.Length} 字符");
                
                if (showDetailedLog && firstResult.Length < 1000)
                {
                    Debug.Log($"结果内容: {firstResult}");
                }
            }
            else
            {
                Debug.LogError("❌ 测试失败：发现非确定性行为！");
                Debug.LogError("这表示碰撞检测存在非确定性因素，不适合帧同步！");
            }

            // 统计信息
            Debug.Log($"\n统计信息:");
            Debug.Log($"- 测试运行次数: {_runResults.Count}");
            Debug.Log($"- 每次运行帧数: {testFrames}");
            Debug.Log($"- 每次运行物体数: {objectCount}");
            Debug.Log($"- 总碰撞检测次数: {_runResults.Count * testFrames * objectCount * (objectCount - 1) / 2}");
        }

        private void FindDifferences(string result1, string result2, int runIndex)
        {
            Debug.LogError($"\n--- 差异分析 (运行 1 vs 运行 {runIndex}) ---");
            
            // 简单的差异分析
            string[] collisions1 = result1.Split(';');
            string[] collisions2 = result2.Split(';');

            HashSet<string> set1 = new HashSet<string>(collisions1);
            HashSet<string> set2 = new HashSet<string>(collisions2);

            // 找出只在结果1中的碰撞
            var onlyIn1 = set1.Except(set2).ToList();
            if (onlyIn1.Count > 0)
            {
                Debug.LogError($"只在运行 1 中检测到的碰撞 ({onlyIn1.Count} 个):");
                foreach (var collision in onlyIn1.Take(10)) // 只显示前10个
                {
                    if (!string.IsNullOrEmpty(collision))
                        Debug.LogError($"  - {collision}");
                }
            }

            // 找出只在结果2中的碰撞
            var onlyIn2 = set2.Except(set1).ToList();
            if (onlyIn2.Count > 0)
            {
                Debug.LogError($"只在运行 {runIndex} 中检测到的碰撞 ({onlyIn2.Count} 个):");
                foreach (var collision in onlyIn2.Take(10)) // 只显示前10个
                {
                    if (!string.IsNullOrEmpty(collision))
                        Debug.LogError($"  - {collision}");
                }
            }
        }

        // 手动触发测试（用于编辑器）
        [ContextMenu("运行确定性测试")]
        private void RunTest()
        {
            StartTest();
        }
    }
}

