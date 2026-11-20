using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Denni odmena v menu: prvni spusteni za den = mince, serie dnu po sobe = vic minci.
public class DailyReward : MonoBehaviour
{
    private const string DateKey = "DailyRewardDate";
    private const string StreakKey = "DailyRewardStreak";
    private static readonly int[] Rewards = { 10, 15, 20, 25, 30, 40, 50 }; // den 1..7+

    private int streak;
    private int reward;

    public static void CheckAndShow()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string last = PlayerPrefs.GetString(DateKey);
        if (last == today) return;

        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        int streak;
        int.TryParse(PlayerPrefs.GetString(StreakKey), out streak);
        streak = last == yesterday ? streak + 1 : 1;
        int reward = Rewards[Mathf.Min(streak, Rewards.Length) - 1];

        long coins;
        long.TryParse(PlayerPrefs.GetString("Coins"), out coins);
        PlayerPrefs.SetString("Coins", (coins + reward).ToString());
        PlayerPrefs.SetString(DateKey, today);
        PlayerPrefs.SetString(StreakKey, streak.ToString());
        PlayerPrefs.Save();

        foreach (var ct in FindObjectsByType<CoinsText>(FindObjectsSortMode.None)) ct.updateCoins();

        var dr = new GameObject("DailyReward").AddComponent<DailyReward>();
        dr.streak = streak;
        dr.reward = reward;
    }

    private void Start()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = FindFont();

        // Cela obrazovka je tlacitko -> tuknuti kamkoli zavre.
        var dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(transform, false);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        dim.AddComponent<Button>().onClick.AddListener(() => Destroy(gameObject));

        var panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(800, 460);
        var pimg = panel.AddComponent<Image>();
        pimg.color = new Color(0.08f, 0.1f, 0.2f, 0.95f);
        pimg.raycastTarget = false;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f);
        outline.effectDistance = new Vector2(4, -4);

        MakeText(panel.transform, font, "DAILY REWARD", 64, new Color(1f, 0.85f, 0.2f), new Vector2(0, 150));
        MakeText(panel.transform, font, "Day " + streak + (streak >= Rewards.Length ? "" : " / " + Rewards.Length), 40, Color.white, new Vector2(0, 60));
        MakeText(panel.transform, font, "+" + reward + " coins", 80, new Color(0.4f, 1f, 0.4f), new Vector2(0, -40));
        MakeText(panel.transform, font, "Come back tomorrow for more!  (tap to close)", 30, new Color(1f, 1f, 1f, 0.7f), new Vector2(0, -170));
    }

    private static Font FindFont()
    {
        foreach (var t in FindObjectsByType<Text>(FindObjectsSortMode.None))
        {
            if (t.font != null) return t.font;
        }
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static void MakeText(Transform parent, Font font, string value, int size, Color color, Vector2 pos)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(780, 100);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
    }
}
