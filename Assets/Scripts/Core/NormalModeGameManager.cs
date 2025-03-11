using UnityEngine;
using System;

public class NormalModeGameManager : MonoBehaviour
{
    public static event Action OnGameCleared;
    public static event Action OnGameFailed;

    // **🔹 Call when the game is cleared**
    public void GameCleared()
    {
        Debug.Log("🎉 Game Cleared!");
        OnGameCleared?.Invoke();
    }

    // **🔹 Call when the player loses**
    public void GameFailed()
    {
        Debug.Log("❌ Game Over!");
        OnGameFailed?.Invoke();
    }

}
