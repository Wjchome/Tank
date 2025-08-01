using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class PlayerFoodUI:MonoBehaviour
    {
        public FoodType foodType;
        public FoodData foodData;
        
        public Image image;
        public int level;
        public TextMeshProUGUI levelText;
        
        public EventTrigger eventTrigger;


        public bool isMouseOn;
        public void Init(FoodType foodType,int level)
        {
            this.foodType = foodType;
            foodData = FoodManager.Instance.foodDict[foodType];
            this.level = level;
            image.sprite =  foodData.sprite;
            levelText.text = (level+1).ToString();
       
            SetupUIEvents();
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
            TooltipUI.Instance.isFollow = true;

            TooltipUI.Instance.titleText.text = foodData.foodName;

            TooltipUI.Instance.descriptionText.text = foodData.foodDescription[level];
            
            isMouseOn = true;
        }

        private void OnPointerExit()
        {
            TooltipUI.Instance.Hide();
            isMouseOn = false;
        
        }

  
    }
