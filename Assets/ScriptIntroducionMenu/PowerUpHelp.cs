using UnityEngine;
using UnityEngine.UI;

// Tlacitko "?" v hlavnim menu (vpravo nahore) -> prehled power-upu vcetne magnetu.
// Vytvari ho StartPageManager.Start(); nic se nenastavuje ve scene.
public class PowerUpHelp : MonoBehaviour
{
    private struct Entry
    {
        public bool star;
        public Color color;
        public string title;
        public string text;
    }

    private static readonly Entry[] Entries =
    {
        new Entry { color = new Color(0.4f, 1f, 0.4f), title = "GREEN power-up", text = "Bonus: faster shooting, more damage or healing." },
        new Entry { color = new Color(1f, 0.4f, 0.4f), title = "RED power-up", text = "Penalty: slower shooting, less damage or lost HP. Avoid it!" },
        new Entry { color = new Color(1f, 0.85f, 0.2f), title = "COIN", text = "Buy new ships. +1 coin per 1000 points. Combo x2 / x3 or MAGNET = more coins drop." },
    };

    private Font font;
    private GameObject panel;

    // menuElement = libovolny prvek hlavniho menu (podle nej se najde jeho canvas).
    public static void Create(Component menuElement)
    {
        if (menuElement == null) return;
        Canvas canvas = menuElement.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        var go = new GameObject("PowerUpHelp", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var help = go.AddComponent<PowerUpHelp>();
        var text = menuElement.GetComponentInChildren<Text>(true);
        help.font = text != null && text.font != null ? text.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        help.BuildButton();
    }

    // Tlacitko je primo v canvasu menu -> okna otevrena pozdeji (nastaveni, skiny...) jsou nad nim.
    private void BuildButton()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-90, -80);
        rt.sizeDelta = new Vector2(110, 110);

        var img = gameObject.AddComponent<Image>();
        img.sprite = ProceduralSprites.Circle();
        img.color = new Color(0.1f, 0.12f, 0.25f, 0.9f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f);
        outline.effectDistance = new Vector2(3, -3);
        gameObject.AddComponent<Button>().onClick.AddListener(Open);

        MakeText(transform, "?", 72, Color.white, Vector2.zero, new Vector2(110, 110), TextAnchor.MiddleCenter);
    }

    private void Open()
    {
        if (panel != null) return;

        panel = new GameObject("PowerUpHelpPanel");
        var canvas = panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 260;
        var scaler = panel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        panel.AddComponent<GraphicRaycaster>();

        // Tuknuti kamkoli zavre.
        var dim = MakeRect(panel.transform, "Dim", Vector2.zero, Vector2.zero);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        dim.AddComponent<Button>().onClick.AddListener(() => Destroy(panel));

        var box = MakeRect(panel.transform, "Box", Vector2.zero, new Vector2(1300, 600));
        var boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.08f, 0.1f, 0.2f, 0.95f);
        boxImg.raycastTarget = false;
        var o = box.AddComponent<Outline>();
        o.effectColor = new Color(1f, 0.85f, 0.2f);
        o.effectDistance = new Vector2(4, -4);

        MakeText(box.transform, "POWER-UPS", 64, new Color(1f, 0.85f, 0.2f), new Vector2(0, 220), new Vector2(1200, 90), TextAnchor.MiddleCenter);

        float y = 90;
        foreach (var e in Entries)
        {
            var icon = MakeRect(box.transform, "Icon", new Vector2(-540, y), new Vector2(90, 90));
            var iconImg = icon.AddComponent<Image>();
            iconImg.sprite = e.star ? ProceduralSprites.Star() : ProceduralSprites.Circle();
            iconImg.color = e.color;
            iconImg.raycastTarget = false;

            string line = "<color=#" + ColorUtility.ToHtmlStringRGB(e.color) + "><b>" + e.title + "</b></color>\n" + e.text;
            MakeText(box.transform, line, 36, Color.white, new Vector2(80, y), new Vector2(1080, 110), TextAnchor.MiddleLeft);
            y -= 125;
        }

        MakeText(box.transform, "Kill enemies quickly in a row for a COMBO score multiplier!   (tap to close)", 28,
            new Color(1f, 1f, 1f, 0.7f), new Vector2(0, -255), new Vector2(1250, 60), TextAnchor.MiddleCenter);

        // Zmena souhlasu s reklamami - Google ji vyzaduje tam, kde plati GDPR.
        if (AdsConsent.PrivacyOptionsRequired)
        {
            var btn = MakeRect(panel.transform, "PrivacySettings", new Vector2(0, -345), new Vector2(420, 80));
            var img = btn.AddComponent<Image>();
            img.color = new Color(1f, 0.85f, 0.2f);
            btn.AddComponent<Button>().onClick.AddListener(() =>
            {
                Destroy(panel);
                AdsConsent.ShowPrivacyOptions();
            });
            MakeText(btn.transform, "Privacy settings", 36, new Color(0.1f, 0.1f, 0.15f), Vector2.zero, new Vector2(420, 80), TextAnchor.MiddleCenter);
        }
    }

    private void OnDestroy()
    {
        if (panel != null) Destroy(panel);
    }

    private static GameObject MakeRect(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    private Text MakeText(Transform parent, string value, int size, Color color, Vector2 pos, Vector2 box, TextAnchor align)
    {
        var go = MakeRect(parent, "Text", pos, box);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.supportRichText = true;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
