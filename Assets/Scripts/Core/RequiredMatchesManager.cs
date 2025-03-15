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

        float parentWidth = requiredMatchesArea.GetComponent<RectTransform>().rect.width;
        int maxItems = 5;
        float leftMargin = parentWidth * 0.1f;
        float spacing = parentWidth * 0.80f / (maxItems - 1);

        int xCount = 0;

        for (int i = 0; i < requiredMatches.Count; i++)
        {
            if (requiredMatches[i] > 0) // ✅ Only instantiate if the required amount is greater than 0
            {
                // Instantiate the required cargo prefab
                GameObject requiredCargo = Instantiate(requiredNormalCargoPrefabs[i], requiredMatchesArea.transform);
                Transform cargoRect = requiredCargo.GetComponent<Transform>();

                // Set position
                float xPos = leftMargin + (xCount++ * spacing) - parentWidth * 0.5f;
                cargoRect.localPosition = new Vector3(xPos, 20, 0);

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
        requiredMatches[id] -= 1;
        if (requiredMatches[id] <= 0)
        {
            try
            {
                requiredMatchesCheck[id].SetActive(true);
                requiredMatchesNums[id].text = "";
                CheckWin();
            }
            catch
            {
                return;
            }
        }
        else
        {
            requiredMatchesNums[id].text = requiredMatches[id].ToString();
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
