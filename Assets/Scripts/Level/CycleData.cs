using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CycleWrapper
{
    public List<Vector2Int> points = new List<Vector2Int>();
}

[System.Serializable]
public class CycleData
{
    public string id;
    public int boardWidth;
    public int boardHeight;
    public List<CycleWrapper> cycles = new List<CycleWrapper>();
}