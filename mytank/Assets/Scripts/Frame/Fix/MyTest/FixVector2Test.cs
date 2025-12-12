using System;
using UnityEngine;

namespace FixMath.NET
{
    /// <summary>
    /// FixVector2 测试套件
    /// </summary>
    public class FixVector2Test : MonoBehaviour
    {
        private void Start()
        {
            RunAllTests();
        }

        private void RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("FixVector2 全面测试套件");
            Debug.Log("========================================\n");

            TestBasicOperations();
            TestVectorMath();
            TestNormalization();
            TestDistance();
            TestDotAndCross();
            TestLerp();
            TestClampMagnitude();
            TestEquality();
            TestTypeConversions();
            TestDeterminism();

            Debug.Log("\n========================================");
            Debug.Log("所有测试完成！");
            Debug.Log("========================================");
        }

        #region 1. 基本运算测试

        private void TestBasicOperations()
        {
            Debug.Log("========== 1. 基本运算测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 b = new FixVector2((Fix64)1, (Fix64)2);

            // 加法
            TestCase("加法: (3,4) + (1,2) = (4,6)", () =>
            {
                FixVector2 result = a + b;
                return result.x == (Fix64)4 && result.y == (Fix64)6;
            }, ref passed, ref failed);

            // 减法
            TestCase("减法: (3,4) - (1,2) = (2,2)", () =>
            {
                FixVector2 result = a - b;
                return result.x == (Fix64)2 && result.y == (Fix64)2;
            }, ref passed, ref failed);

            // 取反
            TestCase("取反: -(3,4) = (-3,-4)", () =>
            {
                FixVector2 result = -a;
                return result.x == (Fix64)(-3) && result.y == (Fix64)(-4);
            }, ref passed, ref failed);

            // 标量乘法（右乘）
            TestCase("标量乘法: (3,4) * 2 = (6,8)", () =>
            {
                FixVector2 result = a * (Fix64)2;
                return result.x == (Fix64)6 && result.y == (Fix64)8;
            }, ref passed, ref failed);

            // 标量乘法（左乘）
            TestCase("标量乘法: 2 * (3,4) = (6,8)", () =>
            {
                FixVector2 result = (Fix64)2 * a;
                return result.x == (Fix64)6 && result.y == (Fix64)8;
            }, ref passed, ref failed);

            // 标量除法
            TestCase("标量除法: (6,8) / 2 = (3,4)", () =>
            {
                FixVector2 vec = new FixVector2((Fix64)6, (Fix64)8);
                FixVector2 result = vec / (Fix64)2;
                return Math.Abs((double)result.x - 3.0) < 1e-6 && Math.Abs((double)result.y - 4.0) < 1e-6;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 2. 向量数学测试

        private void TestVectorMath()
        {
            Debug.Log("========== 2. 向量数学测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 b = new FixVector2((Fix64)1, (Fix64)2);

            // SqrMagnitude
            TestCase("SqrMagnitude: (3,4) = 25", () =>
            {
                Fix64 result = a.SqrMagnitude();
                return Math.Abs((double)result - 25.0) < 1e-6;
            }, ref passed, ref failed);

            // Magnitude
            TestCase("Magnitude: (3,4) = 5", () =>
            {
                Fix64 result = a.Magnitude();
                return Math.Abs((double)result - 5.0) < 1e-5;
            }, ref passed, ref failed);

            // Zero 向量
            TestCase("Zero 向量: Magnitude = 0", () =>
            {
                return FixVector2.Zero.Magnitude() == Fix64.Zero;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 3. 归一化测试

        private void TestNormalization()
        {
            Debug.Log("========== 3. 归一化测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);

            // Normalized（不修改原向量）
            TestCase("Normalized: (3,4) 归一化后长度 = 1", () =>
            {
                FixVector2 normalized = a.Normalized();
                Fix64 magnitude = normalized.Magnitude();
                return Math.Abs((double)magnitude - 1.0) < 1e-5;
            }, ref passed, ref failed);

            // Normalize（修改原向量）
            TestCase("Normalize: 归一化后长度 = 1", () =>
            {
                FixVector2 vec = new FixVector2((Fix64)3, (Fix64)4);
                vec.Normalize();
                Fix64 magnitude = vec.Magnitude();
                return Math.Abs((double)magnitude - 1.0) < 1e-5;
            }, ref passed, ref failed);

            // 零向量归一化
            TestCase("零向量归一化: 返回零向量", () =>
            {
                FixVector2 zero = FixVector2.Zero;
                FixVector2 normalized = zero.Normalized();
                return normalized == FixVector2.Zero;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 4. 距离测试

        private void TestDistance()
        {
            Debug.Log("========== 4. 距离测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)0, (Fix64)0);
            FixVector2 b = new FixVector2((Fix64)3, (Fix64)4);

            // SqrDistance
            TestCase("SqrDistance: (0,0) 到 (3,4) = 25", () =>
            {
                Fix64 result = FixVector2.SqrDistance(a, b);
                return Math.Abs((double)result - 25.0) < 1e-6;
            }, ref passed, ref failed);

            // Distance
            TestCase("Distance: (0,0) 到 (3,4) = 5", () =>
            {
                Fix64 result = FixVector2.Distance(a, b);
                return Math.Abs((double)result - 5.0) < 1e-5;
            }, ref passed, ref failed);

            // 相同点距离为 0
            TestCase("相同点距离 = 0", () =>
            {
                return FixVector2.Distance(a, a) == Fix64.Zero;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 5. 点积和叉积测试

        private void TestDotAndCross()
        {
            Debug.Log("========== 5. 点积和叉积测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 b = new FixVector2((Fix64)1, (Fix64)2);

            // Dot 点积
            TestCase("Dot: (3,4) · (1,2) = 11", () =>
            {
                Fix64 result = FixVector2.Dot(a, b);
                return Math.Abs((double)result - 11.0) < 1e-6;
            }, ref passed, ref failed);

            // Dot 与 Unity Vector2 对比
            TestCase("Dot 与 Unity Vector2 对比", () =>
            {
                Vector2 ua = new Vector2(3, 4);
                Vector2 ub = new Vector2(1, 2);
                float expected = Vector2.Dot(ua, ub);
                Fix64 actual = FixVector2.Dot(a, b);
                return Math.Abs((double)actual - expected) < 1e-5;
            }, ref passed, ref failed);

            // Cross 叉积
            TestCase("Cross: (3,4) × (1,2) = 2", () =>
            {
                Fix64 result = FixVector2.Cross(a, b);
                return Math.Abs((double)result - 2.0) < 1e-6;
            }, ref passed, ref failed);

            // 垂直向量点积为 0
            TestCase("垂直向量点积 = 0", () =>
            {
                FixVector2 up = FixVector2.Up;
                FixVector2 right = FixVector2.Right;
                Fix64 result = FixVector2.Dot(up, right);
                return Math.Abs((double)result) < 1e-6;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 6. 线性插值测试

        private void TestLerp()
        {
            Debug.Log("========== 6. 线性插值测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)0, (Fix64)0);
            FixVector2 b = new FixVector2((Fix64)10, (Fix64)10);

            // Lerp t=0
            TestCase("Lerp: t=0 返回起点", () =>
            {
                FixVector2 result = FixVector2.Lerp(a, b, Fix64.Zero);
                return result == a;
            }, ref passed, ref failed);

            // Lerp t=1
            TestCase("Lerp: t=1 返回终点", () =>
            {
                FixVector2 result = FixVector2.Lerp(a, b, Fix64.One);
                return result == b;
            }, ref passed, ref failed);

            // Lerp t=0.5
            TestCase("Lerp: t=0.5 返回中点", () =>
            {
                FixVector2 result = FixVector2.Lerp(a, b, (Fix64)0.5m);
                FixVector2 expected = new FixVector2((Fix64)5, (Fix64)5);
                return FixVector2.Distance(result, expected) < (Fix64)0.01m;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 7. 限制长度测试

        private void TestClampMagnitude()
        {
            Debug.Log("========== 7. 限制长度测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 longVec = new FixVector2((Fix64)10, (Fix64)10);

            // ClampMagnitude 限制长度
            TestCase("ClampMagnitude: 限制长度为 5", () =>
            {
                FixVector2 result = FixVector2.ClampMagnitude(longVec, (Fix64)5);
                Fix64 magnitude = result.Magnitude();
                return magnitude <= (Fix64)5.01m && magnitude >= (Fix64)4.99m;
            }, ref passed, ref failed);

            // ClampMagnitude 不改变短向量
            TestCase("ClampMagnitude: 短向量不变", () =>
            {
                FixVector2 shortVec = new FixVector2((Fix64)1, (Fix64)1);
                FixVector2 result = FixVector2.ClampMagnitude(shortVec, (Fix64)5);
                return result == shortVec;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 8. 相等性测试

        private void TestEquality()
        {
            Debug.Log("========== 8. 相等性测试 ==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 b = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 c = new FixVector2((Fix64)1, (Fix64)2);

            TestCase("相等: (3,4) == (3,4)", () => a == b, ref passed, ref failed);
            TestCase("不等: (3,4) != (1,2)", () => a != c, ref passed, ref failed);
            TestCase("Equals 方法", () => a.Equals(b), ref passed, ref failed);
            TestCase("GetHashCode 一致性", () => a.GetHashCode() == b.GetHashCode(), ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 9. 类型转换测试

        private void TestTypeConversions()
        {
            Debug.Log("========== 9. 类型转换测试 ==========");
            int passed = 0;
            int failed = 0;

            // FixVector2 -> Vector2
            TestCase("FixVector2 -> Vector2", () =>
            {
                FixVector2 fixVec = new FixVector2((Fix64)3.5m, (Fix64)4.7m);
                Vector2 unityVec = (Vector2)fixVec;
                return Math.Abs(unityVec.x - 3.5f) < 0.01f && Math.Abs(unityVec.y - 4.7f) < 0.01f;
            }, ref passed, ref failed);

            // Vector2 -> FixVector2
            TestCase("Vector2 -> FixVector2", () =>
            {
                Vector2 unityVec = new Vector2(3.5f, 4.7f);
                FixVector2 fixVec = (FixVector2)unityVec;
                return Math.Abs((double)fixVec.x - 3.5) < 0.01 && Math.Abs((double)fixVec.y - 4.7) < 0.01;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 10. 确定性测试（帧同步关键）

        private void TestDeterminism()
        {
            Debug.Log("========== 10. 确定性测试（帧同步关键）==========");
            int passed = 0;
            int failed = 0;

            FixVector2 a = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 b = new FixVector2((Fix64)1, (Fix64)2);

            // 多次计算应该得到相同结果
            FixVector2 result1 = a + b;
            FixVector2 result2 = a + b;
            FixVector2 result3 = a + b;

            TestCase("确定性: 相同输入产生相同输出", () => result1 == result2 && result2 == result3, ref passed, ref failed);

            // 归一化确定性
            FixVector2 vec = new FixVector2((Fix64)3, (Fix64)4);
            FixVector2 norm1 = vec.Normalized();
            FixVector2 norm2 = vec.Normalized();
            TestCase("确定性: 归一化一致性", () => norm1 == norm2, ref passed, ref failed);

            // 点积确定性
            Fix64 dot1 = FixVector2.Dot(a, b);
            Fix64 dot2 = FixVector2.Dot(a, b);
            TestCase("确定性: 点积一致性", () => dot1 == dot2, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 辅助方法

        private void TestCase(string description, Func<bool> test, ref int passed, ref int failed)
        {
            try
            {
                bool result = test();
                if (result)
                {
                    passed++;
                    Debug.Log($"✓ {description}");
                }
                else
                {
                    failed++;
                    Debug.LogError($"✗ {description}");
                }
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"✗ {description} - 异常: {ex.Message}");
            }
        }

        #endregion
    }
}

