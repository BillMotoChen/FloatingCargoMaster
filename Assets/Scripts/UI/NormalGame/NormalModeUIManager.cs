using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NormalModeUIManager : MonoBehaviour
{
    public GameObject winPopup;
    public GameObject losePopup;
    public GameObject pausePopup;
    public GameObject pauseButton;
    public GameObject coinObject;

    public GameObject board;
    public GameObject storage;
    public GameObject levelObject;

    public TMP_Text levelText;
    public TMP_Text coinText;
    public TMP_Text coinGainWhenClearText;

    private int coinGainWhenClear;

    private void Start()
    {
        coinGainWhenClear = LevelLoader.Instance.GetCurrentLevel().coinGained;
        ShowLevelText();
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
        ShowCoinObject();
        coinGainWhenClearText.text = "+ " + coinGainWhenClear.ToString();
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        winPopup.SetActive(true);
    }

    private void ShowLosePopup()    
    {
        HideGameObjects();
        ShowCoinObject();
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        losePopup.SetActive(true);
    }

    public void ShowPausePopup()
    {
        pausePopup.SetActive(true);
        pauseButton.SetActive(false);
        HideGameObjects();
        ShowCoinObject();
        Debug.Log("⏸️ Game Paused");
        Time.timeScale = 0;
    }

    public void HidePausePopup()
    {
        pausePopup.SetActive(false);
        pauseButton.SetActive(true);
        ShowGameObjects();
        HideCoinObject();
        Debug.Log("▶️ Game Resumed");
        Time.timeScale = 1;
    }

    public void HideAllPopUp()
    {
        winPopup.SetActive(false);
        losePopup.SetActive(false);
        pausePopup.SetActive(false);
        coinObject.SetActive(false);
    }

    public void HideGameObjects()
    {
        board.SetActive(false);
        storage.SetActive(false);
        levelObject.SetActive(false);
    }

    public void ShowGameObjects()
    {
        board.SetActive(true);
        storage.SetActive(true);
        levelObject.SetActive(true);
    }

    public void ReplayNormalMode()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalMode");
    }

    public void NextNormalMode()
    {
        GainCoin(coinGainWhenClear);
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalMode");
    }

    public void Continue()
    {
        HideAllPopUp();
        HidePausePopup();
    }

    public void DoubleCoin()
    {
        GainCoin(coinGainWhenClear * 2);
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalMode");
    }

    public void GoHome(int fromWhere) // 1: win, 2: lose, 3: pause
    {
        if (fromWhere == 1)
        {
            GainCoin(coinGainWhenClear);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }

    private void ShowLevelText()
    {
        levelText.text = "LEVEL " + PlayerData.stage.ToString();
    }

    private void UpdateCoinText()
    {
        coinText.text = PlayerData.coin.ToString();
    }

    private void ShowCoinObject()
    {
        coinObject.SetActive(true);
        UpdateCoinText();
    }

    private void HideCoinObject()
    {
        coinObject.SetActive(false);
    }


    private void GainCoin(int coin)
    {
        PlayerData.coin += coin;
        PlayerData.instance.SaveData();
    }
}
