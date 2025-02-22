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
            default:
                cargoId = -1;
                break;
        }
    }
}