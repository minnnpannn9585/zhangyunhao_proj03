using System.Collections.Generic;
using UnityEngine;

public class PotGridManager : MonoBehaviour
{
    public static PotGridManager Instance;

    [Header("网格配置")] public int gridWidth = 8;
    public int gridHeight = 10;
    public Vector2 gridStartPos;
    public float cellSize = 1f;
    public PotGridCell cellPrefab;

    [Header("食物配置")] public FoodBlock foodPrefab;

    //public List<FoodData> spawnData;
    public float spawnInterval = 2f;
    public float fallSpeed = 1f;

    [Header("食材池配置")] public List<FoodData> allFoodList = new List<FoodData>(); // 全部食材列表 
    public List<FoodData> initialSpawnPool = new List<FoodData>(); // 初始生成池 
    public int spawnPoolMaxSize = 6; //生成池总长度上限 [Header("解锁配置")]
    public List<int> unlockScoreThresholds = new List<int> { 50, 120, 220 };
    public FoodUnlockUI unlockUI;

    private PotGridCell[,] gridCells;
    public List<FoodBlock> fallingFoods = new List<FoodBlock>();
    private float spawnTimer;

    // 按加入顺序维护，便于超长时移除最早食材 
    private readonly List<FoodData> spawnPool = new List<FoodData>();
    private int nextUnlockThresholdIndex = 0;
    private bool isWaitingUnlockChoice = false;
    public IReadOnlyList<FoodData> CurrentSpawnPool => spawnPool;
    private bool isPausedByUnlockUI = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitGrid();
        InitSpawnPool();
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
    
    private void OnDestroy()
    {
        // 防止对象销毁时仍保持暂停 if (isPausedByUnlockUI)
        {
            Time.timeScale =1f;
            isPausedByUnlockUI = false;
        }
    }
    
    private void PauseGameForUnlockUI()
    {
        if (isPausedByUnlockUI) return;
        Time.timeScale =0f;
        isPausedByUnlockUI = true;
    }

    private void ResumeGameFromUnlockUI()
    {
        if (!isPausedByUnlockUI) return;
        Time.timeScale =1f;
        isPausedByUnlockUI = false;
    }

    private void InitSpawnPool()
    {
        spawnPool.Clear();

        // 使用初始生成池 
        foreach (var food in initialSpawnPool)
        {
            AddFoodToSpawnPool(food);
        }

        //兜底：避免池子为空 
        if (spawnPool.Count == 0 && allFoodList.Count > 0)
        {
            AddFoodToSpawnPool(allFoodList[0]);
        }
    }

    public void TryOpenUnlockByScore(int totalScore)
    {
        if (isWaitingUnlockChoice) return;
        if (nextUnlockThresholdIndex >= unlockScoreThresholds.Count) return;
        if (totalScore < unlockScoreThresholds[nextUnlockThresholdIndex]) return;

        List<FoodData> options = BuildUnlockOptions(3);

        // 即使无可选项，也视为该阈值已处理，避免重复触发 
        nextUnlockThresholdIndex++;

        if (options.Count == 0 || unlockUI == null) return;

        isWaitingUnlockChoice = true;
        PauseGameForUnlockUI();
        unlockUI.ShowUnlockOptions(options, OnUnlockSelected);
    }

    private List<FoodData> BuildUnlockOptions(int count)
    {
        List<FoodData> candidates = new List<FoodData>();
        foreach (var food in allFoodList)
        {
            if (food == null) continue;
            if (spawnPool.Contains(food)) continue;
            candidates.Add(food);
        }

        // 打乱后取前 count 个 
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        if (candidates.Count > count)
        {
            candidates.RemoveRange(count, candidates.Count - count);
        }

        return candidates;
    }

    private void OnUnlockSelected(FoodData selectedFood)
    {
        AddFoodToSpawnPool(selectedFood);
        isWaitingUnlockChoice = false;
        ResumeGameFromUnlockUI();
    }

    private void AddFoodToSpawnPool(FoodData food)
    {
        if (food == null) return;
        if (spawnPool.Contains(food)) return;

        spawnPool.Add(food);

        while (spawnPool.Count > spawnPoolMaxSize)
        {
            spawnPool.RemoveAt(0);
                
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
        if (spawnPool.Count ==0) return;

        FoodData randomFoodData = spawnPool[Random.Range(0, spawnPool.Count)];

        FoodBlock newFood = Instantiate(foodPrefab);
        newFood.foodData = randomFoodData;

        SpriteRenderer foodRenderer = newFood.GetComponent<SpriteRenderer>();
        if (foodRenderer != null && randomFoodData.foodSprite != null)
        {
            foodRenderer.sprite = randomFoodData.foodSprite;
        }

        int randomX = Random.Range(0, gridWidth);
        Vector2 spawnPosition = gridStartPos + new Vector2(randomX * cellSize, (gridHeight -1) * cellSize);
        newFood.transform.position = spawnPosition;
        gridCells[randomX, gridHeight -1].SetFood(newFood);

        newFood.fallTimer =0f;
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
