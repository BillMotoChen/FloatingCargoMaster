using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;

public class RequiredMatchesManager : MonoBehaviour
{
    private LevelData levelData;
    private List<int> requiredMatches;
    private List<TMP_Text> requiredMatchesNums;
    private List<GameObject> requiredMatchesCheck;
    private Dictionary<int, int> matchIndexMap;


    public List<GameObject> requiredNormalCargoPrefabs;
    public GameObject requiredMatchesArea;

    public NormalModeGameManager normalModeGameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelLoader.Instance.LoadLevel(PlayerData.stage);
        levelData = LevelLoader.Instance.GetCurrentLevel();
        requiredMatches = new List<int>(levelData.requiredSets);
        InitRequiredArea();
    }

    private void OnEnable()
    {
        StorageManager.OnNormalCargoMatch += UpdateTextWhenMatch;
    }

    private void OnDisable()
    {
        StorageManager.OnNormalCargoMatch -= UpdateTextWhenMatch;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitRequiredArea()
    {
        requiredMatchesNums = new List<TMP_Text>();
        requiredMatchesCheck = new List<GameObject>();
        matchIndexMap = new Dictionary<int, int>();

        float parentWidth = requiredMatchesArea.GetComponent<RectTransform>().rect.width;
        int maxPerRow = 5;
        float leftMargin = parentWidth * 0.1f;
        float spacing = parentWidth * 0.80f / (maxPerRow - 1);

        int xCount = 0, yCount = 0;
        int actualIndex = 0; // Index for `requiredMatchesNums` and `requiredMatchesCheck`

        for (int i = 0; i < requiredMatches.Count; i++)
        {
            if (requiredMatches[i] > 0) // ✅ Only instantiate if the required amount is greater than 0
            {
                // Store mapping from `requiredMatches[i]` index to `requiredMatchesNums` index
                matchIndexMap[i] = actualIndex;

                // Instantiate the required cargo prefab
                GameObject requiredCargo = Instantiate(requiredNormalCargoPrefabs[i], requiredMatchesArea.transform);
                Transform cargoRect = requiredCargo.GetComponent<Transform>();

                // Set position
                float xPos = leftMargin + (xCount * spacing) - parentWidth * 0.5f;
                float yPos = 20 - (yCount * 80);
                cargoRect.localPosition = new Vector3(xPos, yPos, 0);

                // ✅ Find TMP_Text inside the instantiated prefab (RequiredMatchNum)
                TMP_Text matchText = requiredCargo.GetComponentInChildren<TMP_Text>();
                if (matchText != null)
                {
                    matchText.text = requiredMatches[i].ToString();
                    requiredMatchesNums.Add(matchText);
                }

                // ✅ Find checkmark GameObject inside the instantiated prefab (RequiredMatchCheck)
                GameObject checkObj = FindChildWithTag(requiredCargo.transform, "RequiredMatchCheck");
                if (checkObj != null)
                {
                    checkObj.SetActive(false);
                    requiredMatchesCheck.Add(checkObj);
                }

                actualIndex++; // ✅ Increment stored index for `requiredMatchesNums`

                // Move to next row if needed
                xCount++;
                if (xCount >= maxPerRow)
                {
                    xCount = 0;
                    yCount++;
                }
            }
        }
    }

    /// ✅ Helper method to find a child GameObject by tag
    private GameObject FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private void UpdateTextWhenMatch(int id)
    {
        if (!matchIndexMap.ContainsKey(id)) return; // ✅ Ensure we have a valid mapping

        int mappedIndex = matchIndexMap[id]; // ✅ Get correct index in `requiredMatchesNums`

        requiredMatches[id] -= 1;
        if (requiredMatches[id] <= 0)
        {
            try
            {
                requiredMatchesCheck[mappedIndex].SetActive(true);
                requiredMatchesNums[mappedIndex].text = "";
                CheckWin();
            }
            catch
            {
                return;
            }
        }
        else
        {
            requiredMatchesNums[mappedIndex].text = requiredMatches[id].ToString();
        }
    }

    private void CheckWin()
    {
        if (requiredMatches.All(x=> x <= 0))
        {
            normalModeGameManager.GameCleared();
        }
    }
}
