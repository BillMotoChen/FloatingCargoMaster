using UnityEngine;

public class RainbowCargo : SpecialCargo, ISpecialCargoEffect
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
        int maxColor = LevelLoader.Instance.GetCurrentLevel().color;

        int randomId = Random.Range(0, maxColor);

        GameObject dummyObj = new GameObject("FakeNormalCargo_" + randomId);
        NormalCargo fakeCargo = dummyObj.AddComponent<NormalCargo>();
        fakeCargo.cargoId = randomId;

        NormalCargo.TriggerNormalCargoClicked(fakeCargo);

        Destroy(dummyObj);
        Destroy(gameObject);
    }
}