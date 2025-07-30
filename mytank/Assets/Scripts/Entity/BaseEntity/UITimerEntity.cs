using UnityEngine;
using UnityEngine.UI;




// UI倒计时实体基类
public abstract class UITimerEntity : MonoBehaviour, IEntity
{
    public TankController tank;
    public Image timerImage; 
    public int genateIntervalFrame ;
    public long lastTriggerFrame ;
    
    public virtual void UpdateFrame()
    {
        if (NetworkManager.Instance.currentFrame- lastTriggerFrame >= genateIntervalFrame)
        {
            OnTimerComplete();
            lastTriggerFrame = NetworkManager.Instance.currentFrame;
        }
        
        // 更新UI显示
        UpdateTimerUI();
    }
    
    protected abstract void OnTimerComplete(); // 倒计时完成时的行为
    
    protected virtual void UpdateTimerUI()
    {
        if (timerImage != null)
        {
            float progress = (float)(NetworkManager.Instance.currentFrame - lastTriggerFrame) / genateIntervalFrame;
            timerImage.fillAmount = progress;
        }
    }
    
    public virtual void Init(TankController _tank)
    {
        tank = _tank;
        lastTriggerFrame = -genateIntervalFrame;
    }
    
    public virtual void Destroy()
    {
        EntityManager.Instance.RemoveEntity(this);
        Destroy(gameObject);
    }
}

// 生成器实体（生成地图实体）
public abstract class SpawnerEntity : UITimerEntity
{
    protected abstract void  SpawnMapEntity(); // 生成地图实体
    
    protected override void OnTimerComplete()
    {
         SpawnMapEntity();
        
    }
}

// 效果实体（直接调用效果）
public abstract class EffectEntity : UITimerEntity
{
    protected abstract void ApplyEffect(); // 应用效果
    
    protected override void OnTimerComplete()
    {
        ApplyEffect();
    }
} 