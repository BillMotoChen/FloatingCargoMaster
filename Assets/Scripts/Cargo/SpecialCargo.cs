using UnityEngine;
using System.Collections;

public class SpecialCargo : CargoBase
{
    protected BoardManager boardManager;
    protected StorageManager storageManager;

    public delegate void SpecialCargoClickedHandler(SpecialCargo specialCargo);
    public static event SpecialCargoClickedHandler OnSpecialCargoClicked;

    protected virtual void Awake()
    {
        boardManager = BoardManager.Instance;
        storageManager = StorageManager.Instance;
    }

    public override void MoveToNextPosition()
    {
        Vector2Int nextPos = PathManager.Instance.GetNextPosition(position);
        Vector3 nextWorldPos = BoardManager.Instance.GetGridWorldPosition(nextPos);

        StartCoroutine(MoveAndUpdateGrid(nextWorldPos, nextPos));
    }

    private void OnMouseDown()
    {
        if (!IsClickable()) return;
        OnSpecialCargoClicked?.Invoke(this);

        Destroy(gameObject);
    }

    private bool IsClickable()
    {
        return BoardManager.Instance.IsCargoClickable(position);
    }
}