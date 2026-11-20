using UnityEngine;

// Aktivni magnet na hraci: vsechny mince ve scene leti k nemu.
public class MagnetEffect : MonoBehaviour
{
    private const float PullSpeed = 10f;
    private float timeLeft;

    public float TimeLeft { get { return Mathf.Max(0f, timeLeft); } }

    public static void Activate(GameObject player, float duration)
    {
        var m = player.GetComponent<MagnetEffect>();
        if (m == null) m = player.AddComponent<MagnetEffect>();
        m.timeLeft = duration;
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
