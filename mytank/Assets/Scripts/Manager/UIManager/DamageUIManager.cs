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
    Charge
}

public class DamageUIManager : SingletonMono<DamageUIManager>
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
            (ui) => ui.gameObject.SetActive(true),
            (ui) => ui.gameObject.SetActive(false)
        );
    }


    public void ShowDamageWord(DamageType type, int damage, Transform spawnPoint)
    {
        DamageWordUI damageWordUI = damageWordUIPool.GetObject();
        damageWordUI.damageIcon.sprite = iconDatabase.GetIcon(type);
        damageWordUI.damageNum.text = damage.ToString();
        damageWordUI.damageNum.color = iconDatabase.GetColor(type);
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

        // Generate random values for more dramatic variation
        float randomInitialScale = UnityEngine.Random.Range(0.3f, 0.5f); // Start much smaller
        float randomPeakScale = UnityEngine.Random.Range(1.3f, 1.6f); // Bounce bigger
        float randomYOffset =UnityEngine. Random.Range(50f, 70f); // Float higher
        float randomDuration = UnityEngine.Random.Range(0.9f, 1.3f); // Slightly longer duration
        float randomBouncePower = UnityEngine.Random.Range(0.3f, 0.5f); // For bounce effect

        // Reset to very small initial state
        rect.localScale = Vector3.one * randomInitialScale;

        damageUI.damageNum.color = new Color(
            damageUI.damageNum.color.r,
            damageUI.damageNum.color.g,
            damageUI.damageNum.color.b,
            1f
        );
        damageUI.damageIcon.color = Color.white;

        // Animation sequence
        Sequence sequence = DOTween.Sequence();

        // 1. Dramatic bounce-in effect
        sequence.Append(rect.DOScale(randomPeakScale, 0.25f)
            .SetEase(Ease.OutBack, randomBouncePower)); // Stronger bounce

        // 2. Settle to normal size with overshoot
        sequence.Append(rect.DOScale(1f, 0.3f)
            .SetEase(Ease.OutElastic, 0.5f, 0.8f));

        // 3. Floating up effect with more height
        sequence.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + randomYOffset, randomDuration)
            .SetEase(Ease.OutQuad));

        // 4. Dramatic horizontal wobble
        sequence.Join(rect.DOAnchorPosX(rect.anchoredPosition.x + UnityEngine.Random.Range(-1f, 1f), randomDuration * 0.3f)
            .SetLoops(3, LoopType.Yoyo)
            .SetEase(Ease.InOutSine));

       

        // 6. Delayed dramatic fade out
        float fadeDelay = UnityEngine.Random.Range(0.3f, 0.5f);
        sequence.Join(damageUI.damageNum.DOFade(0, randomDuration - fadeDelay)
            .SetDelay(fadeDelay)
            .SetEase(Ease.InQuad));

        if (damageUI.damageIcon != null)
        {
            sequence.Join(damageUI.damageIcon.DOFade(0, randomDuration - fadeDelay)
                .SetDelay(fadeDelay)
                .SetEase(Ease.InQuad));
        }

        // 7. Final scale down as it disappears (optional)
        sequence.Join(rect.DOScale(0.8f, randomDuration - fadeDelay)
            .SetDelay(fadeDelay)
            .SetEase(Ease.InBack));

        // Animation complete callback
        sequence.OnComplete(() => { damageWordUIPool.ReturnObject(damageUI); });
    }
}