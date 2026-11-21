using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu Tools > Store Media > Capture Screenshots + Video
// Spusti hru z menu s autopilotem (StoreMediaAutopilot), ten udela snimky a ~30 s video (MP4 1280x720,
// pres vestaveny MediaEncoder) do slozky StoreMedia/ vedle Assets.
// Ulozena data hrace (PlayerPrefs) se pred spustenim zalohuji a po skonceni vrati.
[InitializeOnLoad]
public static class StoreMediaCapture
{
    private const string StartScene = "Assets/Scenes/StartPage.unity";
    private const string ActiveKey = "StoreMediaCapture.Active";
    private const string BackupPrefix = "StoreMediaCapture.Backup.";
    private const string NoValue = "\u0001none";

    // Klice, ktere hra behem nahravani zmeni a ktere se pak vrati.
    private static readonly string[] StringKeys =
    {
        "Coins", "HighScore", "Games", "KillAliens", "KillAliensOneGame", "BossKilled", "BossKilled2",
        "BossKilledSpecies", "TutorialDone", "DailyRewardDate", "DailyRewardStreak", "Nick",
    };
    private static readonly string[] IntKeys = { "AdDeathCounter" };

    // Vstup do Play Mode znovu nacte skripty -> po reloadu se znovu pripojit.
    static StoreMediaCapture()
    {
        if (SessionState.GetBool(ActiveKey, false)) Subscribe();
    }

    [MenuItem("Tools/Store Media/Capture Screenshots + Video")]
    private static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Store Media", "Stop Play Mode first.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string outDir = OutDir();
        Directory.CreateDirectory(outDir);

        Backup();
        PlayerPrefs.SetString("TutorialDone", "1");
        PlayerPrefs.SetString("DailyRewardDate", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("StoreMediaCapture", "1");
        PlayerPrefs.SetString("StoreMediaOutDir", outDir);
        PlayerPrefs.DeleteKey("StoreMediaCaptureDone");
        PlayerPrefs.Save();

        EditorSceneManager.OpenScene(StartScene);
        ShowGameView();
        SessionState.SetBool(ActiveKey, true);
        Subscribe();
        EditorApplication.isPlaying = true;
    }

    // Z prikazove radky (bez klikani): Unity.exe -projectPath <p> -executeMethod StoreMediaCapture.RunFromCommandLine
    //   [-storeMediaOut <slozka>]  -> nastavi Game view 1920x1080, nahraje a Unity zavre.
    public static void RunFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-storeMediaOut") SessionState.SetString(OutKey, Path.GetFullPath(args[i + 1]));
        SessionState.SetBool(QuitKey, true);
        PlayModeWindow.SetViewType(PlayModeWindow.PlayModeViewTypes.GameView);
        PlayModeWindow.SetCustomRenderingResolution(1920, 1080, "Store 1920x1080");
        // Pockat, az se Editor po startu usadi.
        double start = EditorApplication.timeSinceStartup;
        EditorApplication.CallbackFunction wait = null;
        wait = () =>
        {
            if (EditorApplication.timeSinceStartup - start < 5 || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= wait;
            Run();
        };
        EditorApplication.update += wait;
    }

    // Snimky (WaitForEndOfFrame / ScreenCapture) funguji jen kdyz je okno Game videt a vykresluje se.
    private static void ShowGameView()
    {
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (type == null) return;
        var w = EditorWindow.GetWindow(type, false, "Game", true);
        w.Show();
        w.Focus();
    }

    private const string OutKey = "StoreMediaCapture.OutDir";
    private const string QuitKey = "StoreMediaCapture.Quit";

    private static string OutDir()
    {
        string custom = SessionState.GetString(OutKey, "");
        if (!string.IsNullOrEmpty(custom)) return custom;
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "StoreMedia"));
    }

    private static void Subscribe()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        EditorApplication.playModeStateChanged += OnPlayMode;
    }

    private static void Tick()
    {
        if (EditorApplication.isPlaying && PlayerPrefs.GetString("StoreMediaCaptureDone") == "1")
            EditorApplication.isPlaying = false;
    }

    private static void OnPlayMode(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode) return;

        Restore();
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        Debug.Log("[StoreMedia] finished, files in " + OutDir());
        if (SessionState.GetBool(QuitKey, false))
        {
            EditorApplication.Exit(0);
            return;
        }
        EditorUtility.RevealInFinder(Path.Combine(OutDir(), "1-menu.png"));
    }

    private static void Backup()
    {
        foreach (var k in StringKeys)
            SessionState.SetString(BackupPrefix + k, PlayerPrefs.HasKey(k) ? PlayerPrefs.GetString(k) : NoValue);
        foreach (var k in IntKeys)
            SessionState.SetString(BackupPrefix + k, PlayerPrefs.HasKey(k) ? PlayerPrefs.GetInt(k).ToString() : NoValue);
    }

    private static void Restore()
    {
        foreach (var k in StringKeys)
        {
            string v = SessionState.GetString(BackupPrefix + k, NoValue);
            if (v == NoValue) PlayerPrefs.DeleteKey(k);
            else PlayerPrefs.SetString(k, v);
        }
        foreach (var k in IntKeys)
        {
            string v = SessionState.GetString(BackupPrefix + k, NoValue);
            int n;
            if (v == NoValue || !int.TryParse(v, out n)) PlayerPrefs.DeleteKey(k);
            else PlayerPrefs.SetInt(k, n);
        }
        PlayerPrefs.DeleteKey("StoreMediaCapture");
        PlayerPrefs.DeleteKey("StoreMediaCaptureDone");
        PlayerPrefs.DeleteKey("StoreMediaOutDir");
        PlayerPrefs.Save();
    }
}
