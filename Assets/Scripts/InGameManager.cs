using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{

    private long KillAliens = 0;
    private string aktual;

    // Statistiky jedne hry (pro shrnuti na konci).
    private long coinsThisGame = 0;
    private long bossesThisGame = 0;

    // Kombo: zabiti v okne ComboWindow sekund za sebou zvysuji nasobic skore.
    private const float ComboWindow = 2f;
    private int combo = 0;
    private float lastKillTime = -100f;
    private int bestMultiplier = 1;

    private void Start()
    {
        if (FindAnyObjectByType<GameHud>() == null)
        {
            new GameObject("GameHud").AddComponent<GameHud>();
        }
        startGame();
        TutorialOverlay.ShowIfNeeded();
    }

    private void OnApplicationPause(bool paused)
    {
        // Na mobilu se aplikace na pozadi muze ukoncit bez OnApplicationQuit.
        if (paused) PlayerPrefs.Save();
    }

    public void EndGame() {
        SceneManager.LoadScene("StartPage");
    }


    public void plusKills() {

        KillAliens++;

    }

    public long getKillsAliens() {

        return KillAliens;
    }

    public void AddCoin() { coinsThisGame++; }
    public long GetCoinsThisGame() { return coinsThisGame; }

    public void AddBossKill() { bossesThisGame++; }
    public long GetBossesThisGame() { return bossesThisGame; }

    public int GetBestMultiplier() { return bestMultiplier; }

    // Mince za skore: 1 mince za kazdych PointsPerCoin bodu.
    public const long PointsPerCoin = 1000;
    private long scoreCoinsAwarded = 0;
    private bool coinsDoubled = false;

    // Pripise nove mince za skore (po pokracovani za reklamu jen rozdil) a vrati jejich pocet.
    public long AwardScoreCoins(long score)
    {
        long total = score / PointsPerCoin;
        long add = total - scoreCoinsAwarded;
        if (add <= 0) return 0;
        scoreCoinsAwarded = total;
        coinsThisGame += add;
        AddCoinsToWallet(add);
        return add;
    }

    public bool CanDoubleCoins { get { return !coinsDoubled && coinsThisGame > 0; } }

    // Zdvoji mince z teto hry (odmena za reklamu). Vrati pocet pridanych minci.
    public long DoubleCoins()
    {
        if (!CanDoubleCoins) return 0;
        coinsDoubled = true;
        long added = coinsThisGame;
        coinsThisGame += added;
        AddCoinsToWallet(added);
        return added;
    }

    public static void AddCoinsToWallet(long amount)
    {
        long coins;
        long.TryParse(PlayerPrefs.GetString("Coins"), out coins);
        PlayerPrefs.SetString("Coins", (coins + amount).ToString());
    }

    // Zaznamena zabiti a vrati nasobic skore, ktery se na nej pouzije.
    public int RegisterComboKill()
    {
        if (Time.time - lastKillTime <= ComboWindow) combo++;
        else combo = 1;
        lastKillTime = Time.time;

        int m = CurrentMultiplier;
        if (m > bestMultiplier) bestMultiplier = m;
        return m;
    }

    public int ComboCount
    {
        get { return (Time.time - lastKillTime <= ComboWindow) ? combo : 0; }
    }

    public int CurrentMultiplier
    {
        get
        {
            int c = ComboCount;
            if (c >= 10) return 3;
            if (c >= 5) return 2;
            return 1;
        }
    }

    // 1 = kombo prave zacalo, 0 = vyprsi.
    public float ComboTimeLeft01
    {
        get { return Mathf.Clamp01((ComboWindow - (Time.time - lastKillTime)) / ComboWindow); }
    }

    public void startGame(){

        aktual = PlayerPrefs.GetString("AktualSkin");
        if (aktual == "")
        {
            aktual = "0";
        }

        var m_NewPosition = new Vector2(-6.5f, 0.0f);
        var loadedObject = Resources.Load("plane/Plane" + aktual);
        Instantiate(loadedObject, m_NewPosition, Quaternion.identity);
    }

}
