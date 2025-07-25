
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

public enum FoodType
{
    Bomb,
    Encourage,
    Pistol,
    PocketWatch,
     
    
   
    Shoe,
    Shovel,
    Star,
     SteelHelmet,
   WarCar,
    
    
}
    public class FoodFactory:SingletonMono<FoodFactory>
    {
        public List<FoodData> foodDatas;
      

    public RectTransform panelParent;
    public List<FoodUIPanel> panels;
    

    public void ShowPanels(TankController tank, Random random)
    {
        panelParent.DOAnchorPos(new Vector2(0, 0), 0.5f);
        
        for (int index = 0;
             index < 3;
             index++)
        {
            panels[index].gameObject.SetActive(true);
            panels[index].chooseButton.interactable = true;
            abcd(index, random);
        }




    }

    public void ClosePanels()
    {
        panelParent.DOAnchorPos(new Vector2(0, -800), 0.5f);
    }

    void abcd(int index , Random random)
    {
        FoodData randomFood = foodDatas[random.Next(0, foodDatas.Count)];
        panels[index].image.sprite = randomFood.sprite;
        panels[index].foodName.text = randomFood.foodName;
        panels[index].foodDescription.text = randomFood.foodDescription;
        panels[index].chooseButton.onClick.AddListener(() =>
        {
            NetworkManager.Instance.FoodChooseRequest(randomFood.foodType);
            panels[0].chooseButton.interactable = false;
            panels[1].chooseButton.interactable = false;
            panels[2].chooseButton.interactable = false;
            ClosePanels();
        });
    }
    
    }

    