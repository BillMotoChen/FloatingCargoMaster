using UnityEngine;
using System;

public class NormalModeGameManager : MonoBehaviour
{
    public static event Action OnGameCleared;
    public static event Action OnGameFailed;

    // **🔹 Call when the game is cleared**
    public void GameCleared()
    {
        PlayerData.stage += 1;
        PlayerData.instance.SaveData();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.levelCleared);
        OnGameCleared?.Invoke();
    }

    // **🔹 Call when the player loses**
    public void GameFailed()
    {
        OnGameFailed?.Invoke();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.levelFailed);
    }

}
