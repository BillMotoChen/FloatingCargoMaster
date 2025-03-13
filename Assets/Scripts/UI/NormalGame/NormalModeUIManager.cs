using UnityEngine;
using UnityEngine.SceneManagement;

public class NormalModeUIManager : MonoBehaviour
{
    public GameObject winPopup;
    public GameObject losePopup;
    public GameObject pausePopup;
    public GameObject pauseButton;

    public GameObject board;
    public GameObject storage;

    private void Start()
    {
        HideAllPopUp();
    }

    private void OnEnable()
    {
        NormalModeGameManager.OnGameCleared += ShowWinPopup;
        NormalModeGameManager.OnGameFailed += ShowLosePopup;
    }

    private void OnDisable()
    {
        NormalModeGameManager.OnGameCleared -= ShowWinPopup;
        NormalModeGameManager.OnGameFailed -= ShowLosePopup;
    }

    private void ShowWinPopup()
    {
        HideGameObjects();
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        winPopup.SetActive(true);
    }

    private void ShowLosePopup()    
    {
        HideGameObjects();
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        losePopup.SetActive(true);
    }

    public void ShowPausePopup()
    {
        pausePopup.SetActive(true);
        pauseButton.SetActive(false);
        HideGameObjects();
        Debug.Log("⏸️ Game Paused");
        Time.timeScale = 0;
    }

    public void HidePausePopup()
    {
        pausePopup.SetActive(false);
        pauseButton.SetActive(true);
        ShowGameObjects();
        Debug.Log("▶️ Game Resumed");
        Time.timeScale = 1;
    }

    public void HideAllPopUp()
    {
        winPopup.SetActive(false);
        losePopup.SetActive(false);
        pausePopup.SetActive(false);
    }

    private void HideGameObjects()
    {
        board.SetActive(false);
        storage.SetActive(false);
    }

    private void ShowGameObjects()
    {
        board.SetActive(true);
        storage.SetActive(true);
    }

    public void ReplayOrNextNormalMode()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalMode");
    }
}
