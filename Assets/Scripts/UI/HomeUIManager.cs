using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeUIManager : MonoBehaviour
{
    public TMP_Text coinText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCoinText();   
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

    private void UpdateCoinText()
    {
        coinText.text = PlayerData.coin.ToString();
    }
}
