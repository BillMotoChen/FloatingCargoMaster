using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    private LevelDataList levelDataList;
    private bool isDataLoaded = false;
    public LevelData CurrentLevel { get; private set; } // Stores the currently loaded level

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
        TextAsset jsonFile = Resources.Load<TextAsset>("LevelData/levels");

        if (jsonFile == null)
        {
            Debug.LogError("❌ Failed to load levels.json from Resources/LevelData/");
            return;
        }

        string jsonText = jsonFile.text;
        levelDataList = JsonUtility.FromJson<LevelDataList>(jsonText);
        isDataLoaded = true;

        Debug.Log("✅ levels.json loaded successfully from Resources/LevelData/");
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
                Debug.Log($"🎮 Loaded Level {levelNumber}");
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
}