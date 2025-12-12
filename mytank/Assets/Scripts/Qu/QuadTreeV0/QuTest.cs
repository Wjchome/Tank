using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuadTreeV0
{
    public class QuTest : MonoBehaviour
    {
        public GameObject Cube;
        public QuadTree quadTree;

        public float size;

        // 移除检测范围（建议略大于物体尺寸，避免漏检）
        public float removeCheckRange = 1f;

        private void Update()
        {
            // 左键生成物体并添加到四叉树
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector2 pos = Input.mousePosition;
                // 坐标映射：将屏幕坐标转成游戏内的(-8,8) x (-3,5)范围
                pos.x = Mathf.Lerp(-8, 8, viewportPos.x);
                pos.y = Mathf.Lerp(-3, 5, viewportPos.y);

                GameObject a = Instantiate(Cube, pos, Quaternion.identity);
                // 添加物体到四叉树（边界为物体中心±size/2）
                quadTree.AddObjectToQuadTree(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), a);
            }

            // 右键移除鼠标位置的物体
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector2 pos = Input.mousePosition;
                pos.x = Mathf.Lerp(-8, 8, viewportPos.x);
                pos.y = Mathf.Lerp(-3, 5, viewportPos.y);

                // 步骤1：定义鼠标位置的检测区域（矩形）
                Rect checkArea = new Rect(
                    pos.x - removeCheckRange / 2,
                    pos.y - removeCheckRange / 2,
                    removeCheckRange,
                    removeCheckRange
                );

                // 步骤2：从四叉树查询该区域内的所有物体
                List<GameObject> objectsInArea = quadTree.GetObjectsInArea(checkArea);

                // 步骤3：遍历查询结果，移除并销毁物体
                if (objectsInArea.Count > 0)
                {
                    // 取第一个物体（也可根据距离筛选最近的）
                    GameObject target = objectsInArea[0];

                    // 从四叉树中移除物体
                    quadTree.RemoveObjectFromQuadTree(target);

                    // 销毁游戏物体（可选，根据需求决定是否销毁）
                    Destroy(target);
                }
            }

            if (Input.GetMouseButtonDown(2))
            {
                var all = FindObjectsOfType<CubeMovetest>();
                all.ToList().ForEach(a => a.isMove = !a.isMove);
            }
        }
    }
}