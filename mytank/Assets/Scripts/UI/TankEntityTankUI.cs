using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TankEntityTankUI : MonoBehaviour
{
    public Image healthBarFill;       // 实际血条
    public Image healthBarDelay;     // 延迟缓动的血条
    public TextMeshProUGUI nameText;
    public float delayEffectDuration = 1f;

    
    public TankController enemyTank;
    private float currentFillAmount;


    private float targetFillAmount = 1;
    public void UpdateHealthBar()
    {
        //TODO
        targetFillAmount = Mathf.Clamp01((float)enemyTank.HP / enemyTank.orignalHP);
        
        // 更新实际血条（立即变化）
        healthBarFill.fillAmount = targetFillAmount;
        
   
        // 延迟的血条效果（比实际血条慢一点消失）
        healthBarDelay.DOFillAmount(targetFillAmount, delayEffectDuration)
            .SetEase(Ease.OutQuad);
     
    }



    public void LateUpdate()
    {
        transform.rotation = Quaternion.identity;  
    }
}
