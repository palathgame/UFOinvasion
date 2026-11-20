using UnityEngine;

// Power-up "MAGNET" (zelena hvezda): po sebrani pritahuje mince k hraci (viz MagnetEffect).
// Nema vlastni prefab: vyrobi se z prefabu stitu, kteremu se vymeni skript a sprite.
public class MagnetBuff : MonoBehaviour
{
    public const float Duration = 8f;
    public static readonly Color Tint = new Color(0.3f, 1f, 0.4f);

    public static void Spawn(GameObject template, Vector3 pos)
    {
        if (template == null) return;

        // Instance pod neaktivnim rodicem -> OnEnable puvodnich skriptu se nespusti.
        var holder = new GameObject("MagnetHolder");
        holder.SetActive(false);
        GameObject go = Object.Instantiate(template, holder.transform);

        // Spawn probiha behem fyzikalni kolize -> DestroyImmediate nejde.
        // Vypnuta komponenta nedostane OnEnable, smaze se na konci snimku.
        foreach (var b in go.GetComponentsInChildren<BuffScript>(true))
        {
            b.enabled = false;
            Destroy(b);
        }
        foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.tag = "Untagged";
        foreach (var r in go.GetComponentsInChildren<SpriteRenderer>(true)) MakeStar(r);

        var rb = go.GetComponentInChildren<Rigidbody2D>(true);
        (rb != null ? rb.gameObject : go).AddComponent<MagnetBuff>();

        go.transform.position = pos;
        go.transform.SetParent(null, true); // aktivuje objekt
        Destroy(holder);
    }

    // Vymeni sprite za hvezdu stejne velikosti jako puvodni.
    private static void MakeStar(SpriteRenderer r)
    {
        float size = r.sprite != null ? Mathf.Max(r.sprite.bounds.size.x, r.sprite.bounds.size.y) : 1f;
        // Velikost resi pixelsPerUnit spritu, ne meritko -> collider zustane stejny.
        r.sprite = ProceduralSprites.Star(size * 1.2f);
        r.color = Tint;
    }

    private void OnEnable()
    {
        this.Push(Vector2.left * (Random.Range(0, 150) + 250));
        Destroy(transform.root.gameObject, 6);
        FloatingLabel.Attach(transform, "MAGNET", Tint);
        // Pomala rotace, aby byla hvezda videt (pokud ji Rigidbody2D nema zamcenou).
        var rb = this.OwnBody();
        if (rb != null) rb.angularVelocity = 90f;
    }
}
