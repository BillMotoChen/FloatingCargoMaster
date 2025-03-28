using UnityEngine;

public class FreezeCargo : SpecialCargo, ISpecialCargoEffect
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
                
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ApplyEffect()
    {
        boardManager.stopRotate = 3;
    }
}
