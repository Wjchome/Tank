using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletController : MonoBehaviour
{
    public string BulletID { get; private set; }
    public string Direction { get; private set; }
    public string OwnerID { get; private set; }
    
    private bool isLocal; // 是否由本地客户端创建
    
    public Vector2Int Pos => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    
    public void Initialize(string id, string direction, string ownerID, bool local = false)
    {
        BulletID = id;
        Direction = direction;
        OwnerID = ownerID;
        isLocal = local;
        
        // 设置子弹朝向
        SetBulletRotation();
    }
    
    void SetBulletRotation()
    {
        Vector3 rotation = Vector3.zero;
        switch (Direction)
        {
            case "up": rotation = new Vector3(0, 0, 0); break;
            case "down": rotation = new Vector3(0, 0, 180); break;
            case "left": rotation = new Vector3(0, 0, 90); break;
            case "right": rotation = new Vector3(0, 0, -90); break;
        }
        transform.rotation = Quaternion.Euler(rotation);
    }
    
    // 在帧同步系统中，子弹位置由服务器控制，客户端只负责显示
    // 这个方法会被GameStateManager调用来更新子弹位置
    public void UpdatePosition(int x, int y)
    {
        transform.position = new Vector2(x, y);
    }
}
