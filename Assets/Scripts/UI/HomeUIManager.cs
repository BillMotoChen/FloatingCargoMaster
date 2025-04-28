using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeUIManager : MonoBehaviour
{
    public static HomeUIManager instance;

    public TMP_Text coinText;
    public TMP_Text shopCoinText;
    public TMP_Text levelText;

    public GameObject shopPopUp;

    public GameObject settingPopUp;
    public GameObject SFXOn;
    public GameObject SFXOff;


    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCoinText();
        UpdateLevelText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayNormalGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalMode");
    }

    public void UpdateCoinText()
    {
        coinText.text = PlayerData.coin.ToString();
        shopCoinText.text = PlayerData.coin.ToString();
    }

    private void UpdateLevelText()
    {
        levelText.text = "Level " + PlayerData.stage.ToString();
    }

    public void SettingOn()
    {
        settingPopUp.SetActive(true);
        SFXToggle();
    }

    public void SettingOff()
    {
        settingPopUp.SetActive(false);
    }

    private void SFXToggle()
    {
        SFXOn.SetActive(PlayerPrefManager.IsSFXEnabled());
        SFXOff.SetActive(!PlayerPrefManager.IsSFXEnabled());
    }

    public void SFXSwitch()
    {
        PlayerPrefManager.SetSFXEnabled(!PlayerPrefManager.IsSFXEnabled());
        SFXToggle();
    }

    public void ShopOn()
    {
        shopPopUp.SetActive(true);
        UpdateCoinText();
    }

    public void ShopOff()
    {
        shopPopUp.SetActive(false);
        UpdateCoinText();
    }
}
