using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


public class AutoTurret : MapEntity
{
    public int shootIntervalFrame = 40;
    private long lastShootFrame;

    private Direction currentDirection = Direction.Up;
    
    
    public int damageNum = 1;
    

    public override void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame - lastShootFrame >= shootIntervalFrame)
        {
            Shoot();
            RotateDirection();


            lastShootFrame = NetworkManager.Instance.currentFrame;
        }
    }

    private void RotateDirection()
    {
        currentDirection = (Direction)(((int)currentDirection + 1) % 8);
        var (dir, dr) = currentDirection.ToFixVector2();


        Vector3 rotation = new Vector3(0, 0, (float)dr);

        // 旋转也用DOTween
        transform.DORotate(rotation, shootIntervalFrame * Constant.FrameInterval).SetEase(Ease.OutQuad);
    }

    private void Shoot()
    {
        var (dir, dr) = currentDirection.ToFixVector2();
        
        //EntityManager.Instance.InitializeBullet(dir, tank, myFixRect,damageNum);
    }
}