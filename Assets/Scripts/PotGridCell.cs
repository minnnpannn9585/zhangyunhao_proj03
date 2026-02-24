using UnityEngine;

public class PotGridCell : MonoBehaviour
{
    [Header("烹饪属性")]
    public float cookSpeed = 1f;     // 该格子的烹饪速度
    public Vector2 gridPos;          // 格子在网格中的坐标（x,y）

    private FoodBlock currentFood;   // 当前格子中的食物
    private SpriteRenderer sr;       // 格子渲染器（可选，用于显示格子状态）

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // 设置格子中的食物
    public void SetFood(FoodBlock food)
    {
        currentFood = food;
        if (currentFood != null)
        {
            currentFood.transform.position = transform.position;
            currentFood.CurrentCell = this;
        }
    }

    // 移除格子中的食物
    public void RemoveFood()
    {
        currentFood = null;
    }

    // 每帧更新烹饪逻辑
    private void Update()
    {
        if (currentFood != null && !currentFood.IsMatched)
        {
            currentFood.UpdateCookedRate(cookSpeed * Time.deltaTime);
        }
    }

    // 获取当前格子的食物
    public FoodBlock GetCurrentFood() => currentFood;

    // 检测点击（可挂载Collider2D，通过射线检测触发）
    public void OnCellClicked()
    {
        if (currentFood != null)
        {
            currentFood.OnFoodClicked();
        }
    }

    // 格子是否为空
    public bool IsEmpty() => currentFood == null;
}