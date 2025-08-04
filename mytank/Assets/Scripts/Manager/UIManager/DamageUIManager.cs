
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using UnityEngine;
    public enum DamageType
    {
        Bullet, 
        Landmine,
        SpikeTrap,
        Bomb,
        Discipline,
        Protect,
    }
    public class DamageUIManager:SingletonMono<DamageUIManager>
    {
        public DamageIconDatabase iconDatabase;
        private ObjectPool<DamageWordUI> damageWordUIPool;
        public DamageWordUI damageWordUIPrefab;
        public Transform parent;
        private void Awake()
        {
            iconDatabase.Initialize();
            damageWordUIPool = new ObjectPool<DamageWordUI>(
                damageWordUIPrefab,
                (ui)=>ui.gameObject.SetActive(true),
                (ui)=>ui.gameObject.SetActive(false)

            );
        }

       

        public void ShowDamageWord(DamageType type, int damage,Transform spawnPoint)
        {
            DamageWordUI damageWordUI = damageWordUIPool.GetObject();
            damageWordUI.damageIcon.sprite = iconDatabase.GetIcon(type);
            damageWordUI.damageNum.text = damage.ToString();
            damageWordUI.damageNum.color=iconDatabase.GetColor(type);
            damageWordUI.transform.SetParent(parent);
            // 将游戏世界坐标转换为屏幕坐标
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(spawnPoint.position);
    
            // 设置UI位置
            RectTransform rectTransform = damageWordUI.GetComponent<RectTransform>();
            rectTransform.position = screenPosition;
    
            // 播放动画
            PlayDamageAnimation(damageWordUI);
            

        }
        private void PlayDamageAnimation(DamageWordUI damageUI)
        {
            RectTransform rect = damageUI.GetComponent<RectTransform>();
    
            // 重置初始状态
            rect.localScale = Vector3.one * 0.8f;
            damageUI.damageNum.color = new Color(
                damageUI.damageNum.color.r,
                damageUI.damageNum.color.g,
                damageUI.damageNum.color.b,
                1f
            );
            damageUI.damageIcon.color=Color.white;
    
            // 动画序列
            Sequence sequence = DOTween.Sequence();
    
            // 1. 弹出效果
            sequence.Append(rect.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
            sequence.Append(rect.DOScale(1f, 0.1f));
    
            // 2. 上浮效果
            sequence.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 30f, 1f));
    
            // 3. 淡出效果
            sequence.Join(damageUI.damageNum.DOFade(0, 1f).SetDelay(0.5f));
            if(damageUI.damageIcon != null)
            {
                sequence.Join(damageUI.damageIcon.DOFade(0, 1f).SetDelay(0.5f));
            }
    
            // 动画完成回调
            sequence.OnComplete(() => {
                damageWordUIPool.ReturnObject(damageUI);
            });
        }
    }
