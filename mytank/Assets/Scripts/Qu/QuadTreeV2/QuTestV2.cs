using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using UnityEngine;

namespace QuadTreeV2
{
    public class QuTestV2 : MonoBehaviour
    {
        public GameObject Cube;
        public GameObject Circle;

        public float size;
        
        public float removeCheckRange = 1f;
        
        public List<CubeMovetestV2> AllCubeMovetestV2  = new List<CubeMovetestV2>();
        public List<CircleMovetestV2> AllCircleMovetestV2  = new List<CircleMovetestV2>();

        
        private void Update()
        {
            // 左键生成物体并添加到四叉树
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector2 pos = Input.mousePosition;
                pos.x = Mathf.Lerp(QuadTreeV2.ins.Bounds.x,QuadTreeV2.ins.Bounds.x+QuadTreeV2.ins.Bounds.width, viewportPos.x);
                pos.y = Mathf.Lerp(QuadTreeV2.ins.Bounds.y, QuadTreeV2.ins.Bounds.y+QuadTreeV2.ins.Bounds.height, viewportPos.y);

                GameObject a = Instantiate(Cube, pos, Quaternion.identity);
                var cube = a.GetComponent<CubeMovetestV2>();
                AllCubeMovetestV2.Add(cube);
                cube.Init(pos,new Vector2(1,1));
                
            }

            // 右键移除鼠标位置的物体
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector2 pos = Input.mousePosition;
                pos.x = Mathf.Lerp(QuadTreeV2.ins.Bounds.x,QuadTreeV2.ins.Bounds.x+QuadTreeV2.ins.Bounds.width, viewportPos.x);
                pos.y = Mathf.Lerp(QuadTreeV2.ins.Bounds.y, QuadTreeV2.ins.Bounds.y+QuadTreeV2.ins.Bounds.height, viewportPos.y);
                
                // 步骤1：定义鼠标位置的检测区域（矩形）
                FixRect checkArea = new FixRect(
                    (Fix64)((pos.x - removeCheckRange / 2)),
                    (Fix64)((pos.y - removeCheckRange / 2)),
                    (Fix64)(removeCheckRange),
                    (Fix64)(removeCheckRange)
                );

                // 步骤2：从四叉树查询该区域内的所有物体
                List<GameObject> objectsInArea = QuadTreeV2.ins.GetObjectsInArea(checkArea);

                // 步骤3：遍历查询结果，移除并销毁物体
                if (objectsInArea.Count > 0)
                {
                    // 取第一个物体（也可根据距离筛选最近的）
                    GameObject target = objectsInArea[0];
                    AllCubeMovetestV2.Remove(target.GetComponent<CubeMovetestV2>());
                    AllCircleMovetestV2.Remove(target.GetComponent<CircleMovetestV2>());
                    // 从四叉树中移除物体
                    QuadTreeV2.ins.RemoveObjectFromQuadTree(target);

                    // 销毁游戏物体（可选，根据需求决定是否销毁）
                    Destroy(target);
                }
            }
            
            if (Input.GetMouseButtonDown(2))
            {
                AllCubeMovetestV2.ForEach(a => a.isMove = !a.isMove);
                AllCircleMovetestV2.ForEach(a => a.isMove = !a.isMove);
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector2 pos = Input.mousePosition;
                pos.x = Mathf.Lerp(QuadTreeV2.ins.Bounds.x,QuadTreeV2.ins.Bounds.x+QuadTreeV2.ins.Bounds.width, viewportPos.x);
                pos.y = Mathf.Lerp(QuadTreeV2.ins.Bounds.y, QuadTreeV2.ins.Bounds.y+QuadTreeV2.ins.Bounds.height, viewportPos.y);

                GameObject a = Instantiate(Circle, pos, Quaternion.identity);
                var cube = a.GetComponent<CircleMovetestV2>();
                AllCircleMovetestV2.Add(cube);
                cube.Init(pos,1);
            }
          

            foreach (var cube in AllCubeMovetestV2)
            {
                cube.UpdateFrame();
            }
            foreach (var cube in AllCircleMovetestV2)
            {
                cube.UpdateFrame();
            }
        }
    }
}