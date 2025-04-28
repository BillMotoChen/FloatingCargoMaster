using UnityEngine;

public class ShopItemCoin : ShopItem, IShopItem
{
    public int itemId;
    public int quantity;
    public float price;


    public void PurchaseItem()
    {
        PlayerData.coin += quantity;
        // TODO: purchase with money function
        PlayerData.instance.SaveData();
        HomeUIManager.instance.UpdateCoinText();
    }
}
