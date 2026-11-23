using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Denni ukoly: kazdy den 3 ukoly ruznych typu (vyber je dany datem), odmena v mincich
// se pripise hned po splneni. Postup se hlasi na konci hry z ResultScore.ReportGame().
// PlayerPrefs (stringy): QuestDate "yyyy-MM-dd", QuestIds "3,7,10", QuestProgress "12,0,1", QuestDone "0,1,0".
public static class DailyQuests
{
    public enum Kind { Kills, Bosses, Score, Combo, Coins, Games }

    public struct Quest
    {
        public Kind kind;
        public long target;
        public int reward;
        public string text;
    }

    // Typy se po dvou stupnich: lehci a tezsi varianta.
    private static readonly Quest[] Pool =
    {
        new Quest { kind = Kind.Kills,  target = 60,   reward = 20, text = "Destroy 60 enemies" },
        new Quest { kind = Kind.Kills,  target = 150,  reward = 35, text = "Destroy 150 enemies" },
        new Quest { kind = Kind.Bosses, target = 1,    reward = 30, text = "Defeat a boss" },
        new Quest { kind = Kind.Bosses, target = 3,    reward = 50, text = "Defeat 3 bosses" },
        new Quest { kind = Kind.Score,  target = 2500, reward = 25, text = "Score 2500 in one game" },
        new Quest { kind = Kind.Score,  target = 6000, reward = 45, text = "Score 6000 in one game" },
        new Quest { kind = Kind.Combo,  target = 2,    reward = 20, text = "Reach combo x2" },
        new Quest { kind = Kind.Combo,  target = 3,    reward = 35, text = "Reach combo x3" },
        new Quest { kind = Kind.Coins,  target = 10,   reward = 20, text = "Earn 10 coins in games" },
        new Quest { kind = Kind.Coins,  target = 25,   reward = 35, text = "Earn 25 coins in games" },
        new Quest { kind = Kind.Games,  target = 3,    reward = 15, text = "Play 3 games" },
        new Quest { kind = Kind.Games,  target = 6,    reward = 25, text = "Play 6 games" },
    };

    public const int Count = 3;
    private const string DateKey = "QuestDate";
    private const string IdsKey = "QuestIds";
    private const string ProgressKey = "QuestProgress";
    private const string DoneKey = "QuestDone";

    private static int[] ids;
    private static long[] progress;
    private static bool[] done;
    private static string loadedDate;

    // Co uz bylo z aktualni hry nahlaseno (po pokracovani za reklamu se hlasi znovu soucty za celou hru).
    private static long repKills, repBosses, repCoins;
    private static bool repGame;

    // Editor bez domain reloadu: zahodit mezipamet mezi spustenimi Play Mode.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ids = null;
        loadedDate = null;
        BeginGame();
    }

    private static string Today()
    {
        return DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static Quest Get(int i) { Ensure(); return Pool[ids[i]]; }
    public static long Progress(int i) { Ensure(); return Math.Min(progress[i], Pool[ids[i]].target); }
    public static bool IsDone(int i) { Ensure(); return done[i]; }

    public static int DoneCount()
    {
        Ensure();
        int n = 0;
        for (int i = 0; i < Count; i++) if (done[i]) n++;
        return n;
    }

    // Volat na zacatku kazde hry (InGameManager.Start).
    public static void BeginGame()
    {
        repKills = repBosses = repCoins = 0;
        repGame = false;
    }

    // Nahlasi soucty za aktualni hru. Vrati splnene ukoly (odmena uz je v penezence).
    public static List<Quest> ReportGame(long kills, long bosses, long coins, long score, int bestMultiplier)
    {
        Ensure();
        long dKills = Math.Max(0, kills - repKills);
        long dBosses = Math.Max(0, bosses - repBosses);
        long dCoins = Math.Max(0, coins - repCoins);
        long dGames = repGame ? 0 : 1;
        repKills = kills;
        repBosses = bosses;
        repCoins = coins;
        repGame = true;

        var completed = new List<Quest>();
        for (int i = 0; i < Count; i++)
        {
            if (done[i]) continue;
            Quest q = Pool[ids[i]];
            switch (q.kind)
            {
                case Kind.Kills: progress[i] += dKills; break;
                case Kind.Bosses: progress[i] += dBosses; break;
                case Kind.Coins: progress[i] += dCoins; break;
                case Kind.Games: progress[i] += dGames; break;
                case Kind.Score: progress[i] = Math.Max(progress[i], score); break;
                case Kind.Combo: progress[i] = Math.Max(progress[i], bestMultiplier); break;
            }
            if (progress[i] >= q.target)
            {
                done[i] = true;
                InGameManager.AddCoinsToWallet(q.reward);
                completed.Add(q);
            }
        }
        Save();
        return completed;
    }

    // Nacte dnesni ukoly, pripadne vylosuje nove (i kdyz pres pulnoc bezi hra).
    private static void Ensure()
    {
        string today = Today();
        if (loadedDate == today && ids != null) return;
        loadedDate = today;

        if (PlayerPrefs.GetString(DateKey) == today && TryLoad()) return;

        ids = Roll(today);
        progress = new long[Count];
        done = new bool[Count];
        PlayerPrefs.SetString(DateKey, today);
        Save();
    }

    private static int[] Roll(string date)
    {
        int seed;
        int.TryParse(date.Replace("-", ""), out seed);
        var rnd = new System.Random(seed * 7919);
        int kinds = Pool.Length / 2;
        var kindOrder = new List<int>();
        for (int k = 0; k < kinds; k++) kindOrder.Insert(rnd.Next(kindOrder.Count + 1), k);
        var result = new int[Count];
        for (int i = 0; i < Count; i++) result[i] = kindOrder[i] * 2 + rnd.Next(2);
        return result;
    }

    private static bool TryLoad()
    {
        string[] a = PlayerPrefs.GetString(IdsKey).Split(',');
        string[] b = PlayerPrefs.GetString(ProgressKey).Split(',');
        string[] c = PlayerPrefs.GetString(DoneKey).Split(',');
        if (a.Length != Count || b.Length != Count || c.Length != Count) return false;

        ids = new int[Count];
        progress = new long[Count];
        done = new bool[Count];
        for (int i = 0; i < Count; i++)
        {
            if (!int.TryParse(a[i], out ids[i]) || ids[i] < 0 || ids[i] >= Pool.Length) return false;
            long.TryParse(b[i], out progress[i]);
            done[i] = c[i] == "1";
        }
        return true;
    }

    private static void Save()
    {
        var a = new string[Count];
        var b = new string[Count];
        var c = new string[Count];
        for (int i = 0; i < Count; i++)
        {
            a[i] = ids[i].ToString();
            b[i] = progress[i].ToString();
            c[i] = done[i] ? "1" : "0";
        }
        PlayerPrefs.SetString(IdsKey, string.Join(",", a));
        PlayerPrefs.SetString(ProgressKey, string.Join(",", b));
        PlayerPrefs.SetString(DoneKey, string.Join(",", c));
        PlayerPrefs.Save();
    }
}
