using UnityEngine;

// Vibrace telefonu s omezenim frekvence (aby telefon nebzucel neustale).
public static class Haptics
{
    private static float last = -10f;

    public static void Vibrate(float minInterval = 0.8f)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (PlayerPrefs.GetString("VibrationOff") == "1") return;
        if (Time.unscaledTime - last < minInterval) return;
        last = Time.unscaledTime;
        Handheld.Vibrate();
#endif
    }
}
