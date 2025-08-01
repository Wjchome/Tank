
    using System.Collections.Generic;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerPanelUI:MonoBehaviour
    {
        public Transform playerInfo;
        public TextMeshProUGUI playerNameText;
        public Image playerImage;
        public TextMeshProUGUI playerHealthText;
        public TextMeshProUGUI playerKillText;
        public Image HPbar;

        public int interval;

        public Transform scollView;
        public Transform content;
        
        

        public Button button;
        public bool isOpenFood=false;

        public Vector2 leftPos = Vector2.zero; 
        public Vector2 rightPos = Vector2.zero; 
        
        public TankController tank;
        public string tankID;
        
        private Dictionary<FoodType, PlayerFoodUI> foodUIDict = new Dictionary<FoodType, PlayerFoodUI>();
        public void Init(TankController tankController, int index)
        {
            tank=tankController;
            tankID=tank.tankID;
            HPbar.color = tank.playerColor;
            
            GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-index*interval);
            leftPos=playerInfo.GetComponent<RectTransform>().anchoredPosition;
            rightPos=scollView.GetComponent<RectTransform>().anchoredPosition;
            button.onClick.AddListener(Change);
        }

        public void Revival(TankController tankController)
        {
            tank=tankController;
            UpdateUI();
        }
        public void UpdateUI()
        {
            playerNameText.text=tank.playerName;
            if (tank.identity==Identity.Myself)
            {
                playerNameText.color = Color.yellow;
            }
            playerImage.color = tank.playerColor;
            playerHealthText.text=tank.currentData.HP+" / "+tank.currentData.orignalHP;
            playerKillText.text="击杀:"+tank.killNum.ToString();
            HPbar.fillAmount = ((float)tank.currentData.HP / tank.currentData.orignalHP);
        }

        public void Change()
        {
            if (isOpenFood)
            {
                playerInfo.GetComponent<RectTransform>().DOAnchorPos(leftPos, 0.5f);
                scollView.GetComponent<RectTransform>().DOAnchorPos(rightPos, 0.5f);
                isOpenFood=false;
            }
            else
            {
                playerInfo.GetComponent<RectTransform>().DOAnchorPos(rightPos, 0.5f);
                scollView.GetComponent<RectTransform>().DOAnchorPos(leftPos, 0.5f);
                // 清除所有子物体
               /* for (int i = content.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(content.GetChild(i).gameObject);
                }

                foreach (var kv in tank.foodDict)
                {
                    var foodUI = Instantiate(playerFoodUIPrefab, content);
                    foodUI.image.sprite=FoodManager.Instance.foodDict[kv.Key].sprite;
                    foodUI.text.text = kv.Value.ToString();
                }*/
                
                
                isOpenFood=true;
            }
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
