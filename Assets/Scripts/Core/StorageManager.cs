using UnityEngine;
using System.Linq;
using System;

public class StorageManager : MonoBehaviour
{
    public GameObject storage;
    public GameObject slotPrefab;
    public GameObject lockedSlotPrefab;
    public GameObject[] cargoPrefabs;

    private Transform[] slots;
    private int[] cargosInSlots;
    private int availableStorageNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        availableStorageNum = 7;
        slots = storage.GetComponentsInChildren<Transform>();
        InitiateStorage(availableStorageNum);
        cargosInSlots = new int[availableStorageNum];
        Array.Fill(cargosInSlots, -1);
        foreach (Transform t in slots)
        {
            Debug.Log(t.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        NormalCargo.OnNormalCargoClicked += AddNormalCargoToSlot;
    }

    private void OnDisable()
    {
        NormalCargo.OnNormalCargoClicked -= AddNormalCargoToSlot;
    }

    private void InitiateStorage(int unlockNum)
    {
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

    private void AddNormalCargoToSlot(NormalCargo cargo)
    {
        int slotId = FindSlotToInsert(cargo.cargoId);
        Debug.Log("SLOT ID: " + slotId);
        if (slotId == -2)
        {
            Debug.LogError("❌ LOSE - No Available Slot!");
            Time.timeScale = 0; // ✅ Stop the game
            return;
        }

        cargosInSlots[slotId] = cargo.cargoId;
        foreach (Transform slot in slots)
        {
            if (!slot.GetChild(0).CompareTag("StorageSlot")) continue;
            Transform firstChild = slot.GetChild(0);
            SlotManager slotManager = firstChild.GetComponent<SlotManager>();
            if (!slotManager.hasCargo)
            {
                GameObject cargoToSpawn = Instantiate(cargoPrefabs[cargo.cargoId], firstChild);
                cargoToSpawn.transform.SetParent(firstChild);
                cargoToSpawn.transform.localPosition = Vector3.zero;
                cargoToSpawn.transform.localScale = new Vector3(20, 20, 1);
                Destroy(cargoToSpawn.GetComponent<NormalCargo>());

                slotManager.hasCargo = true;
                break;
            }
        }
        //if (!slotManager.hasCargo)
        //{
        //    // move all the cargo in the slots behind to one slot behind.
        //    // spawn the cargo at slots[slotId]
        //    // mark the last slot with cargo's hasCargo = true
        //}
        //else
        //{
        //    // just spawn
        //    // and mark it's hasCargo = true
        //}
    }

    private int FindSlotToInsert(int id)
    {
        Debug.Log(slots[availableStorageNum].name);
        if (slots[availableStorageNum].GetChild(0).GetComponent<SlotManager>().hasCargo)
        {
            return -2;
        }
        int firstEmptySlot = -2;
        for (int i = 0; i < cargosInSlots.Length; i++)
        {
            if (cargosInSlots[i] == -1)
            {
                return i;
            }
            if (cargosInSlots[i] == id)
            {
                return i + 1;
            }
        }
        return firstEmptySlot;
    }
}
