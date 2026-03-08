using System.Collections.Generic;
using UnityEngine;

public class FoodBlock : MonoBehaviour
{
    [Header("食物基础属性")]
    public FoodData foodData;        // 食物数据配置
    public float currentCookedRate;  // 当前熟度（0-100）
    public PotGridCell currentCell;  // 当前所在的网格单元格

    private SpriteRenderer sr;       // 食物渲染器
    private Collider2D col;          // 碰撞体

    [Header("成熟度属性")]
    public int maturity; // 当前成熟度（0-10）
    public float maturityTimer; // 成熟度计时器
    public float maturityIncreaseInterval = 1f; // 每次成熟度增加的时间间隔

    public float fallTimer; // 用于控制食物下落的计时器

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (foodData != null)
        {
            sr.sprite = foodData.foodSprite;
        }
    }

    private void Update()
    {
        // 确保食物落地后才更新成熟度
        if (CanIncreaseMaturity())
        {
            maturityTimer += Time.deltaTime;
            if (maturityTimer >= maturityIncreaseInterval && maturity < 10)
            {
                maturity++;
                maturityTimer = 0f;
                GameUIManager.Instance.UpdateFoodInfo(); // 更新场上食物信息
            }
        }
    }

    private bool CanIncreaseMaturity()
    {
        return currentCell != null && !PotGridManager.Instance.fallingFoods.Contains(this);
    }

    public void MoveHorizontal(int direction)
    {
        if (currentCell == null) return;

        // 计算目标格子位置
        Vector2 targetGridPos = new Vector2(currentCell.gridPos.x + direction, currentCell.gridPos.y);

        // 检测目标格子是否有效且为空
        if (PotGridManager.Instance.IsValidGridPos(targetGridPos))
        {
            PotGridCell targetCell = PotGridManager.Instance.GetCellAtPosition(targetGridPos);
            if (targetCell != null && targetCell.IsEmpty())
            {
                // 移动到目标格子
                currentCell.RemoveFood();
                targetCell.SetFood(this);
            }
        }
    }

    public void OnFoodClicked()
    {
        if (currentCell == null) return;
        
        
        GameUIManager.Instance.AddScore(maturity);

        currentCell.RemoveFood();
        Destroy(gameObject);
        GameUIManager.Instance.UpdateFoodInfo();
    }
    

    public PotGridCell CurrentCell { get => currentCell; set => currentCell = value; }
}