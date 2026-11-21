using UnityEngine;

// Aktivni magnet na hraci: vsechny mince ve scene leti k nemu.
public class MagnetEffect : MonoBehaviour
{
    private const float PullSpeed = 10f;
    // Dokud magnet bezi, padaji mince castej (sance viz BuffDrops).
    private static MagnetEffect current;
    private float timeLeft;

    public float TimeLeft { get { return Mathf.Max(0f, timeLeft); } }
    public static bool IsActive { get { return current != null && current.timeLeft > 0f; } }

    public static void Activate(GameObject player, float duration)
    {
        var m = player.GetComponent<MagnetEffect>();
        if (m == null) m = player.AddComponent<MagnetEffect>();
        m.timeLeft = duration;
        current = m;
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
    }

    private void Update()
    {
        if (timeLeft <= 0f) return;
        timeLeft -= Time.deltaTime;

        foreach (var coin in FindObjectsByType<CoinScript>(FindObjectsSortMode.None))
        {
            var rb = coin.OwnBody();
            if (rb == null) continue;
            Vector2 dir = transform.position - coin.transform.position;
            rb.linearVelocity = dir.normalized * PullSpeed;
        }
    }
}
