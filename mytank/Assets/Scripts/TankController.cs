using System;
using System.Collections;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public string PlayerID { get; private set; }
    public int HP { get; private set; }
    public string Direction { get; private set; }
    
    public Vector2Int Position { get=>new Vector2Int(Mathf.RoundToInt( transform.position.x ),Mathf.RoundToInt( transform.position.y ));  }
    private float moveInterval = 0.1f; // 移动间隔限制
    private float shootInterval = 0.5f; // 射击间隔限制
    
    private float lastMoveTime;
    private float lastShootTime;
    
 public bool isLocalPlayer;

    private void OnEnable()
    {
        TankManager.Instance.controllers.Add(this);
    }

    private void OnDisable()
    {
        TankManager.Instance.controllers.Remove(this);
    }

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
        int newY = Mathf.RoundToInt(transform.position.y);
        string newDirection = Direction;
        bool moved = false;
        
        if (Input.GetKey(KeyCode.W))
        {
            newY += 1;
            newDirection = "up";
            moved = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            newY -= 1;
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
        
        if (moved && MapManager.Instance.IsWalkable(newX, newY))
        {
            // 本地逻辑先执行
            MoveTo(newX, newY, newDirection);
            
            // 发送移动消息给服务器
            SendMoveUpdate(newX, newY, newDirection);
            lastMoveTime = Time.time;
        }
    }
    
    public void MoveTo(int x, int y, string direction)
    {
        transform.position = new Vector2(x, y);
        Direction = direction;
        Vector3 rotation=Vector3.zero;
        switch (direction)
        {
            case "up":rotation= new Vector3(0,0,0);
                break;
            case "down":rotation= new Vector3(0,0,180);
                break;
            case "left":rotation= new Vector3(0,0,90);
                break;
            case "right":rotation= new Vector3(0,0,-90);
                break;
                
        }
        transform.rotation = Quaternion.Euler(rotation);
    }
    
    void SendMoveUpdate(int x, int y, string direction)
    {
        var moveData = new PlayerMoveData
        {
            PlayerID = PlayerID,
            X = x,
            Y = y, 
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
        int startY = Mathf.RoundToInt(transform.position.y);
        
        // 根据朝向确定子弹初始位置
        switch (Direction)
        {
            case "up":
                startY += 1;
                break;
            case "down":
                startY -= 1;
                break;
            case "left":
                startX -= 1;
                break;
            case "right":
                startX += 1;
                break;
        }
        
       
        
        // 创建本地子弹
        string bulletID = System.Guid.NewGuid().ToString();
        GameObject bulletObj = Instantiate(
            GameManager.Instance.bulletPrefab, 
            new Vector3(startX, 0.5f, startY), 
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
            Y = startY, // 注意：在网络传输中使用Y而不是Z
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

