using UnityEngine;

[CreateAssetMenu(fileName = "FoodCombination", menuName = "Tank Game/FoodCombination")]

public class FoodCombination : ScriptableObject
{
    public FoodType food1;
    public FoodType food2;
    public FoodType food3;

    public FoodType foodTarget;
}