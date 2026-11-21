using UnityEngine;

// Zadost o hodnoceni hry v obchode (nativni okno: Google Play In-App Review / iOS StoreKit).
// Po dobre hre (novy high score nebo zabity boss) se nastavi ReviewPending; okno se ukaze
// pri navratu do menu, nejdriv od MinGames-te hry a jen jednou (ReviewAsked = "1").
// Obchod sam rozhoduje, jestli okno opravdu zobrazi (kvoty), takze se neda vynutit.
public static class ReviewPrompt
{
    private const string AskedKey = "ReviewAsked";
    private const string PendingKey = "ReviewPending";
    private const long MinGames = 5;

    // Volat na konci dobre hry.
    public static void MarkGoodGame()
    {
        if (PlayerPrefs.GetString(AskedKey) == "1") return;
        PlayerPrefs.SetString(PendingKey, "1");
    }

    // Volat v menu; zobrazi zadost, pokud jsou splnene podminky.
    public static void TryShow()
    {
        if (PlayerPrefs.GetString(AskedKey) == "1") return;
        if (PlayerPrefs.GetString(PendingKey) != "1") return;
        long games;
        long.TryParse(PlayerPrefs.GetString("Games"), out games);
        if (games < MinGames) return;

        PlayerPrefs.SetString(AskedKey, "1");
        PlayerPrefs.SetString(PendingKey, "0");
        PlayerPrefs.Save();
        Request();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject manager;

    private static void Request()
    {
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var factory = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory");
            manager = factory.CallStatic<AndroidJavaObject>("create", activity);
            var task = manager.Call<AndroidJavaObject>("requestReviewFlow");
            task.Call<AndroidJavaObject>("addOnCompleteListener", new ReviewInfoListener(activity));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("ReviewPrompt: " + e.Message);
        }
    }

    private class ReviewInfoListener : AndroidJavaProxy
    {
        private readonly AndroidJavaObject activity;

        public ReviewInfoListener(AndroidJavaObject activity)
            : base("com.google.android.gms.tasks.OnCompleteListener")
        {
            this.activity = activity;
        }

        public void onComplete(AndroidJavaObject task)
        {
            try
            {
                if (!task.Call<bool>("isSuccessful")) return;
                var reviewInfo = task.Call<AndroidJavaObject>("getResult");
                manager.Call<AndroidJavaObject>("launchReviewFlow", activity, reviewInfo);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("ReviewPrompt: " + e.Message);
            }
        }
    }
#elif UNITY_IOS && !UNITY_EDITOR
    private static void Request()
    {
        UnityEngine.iOS.Device.RequestStoreReview();
    }
#else
    private static void Request()
    {
        Debug.Log("ReviewPrompt: zde by se zobrazila zadost o hodnoceni (jen na zarizeni).");
    }
#endif
}
