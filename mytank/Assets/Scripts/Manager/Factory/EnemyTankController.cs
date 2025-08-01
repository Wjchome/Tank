
    using DG.Tweening;
    using UnityEngine;

    public class EnemyTankController:TankController
    {
        public override void UpdateFrame()
        {
            if (identity == Identity.Enemy && !isDead &&
                NetworkManager.Instance.currentFrame >= EnemyManager.Instance.pauseEndFrame)
            {
                AiControls();
            }
        }
        
        
        
        void AiControls()
        {
            if (NetworkManager.Instance.currentFrame - lastMoveFrame > currentData.moveIntervalFrame)
            {
                if (!MoveBy(tankDirection))
                {
                    int a = random.Next(0, 4);

                    tankDirection = (Direction)a;

                    // 使用帧时间更新
                    lastMoveFrame = NetworkManager.Instance.currentFrame;
                }
                else
                {
                    // 移动成功
                    lastMoveFrame = NetworkManager.Instance.currentFrame;
                }
            }

            if (NetworkManager.Instance.currentFrame - lastShootFrame > currentData.shootIntervalFrame)
            {
                Shoot();
                lastShootFrame = NetworkManager.Instance.currentFrame;
            }
        }

        protected override bool IsCanMoveTo(Vector2Int targetPos)
        {
            return MapManager.Instance.IsAreaWalkable(targetPos.x, targetPos.y, 2, 2, tankID);
        }

        protected override void MoveTo(Vector2Int targetPos)
        {
            Pos = targetPos;

            // 计算坦克中心位置
            Vector2 centerPos = GetCenter();

            isMoving = true;
            animator.Play("Tank" + animType);
            lastAnimStartFrame = NetworkManager.Instance.currentFrame;
            transform.DOMove(centerPos, currentData.moveIntervalFrame * Constant.FrameInterval).SetEase(Ease.Linear);

        }


        public override void Shoot()
        {
            EntityManager.Instance.InitializeBullet(tankDirection, this, Pos, bulletDamageNum);
            
        }

        public override void AddOrignalHP(int num)
        {
        currentData.orignalHP += num;
            
        }
        public override void AddHP(int num)
        {
            currentData.HP = Mathf.Min(currentData.HP + num, currentData.orignalHP);
        }
        
        public void DamageHP(int damage, PlayerTankController attacker)
        {


            currentData.HP -= damage;

            if (currentData.HP <= 0)
            {
                Dead(attacker);
            }
        }

        public void Dead(PlayerTankController attacker)
        {
            if (isDead) return;
            isDead = true;

            animator.Play("BigBoom");

            deathDelayFrames = NetworkManager.Instance.currentFrame + (int)(animTime / Constant.FrameInterval);


            if (attacker != null)
            {
                if (attacker.isSpeedKiller)
                {
                    EntityManager.Instance.InitSpikeTrap(attacker, Pos, int.MaxValue);
                    EntityManager.Instance.InitSpikeTrap(attacker, PosRight, int.MaxValue);
                    EntityManager.Instance.InitSpikeTrap(attacker, PosUp, int.MaxValue);
                    EntityManager.Instance.InitSpikeTrap(attacker, PosUpRight, int.MaxValue);
                    Debug.LogWarning(Pos);
                }

                attacker.Kill(GetComponent<Special>() != null);
            }
            Pos = new Vector2Int(-2, -2);
        
        }
        
        protected override void ExecuteDeathLogic()
        {
            EnemyManager.Instance.RemoveEnemy(this);
            EnemyManager.Instance.TankPool.ReturnObject(this);
        deathDelayFrames = 0;
            
        }
        
    }
