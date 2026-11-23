using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Oznameni nahore na obrazovce "QUEST COMPLETE" po splneni denniho ukolu.
public class QuestToast : MonoBehaviour
{
    private List<DailyQuests.Quest> quests;

    public static void Show(List<DailyQuests.Quest> completed)
    {
        if (completed == null || completed.Count == 0) return;
        var go = new GameObject("QuestToast");
        go.AddComponent<QuestToast>().quests = completed;
    }

    private void Start()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 280;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = null;
        foreach (var t in FindObjectsByType<Text>(FindObjectsSortMode.None))
            if (t.font != null) { font = t.font; break; }
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        float h = 90 + 55 * quests.Count;
        var box = new GameObject("Box", typeof(RectTransform));
        box.transform.SetParent(transform, false);
        var rt = box.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -20);
        rt.sizeDelta = new Vector2(900, h);
        var img = box.AddComponent<Image>();
        img.color = new Color(0.08f, 0.1f, 0.2f, 0.95f);
        img.raycastTarget = false;
        var o = box.AddComponent<Outline>();
        o.effectColor = new Color(0.4f, 1f, 0.4f);
        o.effectDistance = new Vector2(4, -4);

        string text = "<size=52><color=#66FF66><b>QUEST COMPLETE!</b></color></size>";
        foreach (var q in quests)
            text += "\n<size=38>" + q.text + "   <color=#FFD633>+" + q.reward + " coins</color></size>";

        var tgo = new GameObject("Text", typeof(RectTransform));
        tgo.transform.SetParent(box.transform, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20, 10);
        trt.offsetMax = new Vector2(-20, -10);
        var txt = tgo.AddComponent<Text>();
        txt.font = font;
        txt.text = text;
        txt.supportRichText = true;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.raycastTarget = false;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        StartCoroutine(Life(box.AddComponent<CanvasGroup>()));
    }

    // Nerealny cas: na konci hry muze byt Time.timeScale = 0.
    private IEnumerator Life(CanvasGroup g)
    {
        for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime) { g.alpha = t / 0.3f; yield return null; }
        g.alpha = 1f;
        yield return new WaitForSecondsRealtime(3.5f);
        for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime) { g.alpha = 1f - t / 0.5f; yield return null; }
        Destroy(gameObject);
    }
}
