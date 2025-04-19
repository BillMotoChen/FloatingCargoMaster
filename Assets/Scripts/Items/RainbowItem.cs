using UnityEngine;

public class RainbowItem : ItemBase
{
    public override void ActivateItem()
    {
        BoardManager.Instance.RainbowClick();
    }
}
