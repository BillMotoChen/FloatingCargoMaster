using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class StorageManager : MonoBehaviour
{
    public GameObject storage;
    public GameObject slotPrefab;
    public GameObject lockedSlotPrefab;
    public GameObject[] cargoPrefabs;

    private Transform[] slots;
    private int[] cargosInSlots;
    private int availableStorageNum;

    private bool clickable;

    public NormalModeGameManager normalModeGameManager;

    public delegate void NormalMatchHandler(int id);
    public static event NormalMatchHandler OnNormalCargoMatch;


    void Start()
    {
        clickable = true;
        availableStorageNum = 5;
        slots = storage.GetComponentsInChildren<Transform>()
               .Where(slot => slot.CompareTag("Slot"))
               .ToArray();
        InitiateStorage(availableStorageNum);
        cargosInSlots = new int[availableStorageNum];
        Array.Fill(cargosInSlots, -1);
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

    private async void AddNormalCargoToSlot(NormalCargo cargo)
    {
        if (!clickable || Time.timeScale <= 0) return;
        clickable = false;
        if (await IsAMatch(cargo.cargoId))
        {
            Debug.Log($"✅ Match Found! Removing all instances of {cargo.cargoId}.");
            OnNormalCargoMatch?.Invoke(cargo.cargoId);
            return;
        }

        int slotId = await FindSlotToInsert(cargo.cargoId);
        if (slotId == -2)
        {
            normalModeGameManager.GameFailed();
            return;
        }

        // Insert cargo visually & update tracking
        Transform firstChild = slots[slotId].GetChild(0);
        GameObject cargoToSpawn = Instantiate(cargoPrefabs[cargo.cargoId], firstChild);
        cargoToSpawn.transform.SetParent(firstChild);
        cargoToSpawn.transform.localPosition = Vector3.zero;
        cargoToSpawn.transform.localScale = new Vector3(20, 20, 1);
        Destroy(cargoToSpawn.GetComponent<NormalCargo>());

        SlotManager slotManager = firstChild.GetComponent<SlotManager>();
        slotManager.hasCargo = true;
        cargosInSlots[slotId] = cargo.cargoId;
        clickable = true;
    }

    private async Task<int> FindSlotToInsert(int id)
    {
        await IsAMatch(id);

        if (slots[availableStorageNum - 1].GetChild(0).GetComponent<SlotManager>().hasCargo)
        {
            return -2;
        }

        for (int i = 0; i < cargosInSlots.Length; i++)
        {
            if (cargosInSlots[i] == id)
            {
                await MoveActualCargo(i + 1, 1);
                return i + 1;
            }
            if (cargosInSlots[i] == -1)
            {
                return i;
            }
        }

        return -2;
    }

    private async Task<bool> IsAMatch(int id)
    {
        List<int> matchPos = new List<int>();

        for (int i = 0; i < cargosInSlots.Length; i++)
        {
            if (cargosInSlots[i] == id)
            {
                matchPos.Add(i);
            }
        }

        if (matchPos.Count >= 2)
        {
            // ✅ Remove the matched cargo
            RemoveCargoWithId(matchPos.ToArray());

            // ✅ Immediately shift remaining items left
            ShiftCargosLeft();

            // ✅ Wait for the movement to complete
            await MoveActualCargo(matchPos[0], -matchPos.Count);

            return true;
        }

        return false;
    }

    private void RemoveCargoWithId(int[] matchPos)
    {
        foreach (int i in matchPos)
        {
            if (i < 0 || i >= slots.Length) continue;

            Transform firstChild = slots[i].GetChild(0);
            foreach (Transform child in firstChild)
            {
                if (child.CompareTag("Cargo"))
                {
                    Destroy(child.gameObject);
                }
            }

            SlotManager slotManager = firstChild.GetComponent<SlotManager>();
            slotManager.hasCargo = false;
            cargosInSlots[i] = -1;
        }
    }

    private async Task MoveActualCargo(int startPos, int distance)
    {
        if (distance > 0)
        {
            for (int i = availableStorageNum - 1; i >= startPos; i--)
            {
                Transform firstChild = slots[i].GetChild(0);
                SlotManager slotManager = firstChild.GetComponent<SlotManager>();

                if (slotManager.hasCargo)
                {
                    Transform lastCargo = null;
                    for (int j = firstChild.childCount - 1; j >= 0; j--)
                    {
                        if (firstChild.GetChild(j).CompareTag("Cargo"))
                        {
                            lastCargo = firstChild.GetChild(j);
                            break;
                        }
                    }

                    if (lastCargo != null)
                    {
                        int targetPos = Math.Min(i + distance, slots.Length - 1);
                        Transform targetFirstChild = slots[targetPos].GetChild(0);

                        lastCargo.SetParent(targetFirstChild);
                        await SmoothMove(lastCargo.gameObject, targetFirstChild.position, 0.1f);

                        slotManager.hasCargo = false;
                        targetFirstChild.GetComponent<SlotManager>().hasCargo = true;

                        cargosInSlots[targetPos] = cargosInSlots[i];
                        cargosInSlots[i] = -1;
                    }
                }
            }
        }
        else
        {
            for (int i = startPos; i < availableStorageNum; i++)
            {
                Transform firstChild = slots[i].GetChild(0);
                SlotManager slotManager = firstChild.GetComponent<SlotManager>();

                if (slotManager.hasCargo)
                {
                    Transform lastCargo = null;
                    for (int j = firstChild.childCount - 1; j >= 0; j--)
                    {
                        if (firstChild.GetChild(j).CompareTag("Cargo"))
                        {
                            lastCargo = firstChild.GetChild(j);
                            break;
                        }
                    }

                    if (lastCargo != null)
                    {
                        int targetPos = Math.Max(i + distance, 0);
                        Transform targetFirstChild = slots[targetPos].GetChild(0);

                        lastCargo.SetParent(targetFirstChild);
                        await SmoothMove(lastCargo.gameObject, targetFirstChild.position, 0.1f);

                        slotManager.hasCargo = false;
                        targetFirstChild.GetComponent<SlotManager>().hasCargo = true;
                    }
                }
            }
        }
        clickable = true;
    }

    private async Task SmoothMove(GameObject obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        obj.transform.position = targetPos;
    }

    private void ShiftCargosLeft()
    {
        int insertPos = 0;
        for (int i = 0; i < cargosInSlots.Length; i++)
        {
            if (cargosInSlots[i] != -1)
            {
                cargosInSlots[insertPos++] = cargosInSlots[i];
            }
        }
        for (int i = insertPos; i < cargosInSlots.Length; i++)
        {
            cargosInSlots[i] = -1;
        }
    }

    private void DebugString()
    {
        Debug.Log(string.Join(" ", cargosInSlots));
    }
}
