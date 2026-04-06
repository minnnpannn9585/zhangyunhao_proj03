using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "TetrisFood/FoodData")]
public class FoodData : ScriptableObject
{
    public string foodName;          // 食物名称
    public Sprite foodSprite;        // 食物外观    
    public int stageOneScore;        // 阶段一得分
    public int stageTwoScore;        // 阶段二得分
    public int stageThreeScore;      // 阶段三得分
    public int stageFourScore;       // 阶段四得分
    public float DropSpeed;            // 食物下落速度
}