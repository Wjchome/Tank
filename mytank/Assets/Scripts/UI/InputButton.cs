using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputButton : MonoBehaviour
{
    public EventTrigger eventTrigger;
    public bool isPressed;
    private void Start()
    {
        // 添加 PointerDown 事件
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerEnter;
        pointerDown.callback.AddListener((data) => { OnPointerDown(); });
        eventTrigger.triggers.Add(pointerDown);
        
        // 添加 PointerUp 事件
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerExit;
        pointerUp.callback.AddListener((data) => { OnPointerUp(); });
        eventTrigger.triggers.Add(pointerUp);
    }
    
    private void OnPointerDown()
    {
        isPressed = true;
        // 按下逻辑
    }
    
    private void OnPointerUp()
    {
        isPressed = false;
        // 抬起逻辑
    }


}