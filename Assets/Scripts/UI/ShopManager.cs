using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public const string NO_ENOUGH_COIN_TEXT = "No enough coins";


    public GameObject warningObj;
    public TMP_Text warningText;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        warningObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowWarning(string message)
    {
        warningText.text = message;
    }

    public IEnumerator ShowWarningIE(string message)
    {
        Debug.Log(message);
        warningObj.SetActive(true);
        ShowWarning(message);
        yield return new WaitForSeconds(1f);
        warningObj.SetActive(false);
    }
}