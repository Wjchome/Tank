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
        Debug.Log(tooltipSize);
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
        Vector2 adjustedPosition = ClampToScreen(mousePos);
        rectTransform.position = adjustedPosition;
    }

    private Vector2 ClampToScreen(Vector2 desiredPosition)
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 finalOffset = offset; // Start with original offset
    
        // Check right edge
        if (desiredPosition.x + tooltipSize.x > screenSize.x)
        {
            finalOffset.x = -offset.x; // Flip X offset
        }
    
        // Check top edge
        if (desiredPosition.y + tooltipSize.y > screenSize.y)
        {
            finalOffset.y = -offset.y; // Flip Y offset
        }
    
        // Apply the possibly flipped offset
        Vector2 adjustedPosition = mousePos + finalOffset;
    
        // Final clamp to ensure it's still on screen after flipping
      //  adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, 0, screenSize.x - tooltipSize.x);
    //    adjustedPosition.y = Mathf.Clamp(adjustedPosition.y, 0, screenSize.y - tooltipSize.y);
    
        return adjustedPosition;
    }
    public void Hide()
    {
        rectTransform.anchoredPosition = new Vector2(-2000, 0);
        isFollow = false;
    }
}