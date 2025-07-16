using System.Collections;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public string PlayerID { get; private set; }
    public int HP { get; private set; }
    public string Direction { get; private set; }
    
    private float moveInterval = 0.1f; // 移动间隔限制
    private float shootInterval = 0.5f; // 射击间隔限制
    
    private float lastMoveTime;
    private float lastShootTime;
    
    private bool isLocalPlayer;
    
    public void Initialize(string playerID, int initialHP = 3)
    {
        PlayerID = playerID;
        HP = initialHP;
        Direction = "up"; // 初始朝向
        
        isLocalPlayer = playerID == NetworkManager.Instance.playerID;
        if (isLocalPlayer)
        {
            GetComponent<Renderer>().material.color = Color.blue; // 本地玩家标记为蓝色
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red; // 远程玩家标记为红色
        }
        
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z); // 调整高度
    }
    
    void Update()
    {
        if (!isLocalPlayer || HP <= 0) return;
        
        HandleMovementInput();
        HandleShootInput();
    }
    
    void HandleMovementInput()
    {
        if (Time.time - lastMoveTime < moveInterval) return;
        
        int newX = Mathf.RoundToInt(transform.position.x);
        int newZ = Mathf.RoundToInt(transform.position.z);
        string newDirection = Direction;
        bool moved = false;
        
        if (Input.GetKey(KeyCode.W))
        {
            newZ += 1;
            newDirection = "up";
            moved = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            newZ -= 1;
            newDirection = "down";
            moved = true;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            newX -= 1;
            newDirection = "left";
            moved = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            newX += 1;
            newDirection = "right";
            moved = true;
        }
        
        if (moved && IsValidMove(newX, newZ))
        {
            // 本地逻辑先执行
            MoveTo(newX, newZ, newDirection);
            
            // 发送移动消息给服务器
            SendMoveUpdate(newX, newZ, newDirection);
            lastMoveTime = Time.time;
        }
    }
    
    public void MoveTo(int x, int z, string direction)
    {
        transform.position = new Vector3(x, 0.5f, z);
        Direction = direction;
        
        // 更新朝向
        Quaternion rotation = Quaternion.identity;
        switch (Direction)
        {
            case "up":
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case "down":
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case "left":
                rotation = Quaternion.Euler(0, 270, 0);
                break;
            case "right":
                rotation = Quaternion.Euler(0, 90, 0);
                break;
        }
        transform.rotation = rotation;
    }
    
    public bool IsValidMove(int x, int z)
    {
        // 检查地图边界
        if (x < 0 || x >= 20 || z < 0 || z >= 20)
            return false;
            
        // 检查是否是空格子（这里需要与GameManager关联以获取地图信息）
        Collider[] colliders = Physics.OverlapBox(new Vector3(x, 0.5f, z), new Vector3(0.4f, 0.4f, 0.4f));
        return colliders.Length == 0;
    }
    
    void SendMoveUpdate(int x, int z, string direction)
    {
        var moveData = new PlayerMoveData
        {
            PlayerID = PlayerID,
            X = x,
            Y = z, // 注意：在网络传输中使用Y而不是Z
            Direction = direction,
            Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        
        NetworkManager.Instance.SendMessage("player_move", moveData);
    }
    
    void HandleShootInput()
    {
        if (Time.time - lastShootTime < shootInterval) return;
        
        if (Input.GetKey(KeyCode.Space))
        {
            // 创建子弹
            ShootBullet();
            lastShootTime = Time.time;
        }
    }
    
    void ShootBullet()
    {
        int startX = Mathf.RoundToInt(transform.position.x);
        int startZ = Mathf.RoundToInt(transform.position.z);
        
        // 根据朝向确定子弹初始位置
        switch (Direction)
        {
            case "up":
                startZ += 1;
                break;
            case "down":
                startZ -= 1;
                break;
            case "left":
                startX -= 1;
                break;
            case "right":
                startX += 1;
                break;
        }
        
        // 检查子弹起始位置是否合法
        if (!IsValidMove(startX, startZ))
            return;
        
        // 创建本地子弹
        string bulletID = System.Guid.NewGuid().ToString();
        GameObject bulletObj = Instantiate(
            GameManager.Instance.bulletPrefab, 
            new Vector3(startX, 0.5f, startZ), 
            Quaternion.identity
        );
        
        BulletController bullet = bulletObj.GetComponent<BulletController>();
        bullet.Initialize(bulletID, Direction, PlayerID, true); // true表示本地创建
        
        // 发送子弹创建消息给服务器
        var bulletData = new BulletCreateData
        {
            ID = bulletID,
            PlayerID = PlayerID,
            X = startX,
            Y = startZ, // 注意：在网络传输中使用Y而不是Z
            Direction = Direction,
            Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        
        NetworkManager.Instance.SendMessage("bullet_create", bulletData);
        GameManager.Instance.bullets[bulletID] = bullet;
    }
    
    public void TakeDamage(int newHP)
    {
        HP = newHP;
        if (HP <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }
    
    IEnumerator DeathSequence()
    {
        // 死亡效果
        GetComponent<Renderer>().material.color = Color.black;
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}

