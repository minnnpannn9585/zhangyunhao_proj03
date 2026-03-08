using System.Text;
using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public TextMeshProUGUI scoreText; // 显示玩家得分
    public TextMeshProUGUI foodInfoText; // 显示场上所有食物及成熟度
    
    private int totalScore = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int score)
    {
        totalScore += score;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {totalScore}";
    }

    

    public void UpdateFoodInfo()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var food in FindObjectsOfType<FoodBlock>())
        {
            sb.AppendLine($"{food.foodData.foodName}: maturity {food.maturity}");
        }
        foodInfoText.text = sb.ToString();
    }
}