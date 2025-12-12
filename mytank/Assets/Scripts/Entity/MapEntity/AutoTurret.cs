using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


public class AutoTurret : MapEntity
{
    public int shootIntervalFrame = 40;
    private long lastShootFrame;

    private Direction currentDirection = Direction.Up;

    public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);
    
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
        currentDirection = (Direction)(((int)currentDirection + 1) % 4);


        Vector3 rotation = Vector3.zero;
        switch (currentDirection)
        {
            case Direction.Up: rotation = new Vector3(0, 0, 0); break;
            case Direction.Down: rotation = new Vector3(0, 0, 180); break;
            case Direction.Left: rotation = new Vector3(0, 0, 90); break;
            case Direction.Right: rotation = new Vector3(0, 0, -90); break;
        }

        // 旋转也用DOTween
        transform.DORotate(rotation, shootIntervalFrame * Constant.FrameInterval).SetEase(Ease.OutQuad);
    }

    private void Shoot()
    {
        EntityManager.Instance.InitializeBullet(currentDirection, tank, tank.myFixRect,damageNum);
    }
}