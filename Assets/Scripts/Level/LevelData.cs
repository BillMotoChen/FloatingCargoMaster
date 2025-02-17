using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int level;
    public int boardWidth;
    public int boardHeight;
    public bool tutorial;
    public int gameMode;
    public int color;
    public List<int> specialCargo;
    public List<int> requiredSets;
    public string cycleId;
}