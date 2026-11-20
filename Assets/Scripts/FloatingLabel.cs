using UnityEngine;

// Popisek ve svete, ktery nasleduje cil (power-up) a zmizi s nim.
public class FloatingLabel : MonoBehaviour
{
    public static readonly Color Good = new Color(0.4f, 1f, 0.4f);
    public static readonly Color Bad = new Color(1f, 0.4f, 0.4f);
    public static readonly Color ShieldColor = new Color(0.4f, 0.88f, 1f);

    private Transform target;
    private Vector3 offset = new Vector3(0f, 0.6f, 0f);

    public static void Attach(Transform target, string text, Color color)
    {
        if (target == null) return;
        var font = GameHud.SharedFont;
        if (font == null) return;

        var go = new GameObject("BuffLabel");
        var tm = go.AddComponent<TextMesh>();
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();

        tm.font = font;
        mr.sharedMaterial = font.material;
        mr.sortingOrder = 200;
        tm.text = text;
        tm.color = color;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 60;
        tm.characterSize = 0.06f;

        var fl = go.AddComponent<FloatingLabel>();
        fl.target = target;
        fl.Follow();
    }

    private void LateUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = target.position + offset;
        // Kamera muze byt otocena (nastaveni Direction) -> text vzdy citelne.
        var cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}
