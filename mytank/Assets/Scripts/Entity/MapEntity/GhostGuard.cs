
    using DG.Tweening;
    using UnityEngine;

    public class GhostGuard: MapEntity
    {
        
        public Direction moveDirection;
        
        public int moveIntervalFrame = 10;
        private long lastMoveFrame;
        
        public int shootIntervalFrame = 60;
        private long lastShootFrame;
        public Vector2 GetCenter() => new Vector2(Pos.x + 0.5f, Pos.y + 0.5f);
        
        public int damageNum;
        
        public override void UpdateFrame()
        {
            if (NetworkManager.Instance.currentFrame - lastMoveFrame >= moveIntervalFrame)
            {
                Move();


                lastMoveFrame = NetworkManager.Instance.currentFrame;
            }
            if (NetworkManager.Instance.currentFrame - lastShootFrame >= shootIntervalFrame)
            {
                Shoot();


                lastShootFrame = NetworkManager.Instance.currentFrame;
            }
            
        }
        private void Shoot()
        {
            EntityManager.Instance.InitializeBullet(moveDirection, tank, Pos,damageNum);
        }

        private void Move()
        {
            int dx = 0, dy = 0;
            switch (moveDirection)
            {
                case Direction.Up:
                    dx = 0;
                    dy = 1;
                    break;
                case Direction.Down:
                    dx = 0;
                    dy = -1;
                    break;
                case Direction.Left:
                    dx = -1;
                    dy = 0;
                    break;
                case Direction.Right:
                    dx = 1;
                    dy = 0;
                    break;
            }
            Vector2Int targetPos=new Vector2Int(Pos.x + dx, Pos.y + dy);
            if (!MapManager.Instance.isVailePos(targetPos))
            {
                Destroy();
                return;
            }
            
            Pos = targetPos;

            // 计算坦克中心位置
            Vector2 centerPos = GetCenter();

           // isMoving = true;
          //  animator.Play("Tank" + animType);
           // lastAnimStartFrame = NetworkManager.Instance.currentFrame;
            transform.DOMove(centerPos, moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);

        }
    }
