using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// HUD vytvareny za behu (neni potreba nic nastavovat ve scene):
// varovani pred bossem, kombo, aktivni buffy, varovani pri nizkem HP a odpocet 3-2-1.
// Vytvari ho InGameManager.Start().
public class GameHud : MonoBehaviour
{
    private const float LowHpRatio = 0.25f;

    private static GameHud instance;
    private static Font sharedFont;

    private GameObject root;
    private Text warningText;
    private Text comboText;
    private Text buffText;
    private CanvasGroup lowHp;

    // Odpocet je na vlastnim canvasu, aby byl videt i kdyz je HUD schovany.
    private Text countdownText;
    private bool counting;

    private float warningUntil;
    private InGameManager gm;
    private PlaneShip player;
    private Attack playerAttack;
    private Shield playerShield;
    private float nextPlayerSearch;

    // Font ze hry (z textu se skore), jinak vestaveny.
    public static Font SharedFont
    {
        get
        {
            if (sharedFont == null)
            {
                var score = FindAnyObjectByType<Score>();
                var text = score != null ? score.GetComponent<Text>() : null;
                sharedFont = text != null && text.font != null
                    ? text.font
                    : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return sharedFont;
        }
    }

    public static void ShowBossWarning(float seconds)
    {
        if (instance != null) instance.warningUntil = Time.time + Mathf.Max(1.5f, seconds);
    }

    // Boss dorazil -> schovat varovani.
    public static void TrackBoss(EnemyShip b)
    {
        if (instance == null) return;
        instance.warningUntil = 0f;
    }

    public static bool IsCountingDown
    {
        get { return instance != null && instance.counting; }
    }

    // Zastavi hru, odpocita 3-2-1-GO! a pak hru pusti (Time.timeScale = 1).
    public static void StartCountdown()
    {
        if (instance == null)
        {
            Time.timeScale = 1f;
            return;
        }
        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.CountdownRoutine());
    }

    private void Awake()
    {
        instance = this;
        Build();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private static GameObject MakeCanvas(string name, Transform parent, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        // Bez GraphicRaycasteru -> HUD neblokuje dotyky.
        return go;
    }

    private void Build()
    {
        root = MakeCanvas("HudCanvas", transform, -1); // pod ostatnimi menu (BackMenu apod.)

        BuildLowHpVignette();

        warningText = MakeText(root.transform, "BossWarning", "BOSS INCOMING!", 96, new Color(1f, 0.25f, 0.2f),
            new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(1500, 180), TextAnchor.MiddleCenter);

        comboText = MakeText(root.transform, "Combo", "", 60, new Color(1f, 0.85f, 0.2f),
            new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(700, 90), TextAnchor.MiddleCenter);

        buffText = MakeText(root.transform, "Buffs", "", 36, Color.white,
            new Vector2(0f, 0f), new Vector2(380, 130), new Vector2(700, 220), TextAnchor.LowerLeft);
        buffText.supportRichText = true;

        warningText.gameObject.SetActive(false);
        comboText.gameObject.SetActive(false);

        var overlay = MakeCanvas("CountdownCanvas", transform, 100);
        countdownText = MakeText(overlay.transform, "Countdown", "", 200, Color.white,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 300), TextAnchor.MiddleCenter);
        countdownText.gameObject.SetActive(false);
    }

    // Cerveny pulzujici okraj obrazovky. Kazdy okraj = 3 prekryvajici se pruhy (jednoduchy gradient).
    private void BuildLowHpVignette()
    {
        var go = new GameObject("LowHp", typeof(RectTransform));
        go.transform.SetParent(root.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        lowHp = go.AddComponent<CanvasGroup>();
        lowHp.blocksRaycasts = false;
        lowHp.interactable = false;

        var color = new Color(1f, 0f, 0f, 0.22f);
        for (int k = 1; k <= 3; k++)
        {
            float w = 35f * k;
            MakeEdge(go.transform, color, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, w)); // nahore
            MakeEdge(go.transform, color, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, w)); // dole
            MakeEdge(go.transform, color, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(w, 0)); // vlevo
            MakeEdge(go.transform, color, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(w, 0)); // vpravo
        }
        go.SetActive(false);
    }

    private static void MakeEdge(Transform parent, Color color, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size)
    {
        var e = new GameObject("Edge", typeof(RectTransform));
        e.transform.SetParent(parent, false);
        var rt = e.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        var img = e.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private Text MakeText(Transform parent, string name, string value, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 box, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = go.AddComponent<Text>();
        t.font = SharedFont;
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
        return t;
    }

    private IEnumerator CountdownRoutine()
    {
        counting = true;
        Time.timeScale = 0f;
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return Pulse(0.7f);
        }

        countdownText.text = "GO!";
        Time.timeScale = 1f;
        counting = false;
        yield return Pulse(0.5f);
        countdownText.gameObject.SetActive(false);
    }

    private IEnumerator Pulse(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float s = 1.4f - 0.4f * Mathf.Clamp01(t / duration);
            countdownText.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    private void Update()
    {
        if (gm == null) gm = FindAnyObjectByType<InGameManager>();
        FindPlayer();

        // Po smrti hrace (BackMenu) HUD schovat.
        bool alive = player != null;
        if (root.activeSelf != alive) root.SetActive(alive);
        if (!alive) return;

        UpdateWarning();
        UpdateCombo();
        UpdateBuffs();
        UpdateLowHp();
    }

    private void FindPlayer()
    {
        if (player != null || Time.unscaledTime < nextPlayerSearch) return;
        nextPlayerSearch = Time.unscaledTime + 0.25f;
        player = FindAnyObjectByType<PlaneShip>();
        if (player != null)
        {
            playerAttack = player.GetComponent<Attack>();
            playerShield = player.shieldBuff != null ? player.shieldBuff.GetComponent<Shield>() : null;
        }
    }

    private void UpdateWarning()
    {
        bool show = Time.time < warningUntil;
        if (warningText.gameObject.activeSelf != show) warningText.gameObject.SetActive(show);
        if (show)
        {
            var c = warningText.color;
            c.a = 0.35f + 0.65f * Mathf.PingPong(Time.time * 3f, 1f);
            warningText.color = c;
        }
    }

    private void UpdateCombo()
    {
        int m = gm != null ? gm.CurrentMultiplier : 1;
        bool show = m > 1;
        if (comboText.gameObject.activeSelf != show) comboText.gameObject.SetActive(show);
        if (!show) return;

        comboText.text = "COMBO x" + m + "  (" + gm.ComboCount + ")";
        float pulse = 1f + 0.08f * gm.ComboTimeLeft01;
        comboText.transform.localScale = new Vector3(pulse, pulse, 1f);
    }

    private void UpdateBuffs()
    {
        var sb = new StringBuilder();
        if (playerAttack != null)
        {
            float speed = playerAttack.SpeedBuff;
            if (speed != 0)
                AppendBuff(sb, speed < 0 ? "FIRE RATE +" : "FIRE RATE -", speed < 0, playerAttack.SpeedBuffTimeLeft);

            int dmg = playerAttack.DmgBuff;
            if (dmg != 0)
                AppendBuff(sb, dmg > 0 ? "DMG +" + dmg : "DMG " + dmg, dmg > 0, playerAttack.DmgBuffTimeLeft);
        }
        if (playerShield != null && playerShield.TimeLeft > 0)
        {
            sb.Append("<color=#66E0FF>SHIELD</color>  ").Append(Mathf.CeilToInt(playerShield.TimeLeft)).Append("s\n");
        }
        var magnet = player.GetComponent<MagnetEffect>();
        if (magnet != null && magnet.TimeLeft > 0)
        {
            sb.Append("<color=#FF73FF>MAGNET</color> <color=#FFD633>more coins</color>  ").Append(Mathf.CeilToInt(magnet.TimeLeft)).Append("s\n");
        }
        buffText.text = sb.ToString();
    }

    private void UpdateLowHp()
    {
        int max = player.MaxHp > 0 ? player.MaxHp : 100;
        bool show = player.hp > 0 && player.hp <= max * LowHpRatio;
        if (lowHp.gameObject.activeSelf != show) lowHp.gameObject.SetActive(show);
        if (show) lowHp.alpha = 0.45f + 0.55f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
    }

    private static void AppendBuff(StringBuilder sb, string label, bool good, float timeLeft)
    {
        sb.Append(good ? "<color=#66FF66>" : "<color=#FF6666>").Append(label).Append("</color>  ")
          .Append(Mathf.CeilToInt(timeLeft)).Append("s\n");
    }
}
