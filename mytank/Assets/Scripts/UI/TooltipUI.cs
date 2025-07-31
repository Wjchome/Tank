using TMPro;
using UnityEngine;

public class TooltipUI : SingletonMono<TooltipUI>
{
    public RectTransform rectTransform;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public bool isFollow = false;
    public Vector2 offset = new Vector2(50, 200);

    private Canvas canvas;
    private RectTransform canvasRect;
    private Vector2 tooltipSize;

    private void Start()
    {
        // Get the canvas and its rect transform
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        
        // Calculate the tooltip size once (assuming it doesn't change)
        tooltipSize = rectTransform.sizeDelta/* * canvas.scaleFactor*/;
    }

    private void Update()
    {
        if (isFollow)
        {
            FollowMouse();
        }
    }
    Vector2 mousePos = Vector2.zero;
    private void FollowMouse()
    {
         mousePos = Input.mousePosition;
        Vector2 adjustedPosition = mousePos + offset;
        
        // Adjust position to keep tooltip on screen
        adjustedPosition = ClampToScreen(adjustedPosition);
        
        rectTransform.position = adjustedPosition;
    }

    private Vector2 ClampToScreen(Vector2 desiredPosition)
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
    
        // 计算tooltip的边界
        float halfWidth = tooltipSize.x / 2;
        float halfHeight = tooltipSize.y / 2;

        if (desiredPosition.x + halfWidth > screenSize.x)
        {
            desiredPosition.x  -= halfWidth;
        }

        if (desiredPosition.y + halfHeight > screenSize.y)
        {
            desiredPosition.y -= halfHeight;
        }
        
        // 限制X轴：确保tooltip的左右边界都在屏幕内
       // float clampedX = Mathf.Clamp(desiredPosition.x, halfWidth, screenSize.x - halfWidth);
    
        // 限制Y轴：确保tooltip的上下边界都在屏幕内
      //  float clampedY = Mathf.Clamp(desiredPosition.y, halfHeight, screenSize.y - halfHeight);
    
        return desiredPosition;
    }
    public void Hide()
    {
        rectTransform.anchoredPosition = new Vector2(-2000, 0);
        isFollow = false;
    }
}