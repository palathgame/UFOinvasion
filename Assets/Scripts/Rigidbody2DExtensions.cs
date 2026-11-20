using UnityEngine;

// Najde Rigidbody2D patrici k tomuto objektu (na nem, v detech nebo v rodici).
// Nahrazuje puvodni FindFirstObjectByType<Rigidbody2D>(), ktery vracel libovolne telo ve scene.
public static class Rigidbody2DExtensions
{
    public static Rigidbody2D OwnBody(this Component c)
    {
        var rb = c.GetComponent<Rigidbody2D>();
        if (rb == null) rb = c.GetComponentInChildren<Rigidbody2D>();
        if (rb == null) rb = c.GetComponentInParent<Rigidbody2D>();
        return rb;
    }

    public static void Push(this Component c, Vector2 force)
    {
        var rb = c.OwnBody();
        if (rb != null) rb.AddForce(force);
    }
}
