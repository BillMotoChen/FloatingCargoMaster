using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    private LevelDataList levelDataList;
    private CycleDataList cycleDataList;
    private bool isDataLoaded = false;
    public LevelData CurrentLevel { get; private set; } // Stores the currently loaded level
    public CycleData CurrentLevelCycle { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLevelData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads all level data from JSON (Resources/LevelData/levels.json)
    /// </summary>
    private void LoadLevelData()
    {
        TextAsset levelJsonFile = Resources.Load<TextAsset>("LevelData/levels");
        TextAsset pathJsonFile = Resources.Load<TextAsset>("LevelData/path");

        if (levelJsonFile == null || pathJsonFile == null) return;
       
        string levelJsonText = levelJsonFile.text;
        string pathJsonText = pathJsonFile.text;

        levelDataList = JsonUtility.FromJson<LevelDataList>(levelJsonText);
        cycleDataList = JsonUtility.FromJson<CycleDataList>(pathJsonText);
        isDataLoaded = true;
    }

    /// <summary>
    /// Loads a level by number and sets it as the current level.
    /// </summary>
    public bool LoadLevel(int levelNumber)
    {
        if (!isDataLoaded)
        {
            Debug.LogError("❌ Level data is still loading!");
            return false;
        }

        if (levelDataList == null || levelDataList.levels == null)
        {
            Debug.LogError("❌ Level data is NOT loaded!");
            return false;
        }

        foreach (var level in levelDataList.levels)
        {
            if (level.level == levelNumber)
            {
                CurrentLevel = level;
                foreach (var cycle in cycleDataList.cycles)
                {
                    if (cycle.id == level.cycleId)
                    {
                        CurrentLevelCycle = cycle;
                        break;
                    }
                }
                return true;
            }
        }

        Debug.LogError($"❌ Level {levelNumber} not found in JSON!");
        return false;
    }

    /// <summary>
    /// Retrieves the current level data (so any scene can access it).
    /// </summary>
    public LevelData GetCurrentLevel()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError("❌ No level is currently loaded!");
        }
        return CurrentLevel;
    }

    public CycleData GetCurrentLevelCycles(string cycleId)
    {
        if (CurrentLevelCycle == null)
        {
            Debug.LogError("❌ No cycle is currently loaded!");
        }
        return CurrentLevelCycle;
    }
}