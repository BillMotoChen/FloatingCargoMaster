using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    private int width, height; // Board 大小

    [Header("prefab")]
    public GameObject gridPrefab; // GridCell 的 prefab
    public GameObject exporterPrefab; // 出貨口的 prefab
    public GameObject[] cargoPrefabs;
    public GameObject[] specialCargoPrefabs;

    public Transform boardContainer; // 存放 board 的父物件
    public GridCell[,] gridCells; // 存放所有的格子

    public List<CargoBase> cargos = new List<CargoBase>();
    public List<CargoMover> cargoMovers = new List<CargoMover>(); // 存放所有貨物的移動腳本
    public bool isMoving { get; private set; } = false;
    public int stopRotate;

    // item
    public bool itemInUsed = false;
    public bool isFreeClickEnabled = false;

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

    private void OnEnable()
    {
        NormalCargo.OnNormalCargoClicked += HandleMoveAllCargos;
        SpecialCargo.OnSpecialCargoClicked += HandleSpecialCargoClick;
    }

    private void OnDisable()
    {
        NormalCargo.OnNormalCargoClicked -= HandleMoveAllCargos;
        SpecialCargo.OnSpecialCargoClicked -= HandleSpecialCargoClick;
    }

    private void LevelVariablesInit()
    {
        LevelLoader.Instance.LoadLevel(PlayerData.stage);
        currentLevel = LevelLoader.Instance.GetCurrentLevel();
        width = currentLevel.boardWidth;
        height = currentLevel.boardHeight;
        stopRotate = 0;
    }

    private void Update()
    {
    }

    private void InitializeBoard()
    {
        gridCells = new GridCell[width, height];

        // 取得螢幕總寬高
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 設定 Board 的 Y 軸範圍 (佔畫面 30%，從 21% 開始)
        float boardHeight = screenHeight * 0.3f;
        float boardStartY = screenHeight * 0.5f;

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

    private void HandleMoveAllCargos(NormalCargo cargo)
    {
        StartCoroutine(MoveAllCargos());
        isMoving = true;
    }

    private void HandleSpecialCargoClick(SpecialCargo specialCargo)
    {
        if (specialCargo is ISpecialCargoEffect effect)
        {
            effect.ApplyEffect();
            StartCoroutine(MoveAllCargos());
        }
    }

    private IEnumerator MoveAllCargos()
    {
        if (stopRotate > 0)
        {
            StartCoroutine(SkipMovementWithUpdate());            
            yield break;
        }

        List<Coroutine> activeCoroutines = new List<Coroutine>();

        foreach (CargoBase cargo in cargos)
        {
            if (cargo != null && cargo.gameObject.activeInHierarchy)
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
        UpdateCargoAlphaBasedOnClickability();
    }

    private IEnumerator MoveAndContinue(CargoBase cargo)
    {
        cargo.MoveToNextPosition();
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator SkipMovementWithUpdate()
    {
        stopRotate = Mathf.Max(0, stopRotate - 1);

        yield return null;

        SpawnAfterMove();
        UpdateCargoAlphaBasedOnClickability();
        isMoving = false;
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
                GameObject cargoToSpawn;

                // 10% chance to spawn special cargo
                if (currentLevel.specialCargo.Count > 0 && Random.value < 0.1f)
                {
                    int specialIndex = currentLevel.specialCargo[Random.Range(0, currentLevel.specialCargo.Count)];
                    Debug.Log(stopRotate + " " + specialIndex);
                    if(stopRotate > 0 && specialIndex == 0)
                    {
                        cargoToSpawn = cargoPrefabs[Random.Range(0, currentLevel.color)];
                    }
                    else
                    {
                        cargoToSpawn = specialCargoPrefabs[specialIndex];
                    }
                }
                else
                {
                    cargoToSpawn = cargoPrefabs[Random.Range(0, currentLevel.color)];
                }

                SpawnCargoAt(v, cargoToSpawn);
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
        return data;
    }

    public bool IsCargoClickable(Vector2Int currentPos)
    {
        for (int y = currentPos.y - 1; y >= 0; y--)
        {
            Vector2Int belowPos = new Vector2Int(currentPos.x, y);
            if (gridCells[belowPos.x, belowPos.y].HasCargo())
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateCargoAlphaBasedOnClickability()
    {
        ToggleAfterFreeClickUsed();
        foreach (CargoBase cargo in cargos)
        {
            if (cargo == null) continue;

            bool isClickable = IsCargoClickable(cargo.position);
            SetCargoAlphaAndScale(cargo, isClickable ? 1f : 0.45f, isClickable ? 0.2f : 0.15f);
        }
    }

    /// <summary>
    /// Sets the alpha transparency and scale of the cargo based on its clickability.
    /// </summary>
    private void SetCargoAlphaAndScale(CargoBase cargo, float alpha, float scale)
    {
        // ✅ Update Alpha Transparency
        SpriteRenderer spriteRenderer = cargo.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        // ✅ Update Scale
        cargo.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void CleanupCargos()
    {
        cargos = cargos.Where(cargo => cargo != null).ToList();
    }

    // ITEM

    public void EnableFreeClick()
    {
        isFreeClickEnabled = true;
        foreach (CargoBase cargo in cargos)
        {
            if (cargo == null) continue;

            // ✅ 允許點擊
            Collider2D collider = cargo.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            SpriteRenderer renderer = cargo.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 1f;
                renderer.color = color;
            }

            // ✅ scale 調整為 (0.2, 0.2, 0.2)
            cargo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        Debug.Log("🟢 Free Click Mode Enabled!");
    }

    private void ToggleAfterFreeClickUsed()
    {
        isFreeClickEnabled = false;
        itemInUsed = false;
    }
}