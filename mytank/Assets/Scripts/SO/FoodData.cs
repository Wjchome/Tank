
    using UnityEngine;
[CreateAssetMenu(fileName = "FoodData", menuName = "Tank Game/FoodData")]

    public class FoodData:ScriptableObject
    {
        public FoodType foodType;
        public Sprite  sprite;
        public string  foodName;
        public string[]  foodDescription;
    }

