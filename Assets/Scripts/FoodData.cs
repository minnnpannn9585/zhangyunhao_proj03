using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "TetrisFood/FoodData")]
public class FoodData : ScriptableObject
{
    public string foodName;          // 食物名称
    public Sprite foodSprite;        // 食物外观
    public int baseScore;            // 基础分值
    public float bestCookedRate;     // 最佳熟度（0-100）
    public float scoreWeight;        // 熟度得分权重
}