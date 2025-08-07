
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public class PlayerPanelUI:MonoBehaviour
    {
        [Header("Player Info")]
        public TextMeshProUGUI playerNameText;
        public Image playerImage;
        public TextMeshProUGUI playerHealthText;
        public TextMeshProUGUI playerKillText;
        public Image HPbar;
        public Image HPDelaybar;
        public RectTransform killShowPos;

        
        [Header("Player Food UI")]
        public Transform content;
        
        
    
       
        public PlayerTankController tank;
        
        private Dictionary<FoodType, PlayerFoodUI> foodUIDict = new Dictionary<FoodType, PlayerFoodUI>();
        public void Init(PlayerTankController tankController)
        {
            tank=tankController;
            HPbar.color = tank.playerColor;
            HPDelaybar.color = new Color(tank.playerColor.r,tank.playerColor.g,tank.playerColor.b,0.5f);

        }

      
        public void UpdateUI()
        {
            playerNameText.text=tank.playerName;
            if (tank.identity==Identity.Myself)
            {
                playerNameText.color = Color.yellow;
            }
            playerImage.color = tank.playerColor;
            UpdateHP();
            playerKillText.text = "击杀:" + tank.killNum;
            
           
        }


        public void UpdateHP()
        {
            playerHealthText.text=tank.HP+" / "+tank.orignalHP;
            float rate=(float)tank.HP / tank.orignalHP;
            HPbar.fillAmount = rate;
            HPDelaybar.DOFillAmount(rate, 0.5f);
        }
        
        public void UpdateKillCount(int newKillCount)
        {
            playerKillText.text = "击杀:" + tank.killNum;
            
           KillFeedbackManager.Instance. ShowKillFeedback(newKillCount,killShowPos);
            // 添加击杀数更新动画
            playerKillText.transform.DOScale(1.2f, 0.1f).OnComplete(() => {
                playerKillText.transform.DOScale(1f, 0.1f);
            });
        }

        private void Update()
        {
            HPbar.fillAmount = Mathf.Lerp(HPbar.fillAmount, ((float)tank.HP / tank.orignalHP),Time.deltaTime);

        }

       
        
        public void UpdateFoodUI(FoodType foodType, int level)
        {
            if (foodUIDict.ContainsKey(foodType))
            {
                // 更新现有UI
                foodUIDict[foodType].levelText.text = (level+1).ToString();
            }
            else
            {
                // 创建新的UI
                var foodUI = Instantiate( FoodManager.Instance.playerFoodUIPrefab, content);
                foodUI .Init(foodType, level);
             
            
                // 添加到字典
                foodUIDict[foodType] = foodUI;
            }
        }
        
    }
