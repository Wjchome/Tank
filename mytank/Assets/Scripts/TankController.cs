using System;
using UnityEngine;
using Tankgame;

public class TankController : MonoBehaviour
{
    public string PlayerID { get; private set; }
    public int HP;
    public string Direction { get; private set; }

    public Vector2Int Pos =>
        new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

    public bool isLocalPlayer;

    public void Initialize(string playerID, int initialHP = 3)
    {
        PlayerID = playerID;
        HP = initialHP;
        Direction = "up";
        isLocalPlayer = playerID == NetworkManager.Instance.playerID;
     
    }

    void Update()
    {
        if (!isLocalPlayer || HP <= 0) return;
        HandleMovementInput();
        HandleShootInput();
    }

    void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            NetworkManager.Instance.SendPlayerInput(InputType.InputMoveUp);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            NetworkManager.Instance.SendPlayerInput(InputType.InputMoveDown);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkManager.Instance.SendPlayerInput(InputType.InputMoveLeft);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            NetworkManager.Instance.SendPlayerInput(InputType.InputMoveRight);
        }
    }

    void HandleShootInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NetworkManager.Instance.SendPlayerInput(InputType.InputShoot);
        }
    }

    // 帧同步推进调用
    public void MoveBy(int dx, int dy, string direction)
    {
        transform.position += new Vector3(dx, dy, 0);
        Direction = direction;
        Vector3 rotation = Vector3.zero;
        switch (direction)
        {
            case "up": rotation = new Vector3(0, 0, 0); break;
            case "down": rotation = new Vector3(0, 0, 180); break;
            case "left": rotation = new Vector3(0, 0, 90); break;
            case "right": rotation = new Vector3(0, 0, -90); break;
        }
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void Shoot()
    {
        // 可实现本地子弹生成逻辑，或简单Debug
        Debug.Log($"{PlayerID} shoot!");
    }

    public void TakeDamage(int newHP)
    {
        HP = newHP;
        if (HP <= 0)
        {
            GetComponent<Renderer>().material.color = Color.black;
            gameObject.SetActive(false);
        }
    }
}

