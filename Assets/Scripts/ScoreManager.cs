using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int currentScore = 0;
    public TextMeshProUGUI scoreText; // 分数显示UI

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

    // 添加分数
    public void AddScore(int score)
    {
        currentScore += score;
        UpdateScoreUI();
    }

    // 更新分数显示
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    // 重置分数
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
}