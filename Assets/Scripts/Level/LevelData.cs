using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Exporter
{
    public Vector2Int cor;
}

[System.Serializable]
public class LevelData
{
    public int level;
    public int boardWidth;
    public int boardHeight;
    public List<Exporter> exporters;
    public bool tutorial;
    public int gameMode;
    public int color;
    public List<int> specialCargo;
    public List<int> requiredSets;
    public string cycleId;
    public int coinGained;
}