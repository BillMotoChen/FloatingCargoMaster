using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public GameObject storage;
    public GameObject slotPrefab;
    public GameObject lockedSlotPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitiateStorage(7);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitiateStorage(int unlockNum)
    {
        Transform[] slots = storage.GetComponentsInChildren<Transform>();

        int count = 0;

        foreach (Transform slot in slots)
        {
            if (!slot.CompareTag("Slot")) continue;

            GameObject prefabToUse = count < unlockNum ? slotPrefab : lockedSlotPrefab;
            GameObject newSlotObject = Instantiate(prefabToUse, slot);
            newSlotObject.transform.SetParent(slot);
            newSlotObject.transform.localPosition = Vector3.zero;
            newSlotObject.transform.localScale = Vector3.one;
            count++;
        }
    }
}
