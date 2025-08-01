using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// UI倒计时实体基类
public abstract class UITimerEntity : MonoBehaviour, IEntity
{
    public PlayerTankController tank;
    public Image timerImage;
    public int genateIntervalFrame;
    public long lastTriggerFrame;
    public EventTrigger eventTrigger;


    private bool isMouseOn = false;
    public virtual void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame - lastTriggerFrame >= genateIntervalFrame)
        {
            OnTimerComplete();
            lastTriggerFrame = NetworkManager.Instance.currentFrame;
        }

        // 更新UI显示
        UpdateTimerUI();
    }
    protected abstract void ApplyEffect(); // 应用效果

    protected virtual void OnTimerComplete()
    {
        ApplyEffect();
    }

    protected virtual void UpdateTimerUI()
    {
        if (timerImage != null)
        {
            float progress = (float)(NetworkManager.Instance.currentFrame - lastTriggerFrame) / genateIntervalFrame;
            timerImage.fillAmount = progress;
        }
        if (isMouseOn)
        {
            ShowTooltip();
        }
    }

    public virtual void Init(PlayerTankController _tank)
    {
        tank = _tank;
        lastTriggerFrame = -genateIntervalFrame;
        SetupUIEvents();
    }

    public virtual void Destroy()
    {
        EntityManager.Instance.RemoveEntity(this);
        Destroy(gameObject);
    }

    private void SetupUIEvents()
    {
        eventTrigger.triggers.Clear();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnPointerEnter(); });
        eventTrigger.triggers.Add(enterEntry);

       
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExit(); });
        eventTrigger.triggers.Add(exitEntry);
    }

    private void OnPointerEnter()
    {
        ShowTooltip();
        isMouseOn = true;
    }

    private void OnPointerExit()
    {
        HideTooltip();
        isMouseOn = false;
        
    }


    protected virtual void ShowTooltip()
    {
        TooltipUI.Instance.titleText.text=$"<b><color={Constant.TITLE_COLOR}>填充</color></b>";
        TooltipUI.Instance.isFollow = true;
    }

    protected virtual void HideTooltip()
    {
        TooltipUI.Instance.Hide();
    }
}

