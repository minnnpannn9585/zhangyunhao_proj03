using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "TetrisFood/FoodData")]
public class FoodData : ScriptableObject
{
    public string foodName;          // 食物名称
    public Sprite foodSprite;        // 食物外观
}