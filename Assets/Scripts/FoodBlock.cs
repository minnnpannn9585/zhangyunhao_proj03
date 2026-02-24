using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FoodBlock : MonoBehaviour
{
    [Header("食物基础属性")]
    public FoodData foodData;        // 食物数据配置
    public float currentCookedRate;  // 当前熟度（0-100）
    public bool isMatched = false;   // 是否是已合成的搭配物
    public FoodBlock matchedPartner; // 搭配的伙伴（合成前）
    public PotGridCell currentCell;  // 当前所在的网格单元格

    [Header("下落属性")]
    public float fallTimer;          // 下落计时器（新增）

    public SpriteRenderer sr;       // 食物渲染器
    private Collider2D col;          // 碰撞体
    private bool isPickable = true;  // 是否可拾取

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (foodData != null)
        {
            sr.sprite = foodData.foodSprite;
        }
    }

    // 更新熟度（保留原有逻辑）
    public void UpdateCookedRate(float cookDelta)
    {
        if (isMatched) return;
        currentCookedRate = Mathf.Clamp(currentCookedRate + cookDelta * 10, 0, 100);
    }

    // 食物被点击时的逻辑（修改：拾取时移除下落状态）
    public void OnFoodClicked()
    {
        if (!isPickable) return;

        if (isMatched)
        {
            HarvestMatchedFood();
        }
        else
        {
            FoodBlock matchFood = CheckAdjacentMatch();
            if (matchFood != null)
            {
                CreateFoodMatch(matchFood);
            }
            else
            {
                // 拾取单个食物时，移除下落状态
                PotGridManager.Instance.RemoveFromFallingList(this);
                HarvestSingleFood();
            }
        }
    }

    // 检测相邻可搭配的食物（保留原有逻辑）
    private FoodBlock CheckAdjacentMatch()
    {
        PotGridManager gridManager = PotGridManager.Instance;
        if (gridManager == null) return null;

        List<PotGridCell> adjacentCells = gridManager.GetAdjacentCells(currentCell.gridPos);
        foreach (var cell in adjacentCells)
        {
            FoodBlock adjacentFood = cell.GetCurrentFood();
            if (adjacentFood != null && !adjacentFood.isMatched)
            {
                FoodMatch match = gridManager.CheckFoodMatch(foodData, adjacentFood.foodData);
                if (match != null)
                {
                    return adjacentFood;
                }
            }
        }
        return null;
    }

    // 创建食物搭配（保留原有逻辑）
    private void CreateFoodMatch(FoodBlock partner)
    {
        PotGridManager.Instance.CreateMatch(this, partner, currentCell.gridPos);
        matchedPartner = partner;
    }

    // 收获单个食物（保留原有逻辑）
    private void HarvestSingleFood()
    {
        float rateDiff = Mathf.Abs(currentCookedRate - foodData.bestCookedRate);
        float scoreRatio = Mathf.Clamp01(1 - rateDiff / 100);
        int finalScore = Mathf.RoundToInt(foodData.baseScore * scoreRatio * foodData.scoreWeight);

        ScoreManager.Instance.AddScore(finalScore);

        currentCell.RemoveFood();
        Destroy(gameObject);
    }

    // 收获搭配食物（保留原有逻辑）
    private void HarvestMatchedFood()
    {
        FoodMatch match = PotGridManager.Instance.CheckFoodMatch(foodData, matchedPartner.foodData);
        int matchScore = match != null ? match.matchScore : 0;

        ScoreManager.Instance.AddScore(matchScore);

        currentCell.RemoveFood();
        Destroy(matchedPartner.gameObject);
        Destroy(gameObject);
    }

    // 外部访问的属性（保留）
    public bool IsMatched { get => isMatched; set => isMatched = value; }
    public PotGridCell CurrentCell { get => currentCell; set => currentCell = value; }
}