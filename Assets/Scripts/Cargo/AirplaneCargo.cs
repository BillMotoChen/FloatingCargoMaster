using UnityEngine;

public class AirplaneCargo : SpecialCargo, ISpecialCargoEffect
{

    public override bool OverrideClickability => true;

    public void ApplyEffect()
    {
        BoardManager.Instance.StartCoroutine(BoardManager.Instance.RotateMultipleTimes(3));
    }

    protected override bool IsClickable()
    {
        return true;
    }
}
