using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    private int width, height; // Board 大小

    [Header("prefab")]
    public GameObject gridPrefab; // GridCell 的 prefab
    public GameObject exporterPrefab; // 出貨口的 prefab
    public GameObject cargoPrefab;

    public Transform boardContainer; // 存放 board 的父物件
    public GridCell[,] gridCells; // 存放所有的格子

    public List<CargoBase> cargos = new List<CargoBase>();
    public List<CargoMover> cargoMovers = new List<CargoMover>(); // 存放所有貨物的移動腳本
    public bool isMoving { get; private set; } = false;

    public PathData pathData;

    public float marginRatio = 0.05f; // Board 與螢幕邊緣的空白比例 (5%)
    public float cellSpacingRatio = 0.25f; // 格子之間的間距比例 (25%)

    private LevelData currentLevel;
    private List<Vector2Int> exporterPositions = new List<Vector2Int>();
    private List<Exporter> exporters;

private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LevelVariablesInit();
        pathData = LoadPathData();
        exporters = LevelLoader.Instance.GetCurrentLevel().exporters;
        foreach (Exporter e in exporters)
        {
            exporterPositions.Add(e.cor);
        }
        InitializeBoard();
        DefinePaths();
        SpawnAfterMove();
    }

    private void LevelVariablesInit()
    {
        LevelLoader.Instance.LoadLevel(1);
        currentLevel = LevelLoader.Instance.GetCurrentLevel();
        width = currentLevel.boardWidth;
        height = currentLevel.boardHeight;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            StartCoroutine(MoveAllCargos());
            isMoving = true;
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
        float boardStartY = screenHeight * 0.56f;

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

        // 取得exporter座標
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                GameObject cellObj;
                if (exporterPositions.Contains(cellPos))
                {
                    cellObj = Instantiate(exporterPrefab, boardContainer); // ✅ Use exporterPrefab
                }
                else
                {
                    cellObj = Instantiate(gridPrefab, boardContainer); // ✅ Use gridPrefab
                }
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

    public Vector3 GetGridWorldPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return Vector3.zero;

        return gridCells[position.x, position.y].cellObject.transform.position;
    }

    private IEnumerator MoveAllCargos()
    {
        List<Coroutine> activeCoroutines = new List<Coroutine>();

        foreach (CargoBase cargo in cargos)
        {
            if (cargo != null)
            {
                Coroutine coroutine = StartCoroutine(MoveAndContinue(cargo));
                activeCoroutines.Add(coroutine);
            }
        }

        // Wait for all coroutines to finish
        foreach (Coroutine coroutine in activeCoroutines)
        {
            yield return coroutine;
        }

        // all movement coroutines are done, spawn after move
        isMoving = false;
        SpawnAfterMove();
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

    public void SpawnAfterMove()
    {
        foreach (Vector2Int v in exporterPositions)
        {
            if (!gridCells[v.x, v.y].HasCargo())
            {
                SpawnCargoAt(v, cargoPrefab);
            }
        }  
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
        PathData data = new PathData();
        CycleData currentCycle = LevelLoader.Instance.CurrentLevelCycle;
        if (currentLevel == null)
        {
            Debug.LogError("❌ No level is currently loaded!");
            return data;
        }

        // Loop through each cycle and add paths
        foreach (var cycleWrapper in currentCycle.cycles)
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
}