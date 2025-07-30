using TMPro;
using UnityEngine;
public class TooltipUI : SingletonMono<TooltipUI>
{
    public RectTransform rectTransform;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public bool isFollow = false;
  
    public Vector2 offset = new Vector2(10, -10);




    private void Update()
    {
        if (isFollow)
        {
            FollowMouse();
        }
    }
    
    private void FollowMouse()
    {
        // Screen Space - Overlay 模式下直接使用屏幕坐标
        Vector2 mousePos = Input.mousePosition;
        
        // 直接设置位置，不需要坐标转换
        rectTransform.position = mousePos + offset;
    }

    public void Hide()
    {
        rectTransform.anchoredPosition = new Vector2(-1000, 0);
        isFollow = false;
    }
}