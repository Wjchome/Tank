using UnityEngine;
using TMPro;
using DG.Tweening;

public class KillFeedbackUI : MonoBehaviour
{
    public RectTransform rectTransform;

    public TextMeshProUGUI playerKillText;


    public void ShowKillFeedback(int killCount, RectTransform killShowPos)
    {
        playerKillText.text = "+ " + killCount;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(killShowPos.anchoredPosition + KillFeedbackManager.Instance.offset,
            0.5f));
        sequence.Append(playerKillText.DOFade(0, 0.5f));
        sequence.OnComplete(() => { KillFeedbackManager.Instance.killFeedbackPool.ReturnObject(this); });
        sequence.Play();
    }
}


/*
public class KillFeedbackUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI killText;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float animationDuration = 1.5f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public float moveDistance = 50f;
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;

    [Header("Colors")]
    public Color normalKillColor = Color.white;
    public Color specialKillColor = Color.yellow;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (killText == null)
            killText = GetComponentInChildren<TextMeshProUGUI>();

        // 初始状态
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示击杀反馈
    /// </summary>
    /// <param name="killCount">击杀数量</param>
    /// <param name="isSpecial">是否为特殊击杀</param>
    /// <param name="position">显示位置（世界坐标）</param>
    public void ShowKillFeedback(int killCount, bool isSpecial = false, Vector3? position = null)
    {
        // 设置文本
        killText.text = $"+{killCount}";
        killText.color = isSpecial ? specialKillColor : normalKillColor;

        // 设置位置
        if (position.HasValue)
        {
            transform.position = position.Value;
        }

        // 重置状态
        canvasGroup.alpha = 0f;
        transform.localScale = startScale;
        gameObject.SetActive(true);

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 淡入 + 缩放动画
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration));
        sequence.Join(transform.DOScale(endScale, fadeInDuration).SetEase(Ease.OutBack));

        // 向上移动
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + moveDistance, animationDuration - fadeInDuration - fadeOutDuration));

        // 淡出
        sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        sequence.Join(transform.DOScale(startScale, fadeOutDuration));

        // 完成后隐藏
        sequence.OnComplete(() => {
            gameObject.SetActive(false);
        });

        sequence.Play();
    }

    /// <summary>
    /// 显示击杀反馈（屏幕坐标）
    /// </summary>
    public void ShowKillFeedbackAtScreenPosition(int killCount, bool isSpecial = false, Vector2 screenPosition = default)
    {
        if (screenPosition == default)
        {
            // 默认在屏幕中央
            screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        // 转换屏幕坐标到世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        ShowKillFeedback(killCount, isSpecial, worldPosition);
    }
} */