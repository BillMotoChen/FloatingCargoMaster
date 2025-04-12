using UnityEngine;

public class FreeClickItem : ItemBase
{
    public override void ActivateItem()
    {
        BoardManager.Instance.EnableFreeClick();
    }
}