using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance;

    // 存放每個格子的下一個方向
    private Dictionary<Vector2Int, Vector2Int> pathMap = new Dictionary<Vector2Int, Vector2Int>();

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
    }

    /// <summary>
    /// 設定某個格子的「下一步」位置
    /// </summary>
    public void DefinePath(Vector2Int from, Vector2Int to)
    {
        pathMap[from] = to;
    }

    /// <summary>
    /// 獲取下一個位置，如果沒有設定則返回原地
    /// </summary>
    public Vector2Int GetNextPosition(Vector2Int currentPos)
    {
        return pathMap.ContainsKey(currentPos) ? pathMap[currentPos] : currentPos;
    }

    /// <summary>
    /// 判斷某個格子是否有定義路線
    /// </summary>
    public bool HasPath(Vector2Int position)
    {
        return pathMap.ContainsKey(position);
    }
}