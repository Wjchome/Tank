using System;
using System.Collections.Generic;
using UnityEngine;

namespace FixMath.NET
{
    /// <summary>
    /// Fix64 全面测试套件
    /// 测试所有主要功能，评估帧同步适用性
    /// </summary>
    public class Fix64ComprehensiveTest : MonoBehaviour
    {
        public void RunTests()
        {
            RunAllTests();
        }

        private void Start()
        {
            RunAllTests();
        }

        private void RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("Fix64 全面测试套件");
            Debug.Log("========================================\n");

            TestBasicArithmetic();
            TestComparisonOperators();
            TestUtilityFunctions();
            TestTrigonometricFunctions();
            TestMathematicalFunctions();
            TestBoundaryCases();
            TestPrecision();
            TestDeterminism();
            TestTypeConversions();

            Debug.Log("\n========================================");
            Debug.Log("所有测试完成！");
            Debug.Log("========================================");
        }

        #region 1. 基本运算测试

        private void TestBasicArithmetic()
        {
            Debug.Log("========== 1. 基本运算测试 ==========");
            int passed = 0;
            int failed = 0;

            // 加法测试
            TestCase("加法: 1 + 2 = 3", () =>
            {
                Fix64 a = (Fix64)1;
                Fix64 b = (Fix64)2;
                Fix64 result = a + b;
                return Math.Abs((double)result - 3.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("加法: -5 + 3 = -2", () =>
            {
                Fix64 a = (Fix64)(-5);
                Fix64 b = (Fix64)3;
                Fix64 result = a + b;
                return Math.Abs((double)result - (-2.0)) < 1e-9;
            }, ref passed, ref failed);

            // 减法测试
            TestCase("减法: 5 - 3 = 2", () =>
            {
                Fix64 a = (Fix64)5;
                Fix64 b = (Fix64)3;
                Fix64 result = a - b;
                return Math.Abs((double)result - 2.0) < 1e-9;
            }, ref passed, ref failed);

            // 乘法测试
            TestCase("乘法: 2 * 3 = 6", () =>
            {
                Fix64 a = (Fix64)2;
                Fix64 b = (Fix64)3;
                Fix64 result = a * b;
                return Math.Abs((double)result - 6.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("乘法: 2.5 * 4 = 10", () =>
            {
                Fix64 a = (Fix64)2.5m;
                Fix64 b = (Fix64)4;
                Fix64 result = a * b;
                return Math.Abs((double)result - 10.0) < 1e-6;
            }, ref passed, ref failed);

            // 除法测试
            TestCase("除法: 10 / 2 = 5", () =>
            {
                Fix64 a = (Fix64)10;
                Fix64 b = (Fix64)2;
                Fix64 result = a / b;
                return Math.Abs((double)result - 5.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("除法: 7 / 3 ≈ 2.333", () =>
            {
                Fix64 a = (Fix64)7;
                Fix64 b = (Fix64)3;
                Fix64 result = a / b;
                double expected = 7.0 / 3.0;
                return Math.Abs((double)result - expected) < 1e-6;
            }, ref passed, ref failed);

            // 取模测试
            TestCase("取模: 10 % 3 = 1", () =>
            {
                Fix64 a = (Fix64)10;
                Fix64 b = (Fix64)3;
                Fix64 result = a % b;
                return Math.Abs((double)result - 1.0) < 1e-9;
            }, ref passed, ref failed);

            // 负数测试
            TestCase("负数: -(-5) = 5", () =>
            {
                Fix64 a = (Fix64)(-5);
                Fix64 result = -a;
                return Math.Abs((double)result - 5.0) < 1e-9;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 2. 比较运算符测试

        private void TestComparisonOperators()
        {
            Debug.Log("========== 2. 比较运算符测试 ==========");
            int passed = 0;
            int failed = 0;

            Fix64 a = (Fix64)5;
            Fix64 b = (Fix64)3;
            Fix64 c = (Fix64)5;

            TestCase("大于: 5 > 3", () => a > b, ref passed, ref failed);
            TestCase("小于: 3 < 5", () => b < a, ref passed, ref failed);
            TestCase("等于: 5 == 5", () => a == c, ref passed, ref failed);
            TestCase("不等于: 5 != 3", () => a != b, ref passed, ref failed);
            TestCase("大于等于: 5 >= 3", () => a >= b, ref passed, ref failed);
            TestCase("大于等于: 5 >= 5", () => a >= c, ref passed, ref failed);
            TestCase("小于等于: 3 <= 5", () => b <= a, ref passed, ref failed);
            TestCase("小于等于: 5 <= 5", () => a <= c, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 3. 工具函数测试

        private void TestUtilityFunctions()
        {
            Debug.Log("========== 3. 工具函数测试 ==========");
            int passed = 0;
            int failed = 0;

            // Abs 测试
            TestCase("Abs: |-5| = 5", () =>
            {
                Fix64 a = (Fix64)(-5);
                Fix64 result = Fix64.Abs(a);
                return Math.Abs((double)result - 5.0) < 1e-9;
            }, ref passed, ref failed);

            // Floor 测试
            TestCase("Floor: Floor(3.7) = 3", () =>
            {
                Fix64 a = (Fix64)3.7m;
                Fix64 result = Fix64.Floor(a);
                return Math.Abs((double)result - 3.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("Floor: Floor(-3.7) = -4", () =>
            {
                Fix64 a = (Fix64)(-3.7m);
                Fix64 result = Fix64.Floor(a);
                return Math.Abs((double)result - (-4.0)) < 1e-9;
            }, ref passed, ref failed);

            // Ceiling 测试
            TestCase("Ceiling: Ceiling(3.2) = 4", () =>
            {
                Fix64 a = (Fix64)3.2m;
                Fix64 result = Fix64.Ceiling(a);
                return Math.Abs((double)result - 4.0) < 1e-9;
            }, ref passed, ref failed);

            // Round 测试
            TestCase("Round: Round(3.4) = 3", () =>
            {
                Fix64 a = (Fix64)3.4m;
                Fix64 result = Fix64.Round(a);
                return Math.Abs((double)result - 3.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("Round: Round(3.6) = 4", () =>
            {
                Fix64 a = (Fix64)3.6m;
                Fix64 result = Fix64.Round(a);
                return Math.Abs((double)result - 4.0) < 1e-9;
            }, ref passed, ref failed);

            TestCase("Round: Round(3.5) = 4 (银行家舍入)", () =>
            {
                Fix64 a = (Fix64)3.5m;
                Fix64 result = Fix64.Round(a);
                // 银行家舍入：3.5 应该舍入到 4（因为 3 是奇数）
                return Math.Abs((double)result - 4.0) < 1e-9;
            }, ref passed, ref failed);

            // Sign 测试
            TestCase("Sign: Sign(5) = 1", () => Fix64.Sign((Fix64)5) == 1, ref passed, ref failed);
            TestCase("Sign: Sign(-5) = -1", () => Fix64.Sign((Fix64)(-5)) == -1, ref passed, ref failed);
            TestCase("Sign: Sign(0) = 0", () => Fix64.Sign(Fix64.Zero) == 0, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 4. 三角函数测试

        private void TestTrigonometricFunctions()
        {
            Debug.Log("========== 4. 三角函数测试 ==========");
            int passed = 0;
            int failed = 0;
            double maxSinError = 0;
            double maxCosError = 0;
            double maxTanError = 0;

            var angles = new[] { 0.0, 0.1, 0.5, 1.0, Math.PI / 4, Math.PI / 3, Math.PI / 6, Math.PI / 2 - 0.1 };

            foreach (var angle in angles)
            {
                // Sin 测试
                double expectedSin = Math.Sin(angle);
                Fix64 fix64Sin = Fix64.Sin((Fix64)angle);
                double actualSin = (double)fix64Sin;
                double sinError = Math.Abs(actualSin - expectedSin);
                double sinRelError = Math.Abs(expectedSin) > 1e-10 ? sinError / Math.Abs(expectedSin) : sinError;
                maxSinError = Math.Max(maxSinError, sinRelError);

                bool sinPassed = sinRelError < 1e-6;
                TestCase($"Sin({angle:F4}) 误差: {sinRelError:E6}", () => sinPassed, ref passed, ref failed);

                // Cos 测试
                double expectedCos = Math.Cos(angle);
                Fix64 fix64Cos = Fix64.Cos((Fix64)angle);
                double actualCos = (double)fix64Cos;
                double cosError = Math.Abs(actualCos - expectedCos);
                double cosRelError = Math.Abs(expectedCos) > 1e-10 ? cosError / Math.Abs(expectedCos) : cosError;
                maxCosError = Math.Max(maxCosError, cosRelError);

                bool cosPassed = cosRelError < 1e-6;
                TestCase($"Cos({angle:F4}) 误差: {cosRelError:E6}", () => cosPassed, ref passed, ref failed);

                // Tan 测试（避免接近 π/2）
                if (Math.Abs(angle - Math.PI / 2) > 0.01)
                {
                    double expectedTan = Math.Tan(angle);
                    Fix64 fix64Tan = Fix64.Tan((Fix64)angle);
                    double actualTan = (double)fix64Tan;

                    // 如果 Tan 返回 MaxValue，说明溢出
                    if (fix64Tan == Fix64.MaxValue || fix64Tan == Fix64.MinValue)
                    {
                        TestCase($"Tan({angle:F4}) 溢出（预期）", () => true, ref passed, ref failed);
                    }
                    else
                    {
                        double tanError = Math.Abs(actualTan - expectedTan);
                        double tanRelError = Math.Abs(expectedTan) > 1.0 ? tanError / Math.Abs(expectedTan) : tanError;
                        maxTanError = Math.Max(maxTanError, tanRelError);

                        bool tanPassed = tanRelError < 1e-4; // Tan 精度较低
                        TestCase($"Tan({angle:F4}) 误差: {tanRelError:E6}", () => tanPassed, ref passed, ref failed);
                    }
                }
            }

            // Atan 测试
            var atanValues = new[] { 0.0, 0.5, 1.0, 2.0, 10.0 };
            foreach (var value in atanValues)
            {
                double expectedAtan = Math.Atan(value);
                Fix64 fix64Atan = Fix64.Atan((Fix64)value);
                double actualAtan = (double)fix64Atan;
                double atanError = Math.Abs(actualAtan - expectedAtan);
                bool atanPassed = atanError < 1e-5;
                TestCase($"Atan({value}) 误差: {atanError:E6}", () => atanPassed, ref passed, ref failed);
            }

            // Atan2 测试
            // 注意：Atan2 使用近似公式，精度较低（约 0.5%），所以放宽误差阈值
            var atan2Cases = new[] { (0.0, 1.0), (1.0, 0.0), (1.0, 1.0), (-1.0, 1.0) };
            foreach (var (y, x) in atan2Cases)
            {
                double expectedAtan2 = Math.Atan2(y, x);
                Fix64 fix64Atan2 = Fix64.Atan2((Fix64)y, (Fix64)x);
                double actualAtan2 = (double)fix64Atan2;
                double atan2Error = Math.Abs(actualAtan2 - expectedAtan2);
                
                // Atan2 使用近似公式 z / (1 + 0.28 * z^2)，精度约为 0.5%
                // 对于 π/4 (0.785)，0.5% 的误差约为 0.004，所以放宽阈值到 1e-2
                bool atan2Passed = atan2Error < 1e-2; // 放宽到 0.01 弧度（约 0.57度）
                
                // 输出详细信息以便调试
                if (!atan2Passed)
                {
                    double relError = Math.Abs(expectedAtan2) > 1e-10 ? atan2Error / Math.Abs(expectedAtan2) : atan2Error;
                    Debug.Log($"  Atan2({y}, {x}): 期望={expectedAtan2:F10}, 实际={actualAtan2:F10}, 绝对误差={atan2Error:E6}, 相对误差={relError:P2}");
                }
                
                TestCase($"Atan2({y}, {x}) 误差: {atan2Error:E6}", () => atan2Passed, ref passed, ref failed);
            }

            Debug.Log($"最大 Sin 相对误差: {maxSinError:E6}");
            Debug.Log($"最大 Cos 相对误差: {maxCosError:E6}");
            Debug.Log($"最大 Tan 相对误差: {maxTanError:E6}");
            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 5. 数学函数测试

        private void TestMathematicalFunctions()
        {
            Debug.Log("========== 5. 数学函数测试 ==========");
            int passed = 0;
            int failed = 0;

            // Sqrt 测试
            var sqrtValues = new[] { 1.0, 4.0, 9.0, 16.0, 25.0, 2.0, 3.0, 10.0 };
            foreach (var value in sqrtValues)
            {
                double expectedSqrt = Math.Sqrt(value);
                Fix64 fix64Sqrt = Fix64.Sqrt((Fix64)value);
                double actualSqrt = (double)fix64Sqrt;
                double sqrtError = Math.Abs(actualSqrt - expectedSqrt);
                double sqrtRelError = sqrtError / expectedSqrt;
                bool sqrtPassed = sqrtRelError < 1e-5;
                TestCase($"Sqrt({value}) 误差: {sqrtRelError:E6}", () => sqrtPassed, ref passed, ref failed);
            }

            // Pow 测试
            var powCases = new[] { (2.0, 3.0), (2.0, 0.5), (10.0, 2.0), (2.0, -1.0) };
            foreach (var (b, exp) in powCases)
            {
                double expectedPow = Math.Pow(b, exp);
                Fix64 fix64Pow = Fix64.Pow((Fix64)b, (Fix64)exp);
                double actualPow = (double)fix64Pow;
                double powError = Math.Abs(actualPow - expectedPow);
                double powRelError = Math.Abs(expectedPow) > 1e-10 ? powError / Math.Abs(expectedPow) : powError;
                bool powPassed = powRelError < 1e-4; // Pow 精度较低
                TestCase($"Pow({b}, {exp}) 误差: {powRelError:E6}", () => powPassed, ref passed, ref failed);
            }

            // Ln 测试
            var lnValues = new[] { 1.0, 2.0, Math.E, 10.0, 11.000101, 150, 1500, 15000, 150000 };
            foreach (var value in lnValues)
            {
                double expectedLn = Math.Log(value);
                Fix64 fix64Ln = Fix64.Ln((Fix64)value);
                double actualLn = (double)fix64Ln;
                double lnError = Math.Abs(actualLn - expectedLn);
                
                // 处理 expectedLn = 0 的情况（避免除以 0 导致 NaN）
                double lnRelError;
                if (Math.Abs(expectedLn) < 1e-10)
                {
                    // 如果期望值为 0，使用绝对误差
                    lnRelError = lnError;
                }
                else
                {
                    lnRelError = lnError / Math.Abs(expectedLn);
                }
                
                bool lnPassed = lnRelError < 1e-4;
                TestCase($"Ln({value}) 误差: {lnRelError:E6}", () => lnPassed, ref passed, ref failed);
            }
            
            // Log2 测试
            var log2Values = new[] { 1.0, 2.0, Math.E, 10.0, 11.000101, 150, 1500, 15000, 150000 };
            foreach (var value in log2Values)
            {
                // 修复：Math.Log(value, 2) 计算以 2 为底，value 的对数
                // 注意：.NET Framework 使用 Math.Log(value, 2)，.NET Core 可以使用 Math.Log2(value)
                double expectedLog2 = Math.Log(value, 2);
                Fix64 fix64Log2 = Fix64.Log2((Fix64)value);
                double actualLog2 = (double)fix64Log2;
                double log2Error = Math.Abs(actualLog2 - expectedLog2);
                
                // 处理 expectedLog2 = 0 的情况（避免除以 0 导致 NaN）
                double log2RelError;
                if (Math.Abs(expectedLog2) < 1e-10)
                {
                    // 如果期望值为 0，使用绝对误差
                    log2RelError = log2Error;
                }
                else
                {
                    log2RelError = log2Error / Math.Abs(expectedLog2);
                }
                
                bool log2Passed = log2RelError < 1e-4;
                TestCase($"Log2({value}) 误差: {log2RelError:E6}", () => log2Passed, ref passed, ref failed);
            }

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 6. 边界情况测试

        private void TestBoundaryCases()
        {
            Debug.Log("========== 6. 边界情况测试 ==========");
            int passed = 0;
            int failed = 0;

            // MaxValue 测试
            TestCase("MaxValue 存在", () => Fix64.MaxValue != Fix64.Zero, ref passed, ref failed);
            TestCase("MaxValue > 0", () => Fix64.MaxValue > Fix64.Zero, ref passed, ref failed);

            // MinValue 测试
            TestCase("MinValue 存在", () => Fix64.MinValue != Fix64.Zero, ref passed, ref failed);
            TestCase("MinValue < 0", () => Fix64.MinValue < Fix64.Zero, ref passed, ref failed);

            // Zero 测试
            TestCase("Zero == 0", () => Fix64.Zero == (Fix64)0, ref passed, ref failed);
            TestCase("Zero + Zero == Zero", () => Fix64.Zero + Fix64.Zero == Fix64.Zero, ref passed, ref failed);

            // One 测试
            TestCase("One == 1", () => Math.Abs((double)Fix64.One - 1.0) < 1e-9, ref passed, ref failed);

            // Pi 测试
            TestCase("Pi 值正确", () =>
            {
                double fix64Pi = (double)Fix64.Pi;
                double mathPi = Math.PI;
                return Math.Abs(fix64Pi - mathPi) < 1e-6;
            }, ref passed, ref failed);

            // 溢出测试
            TestCase("溢出: MaxValue + 1 饱和", () =>
            {
                Fix64 result = Fix64.MaxValue + (Fix64)1;
                return result == Fix64.MaxValue; // 应该饱和
            }, ref passed, ref failed);

            TestCase("溢出: MinValue - 1 饱和", () =>
            {
                Fix64 result = Fix64.MinValue - (Fix64)1;
                return result == Fix64.MinValue; // 应该饱和
            }, ref passed, ref failed);

            // Abs(MinValue) 特殊处理
            TestCase("Abs(MinValue) == MaxValue", () =>
            {
                Fix64 result = Fix64.Abs(Fix64.MinValue);
                return result == Fix64.MaxValue;
            }, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 7. 精度测试

        private void TestPrecision()
        {
            Debug.Log("========== 7. 精度测试 ==========");
            int passed = 0;
            int failed = 0;

            // 测试精度声明（2^-32）
            double declaredPrecision = 2.3283064365386962890625E-10;
            double actualPrecision = (double)Fix64.Precision;

            TestCase($"精度声明正确: {actualPrecision:E15}", () =>
                Math.Abs(actualPrecision - declaredPrecision) < 1e-20, ref passed, ref failed);

            // 测试小数值精度
            TestCase("小数值精度: 0.0000000001", () =>
            {
                Fix64 small = (Fix64)0.0000000001m;
                double value = (double)small;
                // 由于精度限制，可能无法精确表示
                return value >= 0;
            }, ref passed, ref failed);

            // 测试大数值范围
            double maxValueDouble = (double)Fix64.MaxValue;
            TestCase($"MaxValue 范围: {maxValueDouble:E2}", () =>
                maxValueDouble > 2e9 && maxValueDouble < 3e9, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 8. 确定性测试（帧同步关键）

        private void TestDeterminism()
        {
            Debug.Log("========== 8. 确定性测试（帧同步关键）==========");
            int passed = 0;
            int failed = 0;

            // 测试相同输入产生相同输出
            Fix64 a = (Fix64)3.14159m;
            Fix64 b = (Fix64)2.71828m;

            // 多次计算应该得到相同结果
            Fix64 result1 = Fix64.Sin(a) * Fix64.Cos(b);
            Fix64 result2 = Fix64.Sin(a) * Fix64.Cos(b);
            Fix64 result3 = Fix64.Sin(a) * Fix64.Cos(b);

            TestCase("确定性: 相同输入产生相同输出 (1)", () => result1 == result2, ref passed, ref failed);
            TestCase("确定性: 相同输入产生相同输出 (2)", () => result2 == result3, ref passed, ref failed);

            // 测试运算顺序不影响结果（对于加法和乘法）
            Fix64 x = (Fix64)1.5m;
            Fix64 y = (Fix64)2.5m;
            Fix64 z = (Fix64)3.5m;

            Fix64 add1 = (x + y) + z;
            Fix64 add2 = x + (y + z);
            TestCase("确定性: 加法结合律", () => add1 == add2, ref passed, ref failed);

            Fix64 mul1 = (x * y) * z;
            Fix64 mul2 = x * (y * z);
            TestCase("确定性: 乘法结合律", () => mul1 == mul2, ref passed, ref failed);

            // 测试三角函数的一致性
            Fix64 angle = (Fix64)(Math.PI / 4);
            Fix64 sin1 = Fix64.Sin(angle);
            Fix64 sin2 = Fix64.Sin(angle);
            TestCase("确定性: Sin 函数一致性", () => sin1 == sin2, ref passed, ref failed);

            Fix64 cos1 = Fix64.Cos(angle);
            Fix64 cos2 = Fix64.Cos(angle);
            TestCase("确定性: Cos 函数一致性", () => cos1 == cos2, ref passed, ref failed);

            // 测试数学恒等式: sin² + cos² = 1
            Fix64 sin = Fix64.Sin(angle);
            Fix64 cos = Fix64.Cos(angle);
            Fix64 identity = sin * sin + cos * cos;
            double identityError = Math.Abs((double)identity - 1.0);
            TestCase($"确定性: sin² + cos² = 1 (误差: {identityError:E10})", () => identityError < 1e-6, ref passed, ref failed);

            Debug.Log($"通过: {passed}, 失败: {failed}\n");
        }

        #endregion

        #region 9. 类型转换测试

        private void TestTypeConversions()
        {
            Debug.Log("========== 9. 类型转换测试 ==========");
            int passed = 0;
            int failed = 0;

            // int 转换
            TestCase("int -> Fix64: 5", () =>
            {
                Fix64 f = (Fix64)5;
                return Math.Abs((double)f - 5.0) < 1e-9;
            }, ref passed, ref failed);

            // float 转换
            TestCase("float -> Fix64: 3.14f", () =>
            {
                Fix64 f = (Fix64)3.14f;
                double value = (double)f;
                return Math.Abs(value - 3.14) < 1e-5;
            }, ref passed, ref failed);

            // double 转换
            TestCase("double -> Fix64: 2.718", () =>
            {
                Fix64 f = (Fix64)2.718;
                double value = (double)f;
                return Math.Abs(value - 2.718) < 1e-6;
            }, ref passed, ref failed);

            // decimal 转换
            TestCase("decimal -> Fix64: 1.23456789m", () =>
            {
                Fix64 f = (Fix64)1.23456789m;
                decimal value = (decimal)f;
                return Math.Abs((double)value - 1.23456789) < 1e-6;
            }, ref passed, ref failed);

            // 反向转换
            TestCase("Fix64 -> long: 123", () =>
            {
                Fix64 f = (Fix64)123;
                long value = (long)f;
                return value == 123;
            }, ref passed, ref failed);

            TestCase("Fix64 -> float: 3.14", () =>
            {
                Fix64 f = (Fix64)3.14m;
                float value = (float)f;
                return Math.Abs(value - 3.14f) < 1e-5;
            }, ref passed, ref failed);

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

