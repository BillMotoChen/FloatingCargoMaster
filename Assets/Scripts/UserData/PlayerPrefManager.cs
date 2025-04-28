using UnityEngine;

public static class PlayerPrefManager
{
    public const string SFX_KEY = "SoundFXEnabled";

    public static bool IsSFXEnabled() => PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

    public static void SetSFXEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SFX_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}