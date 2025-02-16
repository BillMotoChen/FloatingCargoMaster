using UnityEngine;
using System.Collections;

public abstract class CargoBase : MonoBehaviour
{
    public Vector2Int position; // Current position in the grid
    private float moveSpeed = 10f; // ✅ Reintroduced move speed

    public void Initialize(Vector2Int startPosition)
    {
        position = startPosition;
        transform.position = BoardManager.Instance.GetGridWorldPosition(startPosition);

        // ✅ Set cargo into the grid at spawn time
        GridCell startCell = BoardManager.Instance.gridCells[startPosition.x, startPosition.y];
        startCell.SetCargo(this);
    }

    public abstract void MoveToNextPosition();

    /// <summary>
    /// Handles cargo movement, updating its parent and updating grid state.
    /// </summary>
    protected IEnumerator MoveAndUpdateGrid(Vector3 targetPos, Vector2Int nextGridPos)
    {
        GridCell currentCell = BoardManager.Instance.gridCells[position.x, position.y];
        GridCell nextCell = BoardManager.Instance.gridCells[nextGridPos.x, nextGridPos.y];

        // ✅ Clear old cell before moving
        currentCell.ClearCargo();

        yield return StartCoroutine(MoveAnimation(targetPos));

        // ✅ Update Parent to the new GridCell
        transform.SetParent(nextCell.cellObject.transform);
        transform.localPosition = Vector3.zero;

        // ✅ Update Cargo's Position
        position = nextGridPos;

        // ✅ Set Cargo in the new GridCell
        nextCell.SetCargo(this);
    }

    /// <summary>
    /// Smooth movement animation using moveSpeed
    /// </summary>
    protected virtual IEnumerator MoveAnimation(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed; // ✅ Speed-based movement

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; // ✅ Ensure it reaches the target
    }
}