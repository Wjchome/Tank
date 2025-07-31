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
        tooltipSize = rectTransform.sizeDelta * canvas.scaleFactor;
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
        // Get screen size
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        // Calculate min and max positions
        float minX = tooltipSize.x / 2;
        float maxX = screenSize.x - tooltipSize.x / 2;
        float minY = tooltipSize.y / 2;
        float maxY = screenSize.y - tooltipSize.y / 2;
        
        // Clamp the position
        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
        
        // If we're clamping on X axis, flip the offset to other side of mouse
        if (Mathf.Abs( clampedX - desiredPosition.x)>0.01f)
        {
            clampedX = mousePos.x - offset.x - tooltipSize.x;
            clampedX = Mathf.Clamp(clampedX, minX, maxX);
        }
        
        // If we're clamping on Y axis, flip the offset to other side of mouse
        if (Mathf.Abs( clampedY - desiredPosition.y)>0.01f)
        {
            clampedY = mousePos.y - offset.y - tooltipSize.y;
            clampedY = Mathf.Clamp(clampedY, minY, maxY);
        }
        
        return new Vector2(clampedX, clampedY);
    }

    public void Hide()
    {
        rectTransform.anchoredPosition = new Vector2(-2000, 0);
        isFollow = false;
    }
}