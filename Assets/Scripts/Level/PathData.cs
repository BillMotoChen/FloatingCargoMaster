using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathData
{
    public Dictionary<Vector2Int, Vector2Int> pathMap = new Dictionary<Vector2Int, Vector2Int>(); // 格子路徑
    public Dictionary<Vector2Int, bool> exporters = new Dictionary<Vector2Int, bool>(); // 記錄哪些格子是出口

    public PathData() { }

    /// <summary>
    /// 設定某個格子的「下一步」位置
    /// </summary>
    public void AddPath(Vector2Int from, Vector2Int to)
    {
        pathMap[from] = to;
    }

    /// <summary>
    /// 設定某個格子是否是出貨口
    /// </summary>
    public void AddExporter(Vector2Int position)
    {
        exporters[position] = true;
    }
}