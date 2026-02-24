using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotGridManager : MonoBehaviour
{
    public static PotGridManager Instance; // 单例

    [Header("网格配置")]
    public int gridWidth = 8;          // 网格宽度
    public int gridHeight = 10;        // 网格高度
    public Vector2 gridStartPos;       // 网格起始坐标
    public float cellSize = 1f;        // 单元格大小
    public PotGridCell cellPrefab;     // 单元格预制体

    [Header("食物配置")]
    public List<FoodMatch> foodMatches; // 食物搭配列表
    public FoodBlock foodPrefab;       // 食物预制体
    public List<FoodData> spawnData; // 食物生成配置（带权重）
    public float spawnInterval = 2f;   // 食物生成间隔（秒）
    public float fallSpeed = 1f;       // 食物下落速度（秒/格）

    [Header("调试")]
    public bool isSpawning = true;     // 是否自动生成食物

    private PotGridCell[,] gridCells;  // 网格二维数组
    private List<PotGridCell> allCells = new List<PotGridCell>();
    private List<FoodBlock> fallingFoods = new List<FoodBlock>(); // 正在下落的食物
    private float spawnTimer;          // 生成计时器
    private int totalSpawnWeight;      // 总权重（用于随机生成）

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

        // 初始化网格
        InitGrid();

        // 计算总生成权重
        CalculateTotalSpawnWeight();
    }

    private void Update()
    {
        // 自动生成食物逻辑
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomFood();
                spawnTimer = 0f;
            }
        }

        // 处理食物下落
        UpdateFoodFalling();

        // 鼠标点击检测（保留原有逻辑）
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                PotGridCell cell = hit.collider.GetComponent<PotGridCell>();
                if (cell != null)
                {
                    cell.OnCellClicked();
                }
            }
        }
    }

    // 初始化锅的网格（保留原有逻辑）
    private void InitGrid()
    {
        gridCells = new PotGridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellWorldPos = new Vector2(
                    gridStartPos.x + x * cellSize,
                    gridStartPos.y + y * cellSize
                );

                PotGridCell cell = Instantiate(cellPrefab, cellWorldPos, Quaternion.identity, transform);
                cell.gridPos = new Vector2(x, y);
                cell.name = $"Cell_{x}_{y}";

                gridCells[x, y] = cell;
                allCells.Add(cell);
            }
        }
    }

    // 计算总生成权重
    private void CalculateTotalSpawnWeight()
    {
        totalSpawnWeight = 0;
        foreach (var config in spawnData)
        {
            totalSpawnWeight += config.spawnWeight;
        }
    }

    // 随机选择一种食物（按权重）
    private FoodData GetRandomFood()
    {
        if (spawnData.Count == 0 || totalSpawnWeight == 0)
        {
            Debug.LogWarning("没有配置食物生成列表！");
            return null;
        }

        int randomValue = Random.Range(0, totalSpawnWeight);
        int currentWeight = 0;

        foreach (var config in spawnData)
        {
            currentWeight += config.spawnWeight;
            if (randomValue < currentWeight)
            {
                return config;
            }
        }

        // 兜底返回第一个
        return spawnData[0];
    }

    // 随机生成食物（在顶部随机列）
    public void SpawnRandomFood()
    {
        // 1. 随机选择顶部的列（y坐标为gridHeight-1）
        int randomX = Random.Range(0, gridWidth);
        Vector2 spawnGridPos = new Vector2(randomX, gridHeight - 1);

        // 2. 检测目标位置是否为空
        if (IsValidGridPos(spawnGridPos))
        {
            PotGridCell targetCell = gridCells[(int)spawnGridPos.x, (int)spawnGridPos.y];
            if (targetCell.IsEmpty())
            {
                
                // 3. 随机选择食物类型
                FoodData randomFood = GetRandomFood();
                if (randomFood != null)
                {
                    
                    // 4. 生成食物并加入下落列表
                    FoodBlock newFood = Instantiate(foodPrefab, targetCell.transform.position, Quaternion.identity);
                    newFood.sr.sprite = randomFood.foodSprite;
                    newFood.foodData = randomFood;
                    newFood.currentCookedRate = 0;
                    newFood.fallTimer = 0f; // 初始化下落计时器
                    targetCell.SetFood(newFood);

                    // 加入下落列表
                    fallingFoods.Add(newFood);
                }
            }
            else
            {
                Debug.LogWarning($"生成位置 {spawnGridPos} 已有食物，跳过本次生成");
                // 可选：游戏结束逻辑（顶部堆满）
                // GameOver();
            }
        }
    }

    // 更新所有下落食物的逻辑
    private void UpdateFoodFalling()
    {
        // 倒序遍历，避免移除元素时出错
        for (int i = fallingFoods.Count - 1; i >= 0; i--)
        {
            FoodBlock food = fallingFoods[i];
            if (food == null)
            {
                fallingFoods.RemoveAt(i);
                continue;
            }

            // 更新下落计时器
            food.fallTimer += Time.deltaTime;

            // 计时器达到阈值，尝试下落
            if (food.fallTimer >= fallSpeed)
            {
                food.fallTimer = 0f;
                TryFallFood(food);
            }
        }
    }

    // 尝试让食物下落一格
    private void TryFallFood(FoodBlock food)
    {
        if (food.IsMatched) return; // 搭配物不下落

        // 1. 获取当前格子坐标
        Vector2 currentGridPos = food.CurrentCell.gridPos;
        // 2. 计算下方格子坐标
        Vector2 targetGridPos = new Vector2(currentGridPos.x, currentGridPos.y - 1);

        // 3. 检测下方格子是否有效且为空
        if (IsValidGridPos(targetGridPos))
        {
            PotGridCell targetCell = gridCells[(int)targetGridPos.x, (int)targetGridPos.y];
            if (targetCell.IsEmpty())
            {
                // 4. 执行下落：移除原格子食物，设置到新格子
                food.CurrentCell.RemoveFood();
                targetCell.SetFood(food);
            }
            else
            {
                // 下方有食物，停止下落
                fallingFoods.Remove(food);
            }
        }
        else
        {
            // 超出网格边界（触底），停止下落
            fallingFoods.Remove(food);
        }
    }

    // 从下落列表移除食物（如拾取/合成时）
    public void RemoveFromFallingList(FoodBlock food)
    {
        if (fallingFoods.Contains(food))
        {
            fallingFoods.Remove(food);
        }
    }

    // 保留原有方法...
    public bool IsValidGridPos(Vector2 gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    public List<PotGridCell> GetAdjacentCells(Vector2 gridPos)
    {
        List<PotGridCell> adjacent = new List<PotGridCell>();
        AddCellIfValid(adjacent, new Vector2(gridPos.x, gridPos.y + 1));
        AddCellIfValid(adjacent, new Vector2(gridPos.x, gridPos.y - 1));
        AddCellIfValid(adjacent, new Vector2(gridPos.x - 1, gridPos.y));
        AddCellIfValid(adjacent, new Vector2(gridPos.x + 1, gridPos.y));
        return adjacent;
    }

    private void AddCellIfValid(List<PotGridCell> list, Vector2 gridPos)
    {
        if (IsValidGridPos(gridPos))
        {
            list.Add(gridCells[(int)gridPos.x, (int)gridPos.y]);
        }
    }

    public FoodMatch CheckFoodMatch(FoodData a, FoodData b)
    {
        foreach (var match in foodMatches)
        {
            if ((match.foodA == a && match.foodB == b) || (match.foodA == b && match.foodB == a))
            {
                return match;
            }
        }
        return null;
    }

    public void CreateMatch(FoodBlock a, FoodBlock b, Vector2 targetGridPos)
    {
        PotGridCell targetCell = gridCells[(int)targetGridPos.x, (int)targetGridPos.y];
        b.CurrentCell.RemoveFood();
        // 移除搭配食物的下落状态
        RemoveFromFallingList(a);
        RemoveFromFallingList(b);
        a.IsMatched = true;
        a.sr.sprite = CheckFoodMatch(a.foodData, b.foodData).matchSprite;
    }

    public Sprite GetMatchSprite(FoodData a, FoodData b)
    {
        FoodMatch match = CheckFoodMatch(a, b);
        return match != null ? match.matchSprite : a.foodSprite;
    }

    // 可选：游戏结束逻辑
    public void GameOver()
    {
        isSpawning = false;
        Debug.Log("游戏结束！锅已经堆满了！");
        // 这里可以添加UI提示、分数结算等逻辑
    }
}