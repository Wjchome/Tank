using System.Collections.Generic;
using UnityEngine;

public class KillFeedbackManager : SingletonMono<KillFeedbackManager>
{
    public KillFeedbackUI killFeedbackPrefab;


    public Vector2 offset;
    
    public ObjectPool<KillFeedbackUI> killFeedbackPool;
    

    
    private List<KillFeedbackUI> activeFeedbacks = new List<KillFeedbackUI>();
    
    private void Awake()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        killFeedbackPool = new ObjectPool<KillFeedbackUI>(
            killFeedbackPrefab,
            CreateKillFeedback,
            ReturnKillFeedback
        );
    }
   
   
    /// <summary>
    /// 显示击杀反馈
    /// </summary>
    /// <param name="killCount">击杀数量</param>
    /// <param name="isSpecial">是否为特殊击杀</param>
    /// <param name="position">显示位置</param>
    public void ShowKillFeedback(int killCount, RectTransform killShowPos)
    {
        KillFeedbackUI feedback = killFeedbackPool.GetObject();
        feedback.transform.SetParent(killShowPos);
        feedback.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        feedback.ShowKillFeedback(killCount, killShowPos);
     
    }
    


    
    private void  CreateKillFeedback(KillFeedbackUI feedback)
    {
            feedback.gameObject.SetActive(true);
            feedback.playerKillText.color = Color.white;
        activeFeedbacks.Add(feedback);
    }
    

    protected void ReturnKillFeedback(KillFeedbackUI feedback)
    {
        if (activeFeedbacks.Contains(feedback))
        {
            activeFeedbacks.Remove(feedback);
            feedback.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 清理所有活跃的反馈
    /// </summary>
    public void ClearAllFeedbacks()
    {
        foreach (var feedback in activeFeedbacks.ToArray())
        {
            ReturnKillFeedback(feedback);
        }
    }
} 