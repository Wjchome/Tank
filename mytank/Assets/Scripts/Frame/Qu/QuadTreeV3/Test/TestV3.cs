using System;
using System.Collections.Generic;
using System.Text;
using FixMath.NET;
using UnityEngine;
using System.Security.Cryptography;
namespace QuadTreeV3
{
    public class TestV3 : MonoBehaviour
    {
        public List<Vector2> points;

        public float size = 1;

        public Vector2 start;

        public float mySize = 1;

        List<FixRect> allRects = new List<FixRect>();
        FixRect myRect;
        private GameObject go1;
        public GameObject prefab;

        public float rotate;
        public float rotateDt;

        private void Start()
        {
            for (int i = 0; i < points.Count; i++)
            {
                GameObject go = Instantiate(prefab, points[i], Quaternion.identity);
                go.name = i.ToString();
                allRects.Add(new FixRect((Fix64)(points[i].x - size), (Fix64)(points[i].y - size), (Fix64)(2 * size),
                    (Fix64)(2 * size)));
                QuadTreeV3.Instance.AddObject(allRects[i], go,new QuadTreeLayer(i+1));
            }
            // 001 
            // 010
            // 011
            // 100
            // 101

            go1 = Instantiate(prefab, start, Quaternion.Euler(0,0,rotate));
            myRect = new FixRect((Fix64)(start.x - mySize), (Fix64)(start.y - mySize), (Fix64)(2 * mySize),
                (Fix64)(2 * mySize),(Fix64)rotate);
            QuadTreeV3.Instance.AddObject(myRect, go1);
        }

        int serverFrameCounter = 0;
        public Vector2 Velocity;
        StringBuilder sb = new StringBuilder();

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                serverFrameCounter++;
                myRect.X += (Fix64)Velocity.x;
                myRect.Y += (Fix64)Velocity.y;
                myRect.Rotate += (Fix64)rotateDt;
                go1.transform.position = new Vector3((float)myRect.CenterX,(float) myRect.CenterY, 0);
                go1.transform.rotation = Quaternion.Euler(0, 0, (float)myRect.Rotate);
                QuadTreeV3.Instance.UpdateObject(go1, myRect);
                var res = QuadTreeV3.Instance.Query(myRect,new QuadTreeLayer(1));
                foreach (var re in res)
                {
                    if (re.Target != go1)
                    {
                        Debug.Log($"{re.Target.name}  {serverFrameCounter}");
                        sb.AppendLine($"{re.Target.name}  {serverFrameCounter}");
                    }
                }
            }
        }

        public void OnDestroy()
        {
            Debug.Log(sb);
            Debug.Log(sb.Length);
            Debug.Log(CalculateMD5(sb.ToString()));
        }
        // 计算字符串的MD5哈希（返回16字节数组，可转32位十六进制字符串）
        private static string CalculateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
        
                // 转成32位十六进制字符串（方便显示和对比）
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2")); // 小写，x2保证两位十六进制
                }
                return sb.ToString();
            }
        }
    }
}