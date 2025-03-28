using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public NormalModeUIManager normalModeUIManager;
    public StorageManager storageManager;

    private bool isTutorial;

    [System.Serializable]
    public class Tutorial
    {
        public int level; // Level this tutorial belongs to
        public List<GameObject> steps; // Steps in order
    }

    public List<Tutorial> tutorials; // List of all tutorials
    private Dictionary<int, Tutorial> tutorialDictionary = new Dictionary<int, Tutorial>();

    private int currentStepIndex = 0;
    private Tutorial currentTutorial;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        isTutorial = false;
        InitTutorials();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isTutorial)
        {
            NextStep();
        }
    }

    private void InitTutorials()
    {
        int currentLevel = PlayerData.stage; // Get current level

        // Convert list to dictionary for quick lookup
        foreach (var tutorial in tutorials)
        {
            if (tutorial.level == currentLevel)
            {
                tutorialDictionary[tutorial.level] = tutorial;
            }
            else
            {
                // ✅ Destroy all tutorial GameObjects not for this level
                foreach (GameObject step in tutorial.steps)
                {
                    if (step != null)
                    {
                        Destroy(step);
                    }
                }
            }
        }

        // Check if there's a tutorial for this level
        if (tutorialDictionary.ContainsKey(currentLevel))
        {
            currentTutorial = tutorialDictionary[currentLevel];
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        if (currentTutorial == null || currentTutorial.steps.Count == 0) return;

        isTutorial = true;
        // Hide all steps first
        foreach (GameObject step in currentTutorial.steps)
        {
            step.SetActive(false);
        }

        // Show the first step
        currentStepIndex = 0;
        currentTutorial.steps[currentStepIndex].SetActive(true);
        normalModeUIManager.HideGameObjects();
    }

    public void NextStep()
    {
        if (currentTutorial == null || currentStepIndex >= currentTutorial.steps.Count - 1)
        {
            EndTutorial();
            return;
        }

        // Hide the current step
        currentTutorial.steps[currentStepIndex].SetActive(false);

        // Show the next step
        currentStepIndex++;
        currentTutorial.steps[currentStepIndex].SetActive(true);
    }

    private void EndTutorial()
    {
        isTutorial = false;
        if (currentTutorial != null)
        {
            foreach (GameObject step in currentTutorial.steps)
            {
                step.SetActive(false);
            }
        }
        normalModeUIManager.ShowGameObjects();

        currentTutorial = null; // Clear tutorial data
    }
}