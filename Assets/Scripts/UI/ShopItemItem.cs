using UnityEngine;

public class ShopItemItem : ShopItem, IShopItem
{
    public int itemId;
    public int quantity;
    public int price;


    public void PurchaseItem()
    {
        if(PlayerData.coin < price)
        {
            StartCoroutine(ShopManager.Instance.ShowWarningIE(ShopManager.NO_ENOUGH_COIN_TEXT));
        }
        else {
            PlayerData.coin -= price;
            PlayerData.itemHold[itemId] += quantity;
            PlayerData.instance.SaveData();
            HomeUIManager.instance.UpdateCoinText();
        }
    }
}
