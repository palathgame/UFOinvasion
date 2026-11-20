using UnityEngine;
using UnityEngine.UI;

// Kratky navod pri prvni hre. Hra stoji, tuknuti = dalsi stranka, na konci odpocet 3-2-1.
public class TutorialOverlay : MonoBehaviour
{
    private const string DoneKey = "TutorialDone";

    private static readonly string[] Pages =
    {
        "Hold your finger on the screen.\nYour ship flies towards it.",
        "Shooting is automatic.\nDodge enemy shots and ships!",
        "<color=#66FF66>GREEN</color> power-ups help you,\n<color=#FF6666>RED</color> ones hurt you.\nCollect coins to buy new ships!",
        "Kill enemies quickly one after another\nfor a <color=#FFD633>COMBO</color> score multiplier!",
    };

    private Text pageText;
    private Text hintText;
    private int page;
    private float inputBlockedUntil;

    public static void ShowIfNeeded()
    {
        if (PlayerPrefs.GetString(DoneKey) == "1") return;
        new GameObject("Tutorial").AddComponent<TutorialOverlay>();
    }

    private void Start()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>(); // blokuje tlacitko pauzy pod navodem

        var dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(transform, false);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        pageText = MakeText("Page", 64, new Vector2(0, 40), new Vector2(1700, 500));
        hintText = MakeText("Hint", 36, new Vector2(0, -330), new Vector2(1200, 80));
        hintText.color = new Color(1f, 1f, 1f, 0.7f);

        Time.timeScale = 0f;
        ShowPage(0);
    }

    private Text MakeText(string name, int size, Vector2 pos, Vector2 box)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = go.AddComponent<Text>();
        t.font = GameHud.SharedFont;
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.supportRichText = true;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private void ShowPage(int i)
    {
        page = i;
        pageText.text = Pages[i];
        hintText.text = (i + 1) + " / " + Pages.Length + "   -   tap to continue";
        inputBlockedUntil = Time.unscaledTime + 0.4f; // aby jedno tuknuti nepreskocilo vic stranek
    }

    private void Update()
    {
        if (Time.unscaledTime < inputBlockedUntil) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (page + 1 < Pages.Length) ShowPage(page + 1);
        else Finish();
    }

    private void Finish()
    {
        PlayerPrefs.SetString(DoneKey, "1");
        PlayerPrefs.Save();
        Destroy(gameObject);
        GameHud.StartCountdown();
    }
}
