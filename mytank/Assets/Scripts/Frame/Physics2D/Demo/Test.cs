using System;
using System.Text;
using UnityEngine;

namespace Physics2D
{
    public class Test : SingletonMono<Test>
    {
        public RigidBody2DComponent Body;

        public int count;

        StringBuilder stringBuilder = new StringBuilder();

        public void UpdateFrame()
        {
            count++;
            stringBuilder.Clear();
            bool hasContent = false;
            // 检查 Enter 列表
            if (Body.Enter != null && Body.Enter.Count > 0)
            {
                stringBuilder.Append("Enter:\n");
                foreach (var item in Body.Enter)
                {
                    stringBuilder.Append(item + " ");
                }

                stringBuilder.AppendLine();
                hasContent = true;
            }

            // 检查 Stay 列表
            if (Body.Stay != null && Body.Stay.Count > 0)
            {
                stringBuilder.Append("Stay:\n");
                foreach (var item in Body.Stay)
                {
                    stringBuilder.Append(item + " ");
                }

                stringBuilder.AppendLine();
                hasContent = true;
            }

            // 检查 Exit 列表
            if (Body.Exit != null && Body.Exit.Count > 0)
            {
                stringBuilder.Append("Exit:\n");
                foreach (var item in Body.Exit)
                {
                    stringBuilder.Append(item + " ");
                }

                stringBuilder.AppendLine();
                hasContent = true;
            }

            // 只在有内容时打印
            if (hasContent)
            {
                Debug.Log(count+" "+ stringBuilder.ToString());
            }
        }
    }
}