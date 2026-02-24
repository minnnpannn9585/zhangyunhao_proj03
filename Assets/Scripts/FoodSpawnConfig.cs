using UnityEngine;

[CreateAssetMenu(fileName = "FoodSpawnConfig", menuName = "TetrisFood/FoodSpawnConfig")]
public class FoodSpawnConfig : ScriptableObject
{
    public FoodData foodData;       // 要生成的食物
    [Range(1, 100)]
    public int spawnWeight = 10;    // 生成权重（数值越大概率越高）
}