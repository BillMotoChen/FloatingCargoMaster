using UnityEngine;
using System.Collections;

public class CargoMover : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動速度
    private Vector2Int currentPos;
    private bool isMoving = false;

    public void Initialize(Vector2Int startPosition)
    {
        currentPos = startPosition;
    }

    public IEnumerator MoveToNextGrid()
    {
        if (!PathManager.Instance.HasPath(currentPos) || isMoving)
            yield break;

        isMoving = true;

        // 取得下一個格子的位置
        Vector2Int nextPos = PathManager.Instance.GetNextPosition(currentPos);
        Vector3 nextWorldPos = BoardManager.Instance.GetGridWorldPosition(nextPos);

        // 平滑移動到下一個格子
        while (Vector3.Distance(transform.position, nextWorldPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, nextWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 確保位置準確
        transform.position = nextWorldPos;

        // 更新當前位置
        currentPos = nextPos;
        isMoving = false;
    }
}