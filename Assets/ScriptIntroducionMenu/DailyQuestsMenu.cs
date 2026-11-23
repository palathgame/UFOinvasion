using UnityEngine;
using UnityEngine.UI;

// Tlacitko denních ukolu v hlavnim menu (vlevo od "?") -> okno s ukoly a jejich postupem.
// Vytvari ho StartPageManager.Start(); nic se nenastavuje ve scene.
public class DailyQuestsMenu : MonoBehaviour
{
    private static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
    private static readonly Color Green = new Color(0.4f, 1f, 0.4f);

    private Font font;
    private Text countText;
    private GameObject panel;

    public static void Create(Component menuElement)
    {
        if (menuElement == null) return;
        Canvas canvas = menuElement.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        var go = new GameObject("DailyQuestsMenu", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var menu = go.AddComponent<DailyQuestsMenu>();
        var text = menuElement.GetComponentInChildren<Text>(true);
        menu.font = text != null && text.font != null ? text.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        menu.BuildButton();
    }

    private void BuildButton()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-225, -80); // "?" je na -90, sirka 110
        rt.sizeDelta = new Vector2(110, 110);

        var img = gameObject.AddComponent<Image>();
        img.sprite = ProceduralSprites.Circle();
        img.color = new Color(0.1f, 0.12f, 0.25f, 0.9f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(3, -3);
        gameObject.AddComponent<Button>().onClick.AddListener(Open);

        var star = MakeRect(transform, "Star", new Vector2(0, 8), new Vector2(64, 64));
        var starImg = star.AddComponent<Image>();
        starImg.sprite = ProceduralSprites.Star();
        starImg.color = Gold;
        starImg.raycastTarget = false;

        countText = MakeText(transform, "", 26, Color.white, new Vector2(0, -36), new Vector2(110, 34), TextAnchor.MiddleCenter);
        Refresh();
    }

    // Po navratu z pozadi muze byt novy den -> nove ukoly.
    private void OnApplicationPause(bool paused)
    {
        if (!paused) Refresh();
    }

    private void Refresh()
    {
        if (countText != null) countText.text = DailyQuests.DoneCount() + "/" + DailyQuests.Count;
    }

    private void Open()
    {
        if (panel != null) return;
        Refresh();

        panel = new GameObject("DailyQuestsPanel");
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

        var box = MakeRect(panel.transform, "Box", Vector2.zero, new Vector2(1200, 640));
        var boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.08f, 0.1f, 0.2f, 0.95f);
        boxImg.raycastTarget = false;
        var o = box.AddComponent<Outline>();
        o.effectColor = Gold;
        o.effectDistance = new Vector2(4, -4);

        MakeText(box.transform, "DAILY QUESTS", 64, Gold, new Vector2(0, 240), new Vector2(1100, 90), TextAnchor.MiddleCenter);

        float y = 100;
        for (int i = 0; i < DailyQuests.Count; i++)
        {
            DailyQuests.Quest q = DailyQuests.Get(i);
            long p = DailyQuests.Progress(i);
            bool isDone = DailyQuests.IsDone(i);

            var icon = MakeRect(box.transform, "Icon", new Vector2(-500, y), new Vector2(80, 80));
            var iconImg = icon.AddComponent<Image>();
            iconImg.sprite = ProceduralSprites.Star();
            iconImg.color = isDone ? Green : new Color(1f, 1f, 1f, 0.25f);
            iconImg.raycastTarget = false;

            string title = isDone ? "<color=#66FF66>" + q.text + "</color>" : q.text;
            MakeText(box.transform, title, 40, Color.white, new Vector2(-50, y + 22), new Vector2(800, 56), TextAnchor.MiddleLeft);
            MakeText(box.transform, "+" + q.reward + " coins", 36, Gold, new Vector2(430, y), new Vector2(240, 56), TextAnchor.MiddleRight);

            // Ukazatel postupu.
            const float barW = 800f;
            var bg = MakeRect(box.transform, "BarBg", new Vector2(-50, y - 26), new Vector2(barW, 22));
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.12f);
            bgImg.raycastTarget = false;
            float f = q.target > 0 ? Mathf.Clamp01((float)p / q.target) : 1f;
            if (f > 0f)
            {
                var fill = MakeRect(bg.transform, "Fill", Vector2.zero, Vector2.zero);
                var frt = fill.GetComponent<RectTransform>();
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = new Vector2(f, 1f);
                frt.offsetMin = frt.offsetMax = Vector2.zero;
                var fillImg = fill.AddComponent<Image>();
                fillImg.color = isDone ? Green : Gold;
                fillImg.raycastTarget = false;
            }
            string status = isDone ? "DONE" : ProgressLabel(q, p);
            MakeText(box.transform, status, 30, new Color(1f, 1f, 1f, 0.8f), new Vector2(430, y - 30), new Vector2(240, 44), TextAnchor.MiddleRight);

            y -= 130;
        }

        MakeText(box.transform, "New quests every day. Rewards are added automatically.   (tap to close)", 28,
            new Color(1f, 1f, 1f, 0.7f), new Vector2(0, -275), new Vector2(1150, 60), TextAnchor.MiddleCenter);
    }

    private static string ProgressLabel(DailyQuests.Quest q, long p)
    {
        if (q.kind == DailyQuests.Kind.Combo) return p >= 2 ? "best x" + p : "-";
        return p + " / " + q.target;
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
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
