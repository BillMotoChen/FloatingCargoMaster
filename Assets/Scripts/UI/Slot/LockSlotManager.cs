using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LockSlotManager : MonoBehaviour, IPointerClickHandler
{
    public static event Action OnNewSlotUnlock;
    public int[] price = {20, 50, 100, 500, 1000};

    public GameObject coinIcon;
    public TMP_Text priceText;
    public bool isNextSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isNextSlot)
        {
            if (int.TryParse(priceText.text, out int priceValue))
            {
                if (PlayerData.coin >= priceValue)
                {
                    //PlayerData.coin -= priceValue;
                    //PlayerData.instance.SaveData();
                    OnNewSlotUnlock?.Invoke();
                }
            }
        }
    }
}
