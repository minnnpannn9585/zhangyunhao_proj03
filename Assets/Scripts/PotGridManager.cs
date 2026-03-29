using System.Collections.Generic;
using UnityEngine;

public class PotGridManager : MonoBehaviour
{
    public static PotGridManager Instance;

    [Header("网格配置")]
    public int gridWidth = 8;
    public int gridHeight = 10;
    public Vector2 gridStartPos;
    public float cellSize = 1f;
    public PotGridCell cellPrefab;

    [Header("食物配置")]
    public FoodBlock foodPrefab;
    public List<FoodData> spawnData;
    public float spawnInterval = 2f;
    public float fallSpeed = 1f;

    private PotGridCell[,] gridCells;
    public List<FoodBlock> fallingFoods = new List<FoodBlock>();
    private float spawnTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitGrid();
    }

    private void Start()
    {
        SpawnFood(); // 在游戏开始时生成第一个食物
    }

    private void Update()
    {
        HandleFoodFalling();
        HandleMouseClick();
        HandlePlayerInput();

        // 检查是否需要生成新的食物
        if (fallingFoods.Count == 0)
        {
            SpawnFood();
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FoodBlock clickedFood = GetClickedFood();
            clickedFood?.OnFoodClicked();
        }
    }

    private FoodBlock GetClickedFood()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        return hit.collider?.GetComponent<FoodBlock>();
    }

    private void HandlePlayerInput()
    {
        if (fallingFoods.Count == 0) return;

        FoodBlock currentFood = fallingFoods[fallingFoods.Count - 1];
        int direction = 0;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) direction = -1;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) direction = 1;

        if (direction != 0) currentFood.MoveHorizontal(direction);
    }

    private void InitGrid()
    {
        gridCells = new PotGridCell[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellWorldPos = gridStartPos + new Vector2(x * cellSize, y * cellSize);
                PotGridCell cell = Instantiate(cellPrefab, cellWorldPos, Quaternion.identity, transform);
                cell.gridPos = new Vector2(x, y);
                if (x == 0 || x == gridWidth - 1 || y == 0)
                {
                    cell.cookSpeed = 2;
                }
                else
                {
                    cell.cookSpeed = 1;
                }
                    gridCells[x, y] = cell;
            }
        }
    }

    private void SpawnFood()
    {
        if (spawnData.Count == 0) return;

        // 随机选择一个食物数据
        FoodData randomFoodData = spawnData[Random.Range(0, spawnData.Count)];

        // 创建食物实例
        FoodBlock newFood = Instantiate(foodPrefab);
        newFood.foodData = randomFoodData;

        // 设置食物的外观
        SpriteRenderer foodRenderer = newFood.GetComponent<SpriteRenderer>();
        if (foodRenderer != null && randomFoodData.foodSprite != null)
        {
            foodRenderer.sprite = randomFoodData.foodSprite;
        }

        // 设置食物初始位置
        int randomX = Random.Range(0, gridWidth);
        Vector2 spawnPosition = gridStartPos + new Vector2(randomX * cellSize, (gridHeight -1)  * cellSize);
        newFood.transform.position = spawnPosition;
        gridCells[randomX, gridHeight-1].SetFood(newFood);

        // 设置食物为正在下落状态
        newFood.fallTimer = 0f;
        fallingFoods.Add(newFood);
    }

    private void HandleFoodFalling()
    {
        for (int i = fallingFoods.Count - 1; i >= 0; i--)
        {
            FoodBlock food = fallingFoods[i];
            if (food == null || !TryFallFood(food))
            {
                fallingFoods.RemoveAt(i);
            }
        }
    }

    private bool TryFallFood(FoodBlock food)
    {
        food.fallTimer += Time.deltaTime;
        if (food.fallTimer < fallSpeed) return true;

        food.fallTimer = 0f;
        Vector2 targetGridPos = food.CurrentCell.gridPos + Vector2.down;

        if (IsValidGridPos(targetGridPos) && gridCells[(int)targetGridPos.x, (int)targetGridPos.y].IsEmpty())
        {
            if (food.CurrentCell != null )
            {
                food.CurrentCell.RemoveFood();
            }
            gridCells[(int)targetGridPos.x, (int)targetGridPos.y].SetFood(food);
            return true;
        }

        return false;
    }

    public bool IsValidGridPos(Vector2 gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    public PotGridCell GetCellAtPosition(Vector2 gridPos)
    {
        return gridCells[(int)gridPos.x, (int)gridPos.y];
    }
}
