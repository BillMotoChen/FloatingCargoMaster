using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Vector2Int position;
    public GameObject cargoPrefab;

    public void TrySpawnCargo()
    {
        //if (BoardManager.Instance.CanSpawnCargo(position))
        //{
        //    Instantiate(cargoPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        //}
    }
}
