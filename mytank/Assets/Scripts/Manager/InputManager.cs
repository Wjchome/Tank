using System;
using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;
using Tankgame;

public class InputManager : SingletonMono<InputManager>
{
    private PlayerTankController player => NetworkManager.Instance.myTank;


    // 当前缓冲的帧数据（在服务器帧间隔内收集）
    private InputType bufferedInputType = InputType.InputNone;
    private long bufferedShootX = 0;
    private long bufferedShootY = 0;
    private int bufferedFoodId = -1;
    private bool hasBufferedData = false;

    Queue<int> foodTiggerQueue = new Queue<int>();

    private Vector2 mouseWorldPos;


    private void Update()
    {
        if (!NetworkManager.Instance.isGameing || player.isDead)
        {
            return;
        }

        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 收集当前帧的输入（不立即发送）
        CollectInputs();

        player.barrel.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(mouseWorldPos.y - player.transform.position.x, mouseWorldPos.x - player.transform.position.y) *
            Mathf.Rad2Deg - 90);
    }


    public void UpdateFrame()
    {
        ProcessFood();
        SendBufferedFrameData();
    }

    /// <summary>
    /// 收集当前帧的所有输入（不立即发送，先缓冲）
    /// </summary>
    void CollectInputs()
    {
        // 收集移动输入
        CollectMovementInput();

        // 收集射击输入
        CollectShootInput();
    }

    /// <summary>
    /// 收集移动输入
    /// </summary>
    void CollectMovementInput()
    {
        Vector2Int moveDir = Vector2Int.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.y += 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDir.y -= 1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x -= 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1;
        }

        if (moveDir != Vector2Int.zero)
        {
            // 更新缓冲的移动输入（保留最新的）
            bufferedInputType = moveDir.ToDirection().ToInputType();
            hasBufferedData = true;
        }
    }

    /// <summary>
    /// 收集射击输入
    /// </summary>
    void CollectShootInput()
    {
        // 检查射击冷却
        if (NetworkManager.Instance.currentFrame - player.lastShootFrame > player.CurrentShootIntervalFrame())
        {
            Vector2 targetPos = Vector2.zero;
            bool shouldShoot = false;

            // 键盘输入：使用鼠标位置作为开火目标
            if (Input.GetMouseButton(0))
            {
                targetPos = mouseWorldPos;
                shouldShoot = true;
            }

            if (shouldShoot)
            {
                // 将Vector2转换为Fix64，然后获取原始值（rawValue）
                Fix64 targetXFix = (Fix64)targetPos.x;
                Fix64 targetYFix = (Fix64)targetPos.y;

                // 更新缓冲的射击输入（保留最新的）
                bufferedShootX = targetXFix.RawValue;
                bufferedShootY = targetYFix.RawValue;
                hasBufferedData = true;
                player.lastShootFrame = NetworkManager.Instance.currentFrame;
            }
        }
    }


    void ProcessFood()
    {
        if (foodTiggerQueue.Count > 0)
        {
            hasBufferedData = true;
            bufferedFoodId = foodTiggerQueue.Dequeue();
        }
    }

    public void AddFood(int foodId)
    {
        foodTiggerQueue.Enqueue(foodId);
    }

    /// <summary>
    /// 发送缓冲的帧数据（每50ms调用一次）
    /// </summary>
    void SendBufferedFrameData()
    {
        if (!hasBufferedData)
        {
            return;
        }

        // 发送合并后的帧数据
        NetworkManager.Instance.SendFrameData(
            inputType: bufferedInputType,
            shootX: bufferedShootX,
            shootY: bufferedShootY,
            foodId: bufferedFoodId
        );

        // 重置缓冲区
        bufferedInputType = InputType.InputNone;
        bufferedShootX = 0;
        bufferedShootY = 0;
        bufferedFoodId = -1;
        hasBufferedData = false;
    }


    /// <summary>
    /// 强制发送当前缓冲的数据（用于紧急情况）
    /// </summary>
    public void FlushBuffer()
    {
        if (hasBufferedData)
        {
            SendBufferedFrameData();
        }
    }
}