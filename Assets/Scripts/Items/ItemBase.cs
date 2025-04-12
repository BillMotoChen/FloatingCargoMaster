using UnityEngine;
using TMPro;

public abstract class ItemBase : MonoBehaviour, IItemEffect
{
    public int itemId;
    public int itemUnlockLevel;
    public int price;
    public TMP_Text remainNumText;
    public GameObject itemObject;
    public GameObject itemLockObject;
    public GameObject coin;

    private void Start()
    {
        if (PlayerData.stage >= itemUnlockLevel)
        {
            Unlock();
            UpdateUI();
        }
        else
        {
            Lock();       
        }
    }

    private void Unlock()
    {
        itemObject.SetActive(true);
        itemLockObject.SetActive(false);
    }

    private void Lock()
    {
        itemObject.SetActive(false);
        itemLockObject.SetActive(true);
    }

    private void UpdateUI()
    {
        if(PlayerData.itemHold[itemId] == 0)
        {
            coin.SetActive(true);
            remainNumText.text = price.ToString();
        }
        else
        {
            coin.SetActive(false);
            remainNumText.text = PlayerData.itemHold[itemId].ToString();
        }
    }

    public void TryUseItem()
    {
        if (BoardManager.Instance.itemInUsed) return;

        if (PlayerData.itemHold[itemId] > 0)
        {
            PlayerData.itemHold[itemId]--;
            PlayerData.instance.SaveData();
            BoardManager.Instance.itemInUsed = true;
            UpdateUI();
            ActivateItem();
            Debug.Log("Used item.");
            
        }
        else
        {
            if (PlayerData.coin >= price)
            {
                PlayerData.coin -= price;
                PlayerData.instance.SaveData();
                BoardManager.Instance.itemInUsed = true;
                UpdateUI();
                ActivateItem();
                Debug.Log("Used with coin.");
            }
            else
            {
                Debug.Log("❌ Not enough coins to use.");
            }
        }
    }

    public abstract void ActivateItem();

}