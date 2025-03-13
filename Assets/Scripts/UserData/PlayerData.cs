using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class PlayerData : MonoBehaviour
{
    const string savedFilePath = "/save.dat";

    public static PlayerData instance;

    // stage data
    public static int stage = 1;
    public static int coin = 0;
    public static int slotNum = 5;


    // saved data
    // saved stage data
    public int stageSaved = 1;
    public int coinSaved = 0;
    public int slotNumSaved = 5;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadData();
    }

    void dataInit()
    {
        stage = 1;
        coin = 0;
        slotNum = 5;
        SaveData();
        LoadData();
    }

    public void SaveData()
    {
        string fileLocation = Application.persistentDataPath + savedFilePath;

        coinSaved = coin;
        stageSaved = stage;
        slotNumSaved = slotNum;

        string jsonData = JsonUtility.ToJson(this);

        //string encryptedData = EncryptionUtility.Encrypt(jsonData);
        string encryptedData = jsonData;

        try
        {
            StreamWriter writer = new StreamWriter(fileLocation);
            writer.Write(encryptedData);
            writer.Close();
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to save data: " + e.Message);
        }
    }

    public void LoadData()
    {
        string fileLocation = Application.persistentDataPath + savedFilePath;
        Debug.Log(fileLocation);
        if (File.Exists(fileLocation))
        {
            try
            {
                StreamReader reader = new StreamReader(fileLocation);
                string encryptedData = reader.ReadToEnd();
                reader.Close();

                //string decryptedData = EncryptionUtility.Decrypt(encryptedData);
                string decryptedData = encryptedData;
                JsonUtility.FromJsonOverwrite(decryptedData, this);
                coin = coinSaved;
                stage = stageSaved;
                slotNum = slotNumSaved;
            }
            catch (IOException e)
            {
                Debug.LogError("Failed to load data: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Save file does not exist, initializing new data.");
            dataInit();
        }
    }
}
