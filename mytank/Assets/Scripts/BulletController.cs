using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // 新增

public class BulletController : MonoBehaviour
{
    public string BulletID { get; private set; }
    public string Direction { get; private set; }
    public string OwnerID { get; private set; }
    
    private bool isLocal; // 是否由本地客户端创建

    private float moveInterval = 0.3f;
    
    private float lastMoveTime;

    private Vector2Int dir;
    public Vector2Int Pos; // 权威格子坐标
   
    public float moveDuration = 0.3f; // DOTween动画时长
    
    public void Initialize(string id, string direction, string ownerID, bool local = false)
    {
        BulletID = id;
        Direction = direction;
        OwnerID = ownerID;
        isLocal = local;
        // 设置子弹朝向
        SetBulletRotation();
        Pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }
    
    void SetBulletRotation()
    {
        Vector3 rotation = Vector3.zero;
        switch (Direction)
        {
            case "up": rotation = new Vector3(0, 0, 0);
                dir = new Vector2Int(0, 1);  break;
            case "down": rotation = new Vector3(0, 0, 180);  dir = new Vector2Int(0, -1); break;
            case "left": rotation = new Vector3(0, 0, 90);  dir = new Vector2Int(-1,0); break;
            case "right": rotation = new Vector3(0, 0, -90);  dir = new Vector2Int(1, 0); break;
        }
        transform.rotation = Quaternion.Euler(rotation);
    }
    
    // 在帧同步系统中，子弹位置由服务器控制，客户端只负责显示
    // 这个方法会被GameStateManager调用来更新子弹位置
    public void UpdatePosition(int x, int y)
    {
        Pos = new Vector2Int(x, y);
        transform.DOKill();
        transform.DOMove(new Vector2(x, y), moveDuration).SetEase(Ease.Linear);
    }

    private void Update()
    {
        if (Time.time - lastMoveTime > moveInterval)
        {
            lastMoveTime = Time.time;
            int newX = Pos.x + dir.x;
            int newY = Pos.y + dir.y;
            MapType mapType = MapManager.Instance.GetWallType(newX, newY);
            var tank = MapManager.Instance.GetTankController(newX, newY);
            if (tank != null && tank.PlayerID == OwnerID)
            {
                UpdatePosition(newX, newY);
            }
            else if (tank != null && tank.PlayerID != OwnerID)
            {
                Destroy(gameObject);
            }
            else if (MapManager.Instance.IsBulletPassable(newX, newY))
            {
                UpdatePosition(newX, newY);

            }
            else if (mapType == MapType.breakableWall)
            {
                Destroy(gameObject);
                MapManager.Instance.SetWallType(newX, newY, MapType.floor);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
