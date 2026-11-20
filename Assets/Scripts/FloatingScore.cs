using UnityEngine;

// Plovouci "+15" nad zabitym nepritelem: stoupa a mizi.
public class FloatingScore : MonoBehaviour
{
    private const float Life = 0.8f;
    private const float RiseSpeed = 1.2f;

    private TextMesh tm;
    private Color baseColor;
    private float t;

    public static void Spawn(Vector3 position, string text, Color color, float size = 1f)
    {
        var font = GameHud.SharedFont;
        if (font == null) return;

        var go = new GameObject("ScorePopup");
        go.transform.position = position + new Vector3(0f, 0.3f, 0f);
        var cam = Camera.main;
        if (cam != null) go.transform.rotation = cam.transform.rotation;

        var text3d = go.AddComponent<TextMesh>();
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        text3d.font = font;
        mr.sharedMaterial = font.material;
        mr.sortingOrder = 210;
        text3d.text = text;
        text3d.color = color;
        text3d.anchor = TextAnchor.MiddleCenter;
        text3d.alignment = TextAlignment.Center;
        text3d.fontSize = 60;
        text3d.characterSize = 0.05f * size;

        var fs = go.AddComponent<FloatingScore>();
        fs.tm = text3d;
        fs.baseColor = color;
    }

    private void Update()
    {
        t += Time.deltaTime;
        transform.position += Vector3.up * RiseSpeed * Time.deltaTime;
        var c = baseColor;
        c.a = Mathf.Clamp01(1f - t / Life);
        tm.color = c;
        if (t >= Life) Destroy(gameObject);
    }
}
