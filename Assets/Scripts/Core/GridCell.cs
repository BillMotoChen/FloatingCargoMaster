using UnityEngine;

public class GridCell
{
    public Vector2Int position;
    public GameObject cellObject;
    public CargoBase cargo;

    // **箭頭方向**
    private GameObject arrowUp, arrowDown, arrowLeft, arrowRight;

    public GridCell(Vector2Int pos, GameObject obj)
    {
        position = pos;
        cellObject = obj;
        cargo = null;

        // **找到這個格子裡的箭頭**
        arrowUp = cellObject.transform.Find("ArrowUp")?.gameObject;
        arrowDown = cellObject.transform.Find("ArrowDown")?.gameObject;
        arrowLeft = cellObject.transform.Find("ArrowLeft")?.gameObject;
        arrowRight = cellObject.transform.Find("ArrowRight")?.gameObject;

        // **預設所有箭頭關閉**
        SetArrowActive(false);
    }

    // **設定箭頭方向**
    public void SetArrowDirection(Vector2Int nextPos)
    {
        Vector2Int direction = nextPos - position;

        // **關閉所有箭頭**
        SetArrowActive(false);

        // **根據路線顯示正確的箭頭**
        if (direction == Vector2Int.up) arrowUp?.SetActive(true);
        else if (direction == Vector2Int.down) arrowDown?.SetActive(true);
        else if (direction == Vector2Int.left) arrowLeft?.SetActive(true);
        else if (direction == Vector2Int.right) arrowRight?.SetActive(true);
    }

    private void SetArrowActive(bool state)
    {
        arrowUp?.SetActive(state);
        arrowDown?.SetActive(state);
        arrowLeft?.SetActive(state);
        arrowRight?.SetActive(state);
    }

    public bool HasCargo()
    {
        return cargo != null;
    }

    public void SetCargo(CargoBase newCargo)
    {
        cargo = newCargo;
    }

    public void ClearCargo()
    {
        cargo = null;
    }
}