using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int width, height; // Board 大小
    public GameObject gridPrefab; // GridCell 的 prefab
    public GameObject exporterPrefab; // 出貨口的 prefab
    public GameObject cargoPrefab;
    public Transform boardContainer; // 存放 board 的父物件
    public Transform exporterContainer;
    public GridCell[,] gridCells; // 存放所有的格子
    public GameObject[] exporters; // 存放出貨口
    public List<CargoBase> cargos = new List<CargoBase>();
    public List<CargoMover> cargoMovers = new List<CargoMover>(); // 存放所有貨物的移動腳本
    public List<SpawnPoint> spawnPoints;

    public PathData pathData;

    public float marginRatio = 0.05f; // Board 與螢幕邊緣的空白比例 (5%)
    public float cellSpacingRatio = 0.25f; // 格子之間的間距比例 (25%)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LevelLoader.Instance.LoadLevel(1);
        PrintCurrentLevelData();
        pathData = LoadPathData();
        InitializeBoard();
        DefinePaths();
        CreateExporters();
        SpawnCargoAt(new Vector2Int(0, 0), cargoPrefab);
        SpawnCargoAt(new Vector2Int(1, 0), cargoPrefab);
        SpawnCargoAt(new Vector2Int(2, 0), cargoPrefab);
        SpawnCargoAt(new Vector2Int(3, 1), cargoPrefab);
        SpawnCargoAt(new Vector2Int(2, 1), cargoPrefab);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (GridCell grid in gridCells)
            {
                Debug.Log(grid.position);
                Debug.Log(grid.HasCargo());
            }
            MoveAllCargos();
        }
    }

    private void InitializeBoard()
    {
        gridCells = new GridCell[width, height];

        // 取得螢幕總寬高
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 設定 Board 的 Y 軸範圍 (佔畫面 30%，從 21% 開始)
        float boardHeight = screenHeight * 0.3f;
        float boardStartY = screenHeight * 0.46f;

        // 轉換螢幕座標為世界座標
        Vector3 worldTop = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth / 2, boardStartY + boardHeight, 0));
        Vector3 worldBottom = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth / 2, boardStartY, 0));

        // 計算 Board 的世界座標高度
        float worldBoardHeight = worldTop.y - worldBottom.y;

        // 設定 Board 與螢幕邊緣的留白
        float margin = worldBoardHeight * marginRatio;

        // 計算可用範圍（扣掉邊界）
        float usableHeight = worldBoardHeight - (2 * margin);
        float usableWidth = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0, 0)).x * 2 - (2 * margin);

        // 計算 Cell 間隔
        float cellSpacing = cellSpacingRatio * usableHeight / height;

        // 計算實際 Cell 大小
        float cellSize = Mathf.Min((usableWidth - (width - 1) * cellSpacing) / width,
                                   (usableHeight - (height - 1) * cellSpacing) / height);

        // 計算 Board 的世界寬度
        float worldBoardWidth = cellSize * width + cellSpacing * (width - 1);

        // 計算 Board 置中
        float boardStartX = -((width - 1) / 2.0f) * (cellSize + cellSpacing);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 生成 GridCell 的物件
                GameObject cellObj = Instantiate(gridPrefab, boardContainer);
                cellObj.transform.position = new Vector3(
                    boardStartX + x * (cellSize + cellSpacing),
                    worldBottom.y + margin + y * (cellSize + cellSpacing),
                    0
                );
                cellObj.transform.localScale = Vector3.one * (cellSize * 0.9f); // 稍微縮小，避免格子過滿

                // 將這個 Cell 存到 `gridCells` 陣列
                gridCells[x, y] = new GridCell(new Vector2Int(x, y), cellObj);

                // **檢查這個格子內是否有 Cargo**
                Transform cargo = cellObj.transform.Find("Cargo");
                if (cargo != null)
                {
                    CargoMover mover = cargo.gameObject.AddComponent<CargoMover>();
                    mover.Initialize(new Vector2Int(x, y));
                    cargoMovers.Add(mover); // 存入列表，方便統一控制
                }
            }
        }
    }

    private void CreateExporters()
    {
        if (width < 2) return; // 確保至少有兩個格子，才能有左右出貨口

        exporters = new GameObject[2]; // 兩個出貨口

        float cellSize = gridCells[0, 0].cellObject.transform.localScale.x; // 取得格子大小
        float cellSpacing = cellSize * cellSpacingRatio; // 取得間距大小

        // 找到最左邊和最右邊的 `GridCell`（最上面一排）
        Vector3 leftGridPos = gridCells[0, height - 1].cellObject.transform.position;
        Vector3 rightGridPos = gridCells[width - 1, height - 1].cellObject.transform.position;

        // 設定出貨口的高度（放在 GridCell 上方）
        float exporterY = leftGridPos.y + (cellSize + cellSpacing);

        // 生成左側出貨口
        exporters[0] = Instantiate(exporterPrefab, exporterContainer);
        exporters[0].transform.position = new Vector3(leftGridPos.x, exporterY, 0);
        exporters[0].transform.localScale = Vector3.one * (cellSize * 0.8f); // 讓出貨口稍微小一點

        // 生成右側出貨口
        exporters[1] = Instantiate(exporterPrefab, exporterContainer);
        exporters[1].transform.position = new Vector3(rightGridPos.x, exporterY, 0);
        exporters[1].transform.localScale = Vector3.one * (cellSize * 0.8f);
    }

    public Vector3 GetGridWorldPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return Vector3.zero;

        return gridCells[position.x, position.y].cellObject.transform.position;
    }

    private void MoveAllCargos()
    {
        foreach (CargoBase cargo in cargos)
        {
            if (cargo != null)
            {
                StartCoroutine(MoveAndContinue(cargo));
            }
        }
    }

    private IEnumerator MoveAndContinue(CargoBase cargo)
    {
        cargo.MoveToNextPosition(); // 移動 Cargo
        yield return new WaitForSeconds(0.1f); // 等待一點時間，確保平滑移動
    }

    public void SpawnCargoAt(Vector2Int position, GameObject cargoPrefab)
    {
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return;

        GridCell cell = gridCells[position.x, position.y];

        // 如果格子內已經有 Cargo，先刪除舊的
        if (cell.HasCargo())
        {
            GameObject.Destroy(cell.cargo.gameObject);
            cell.ClearCargo();
        }

        // 生成新的 Cargo
        GameObject newCargo = Instantiate(cargoPrefab, cell.cellObject.transform);
        newCargo.transform.position = cell.cellObject.transform.position;

        // **確保 CargoBase 被初始化**
        CargoBase cargo = newCargo.GetComponent<CargoBase>();
        if (cargo != null)
        {
            cargo.Initialize(position);
            cargos.Add(cargo); // **加入 cargos 清單，確保 Space 按下時可以觸發移動**
        }

        cell.SetCargo(cargo);
    }

    private void MoveCargo(Vector2Int from, Vector2Int to)
    {
        if (gridCells[from.x, from.y].HasCargo())
        {
            CargoBase cargo = gridCells[from.x, from.y].cargo;
            gridCells[from.x, from.y].ClearCargo();

            cargo.transform.position = gridCells[to.x, to.y].cellObject.transform.position; // ✅ Move cargo
            gridCells[to.x, to.y].SetCargo(cargo);
        }
    }

    private void DefinePaths()
    {
        if (pathData == null)
        {
            Debug.LogError("LevelData is null!");
            return;
        }

        foreach (var entry in pathData.pathMap)
        {
            Vector2Int from = entry.Key;
            Vector2Int to = entry.Value;

            PathManager.Instance.DefinePath(from, to);

            // **設定箭頭方向**
            if (gridCells[from.x, from.y] != null)
            {
                gridCells[from.x, from.y].SetArrowDirection(to);
            }
        }
    }

    private PathData LoadPathData()
    {
        //    data.AddPath(new Vector2Int(0, 1), new Vector2Int(0, 0));
        PathData data = new PathData();

        LevelData currentLevel = LevelLoader.Instance.GetCurrentLevel();
        if (currentLevel == null)
        {
            Debug.LogError("❌ No level is currently loaded!");
            return data;
        }

        if (currentLevel.cycles == null || currentLevel.cycles.Count == 0)
        {
            Debug.LogWarning("⚠️ No cycles found in the current level!");
            return data;
        }

        // Loop through each cycle and add paths
        foreach (var cycleWrapper in currentLevel.cycles)
        {
            if (cycleWrapper == null || cycleWrapper.points == null || cycleWrapper.points.Count == 0)
            {
                Debug.LogWarning("⚠️ Skipping an empty or null cycle.");
                continue;
            }

            List<Vector2Int> cycle = cycleWrapper.points;

            for (int i = 0; i < cycle.Count; i++)
            {
                Vector2Int from = cycle[i];
                Vector2Int to = cycle[(i + 1) % cycle.Count];

                data.AddPath(from, to);
            }
        }

        Debug.Log("✅ PathData loaded from level cycles");
        return data;
    }

    // FOR TEST
    public void PrintCurrentLevelData()
    {
        LevelData currentLevel = LevelLoader.Instance.GetCurrentLevel();

        if (currentLevel == null)
        {
            Debug.LogError("❌ No level is currently loaded!");
            return;
        }

        Debug.Log("📌 --- Level Data ---");
        Debug.Log($"🔹 Level: {currentLevel.level}");
        Debug.Log($"📏 Board Size: {currentLevel.boardWidth} x {currentLevel.boardHeight}");
        Debug.Log($"🎮 Game Mode: {currentLevel.gameMode}");
        Debug.Log($"🎨 Color: {currentLevel.color}");
        Debug.Log($"📦 Special Cargo: {string.Join(", ", currentLevel.specialCargo)}");
        Debug.Log($"🎯 Required Sets: {string.Join(", ", currentLevel.requiredSets)}");

        // Print Cycles
        Debug.Log(JsonUtility.ToJson(currentLevel, true));
        Debug.Log("🔄 Cycles:");
        for (int i = 0; i < currentLevel.cycles.Count; i++)
        {
            if (currentLevel.cycles[i] == null || currentLevel.cycles[i].points == null)
            {
                Debug.LogWarning($"⚠️ Cycle {i + 1} is null or has no points!");
                continue;
            }

            string cycleStr = $"Cycle {i + 1}: ";
            foreach (var point in currentLevel.cycles[i].points)
            {
                cycleStr += $"[{point.x},{point.y}] "; // ✅ Use `point.x` and `point.y`
            }
            Debug.Log(cycleStr);
        }

        Debug.Log("✅ Level Data Printed Successfully!");
    }
}