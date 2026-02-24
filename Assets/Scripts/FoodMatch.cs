using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "FoodMatch", menuName = "TetrisFood/FoodMatch")]
public class FoodMatch : ScriptableObject
{
    public FoodData foodA;           // 搭配食物A
    public FoodData foodB;           // 搭配食物B
    public int matchScore;           // 搭配得分
    public Sprite matchSprite;       // 搭配后外观
}