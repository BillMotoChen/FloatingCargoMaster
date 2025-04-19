using UnityEngine;
using System.Collections;
using System;

public class NormalCargo : CargoBase
{
    public delegate void NormalCargoClickedHandler(NormalCargo normalCargo);
    public static event NormalCargoClickedHandler OnNormalCargoClicked;

    public int cargoId;

    private void Start()
    {
        AssignIdBasedOnName();
    }

    public override void MoveToNextPosition()
    {
        Vector2Int nextPos = PathManager.Instance.GetNextPosition(position);
        Vector3 nextWorldPos = BoardManager.Instance.GetGridWorldPosition(nextPos);

        StartCoroutine(MoveAndUpdateGrid(nextWorldPos, nextPos));
    }

    private void OnMouseDown()
    {
        if (BoardManager.Instance.isMoving || !StorageManager.Instance.clickable) return;
        if (!IsClickable()) return;

        OnNormalCargoClicked?.Invoke(this);
        Destroy(gameObject);
    }

    public void AssignIdBasedOnName()
    {
        string objName = gameObject.name;

        switch (objName.Replace("(Clone)", ""))
        {
            case "Cargo_Blue":
                cargoId = 0;
                break;
            case "Cargo_Red":
                cargoId = 1;
                break;
            case "Cargo_Green":
                cargoId = 2;
                break;
            case "Cargo_Purple":
                cargoId = 3;
                break;
            case "Cargo_Yellow":
                cargoId = 4;
                break;
            case "Cargo_Pink":
                cargoId = 5;
                break;
            case "Cargo_Grey":
                cargoId = 6;
                break;
            case "Cargo_Cyan":
                cargoId = 7;
                break;
            case "Cargo_Orange":
                cargoId = 8;
                break;
            case "Cargo_Olive":
                cargoId = 9;
                break;
            default:
                cargoId = -1;
                break;
        }
    }

    private bool IsClickable()
    {
        if (BoardManager.Instance.isFreeClickEnabled) return true;
        return BoardManager.Instance.IsCargoClickable(position);
    }

    public static void TriggerNormalCargoClicked(NormalCargo cargo)
    {
        OnNormalCargoClicked?.Invoke(cargo);
    }
}