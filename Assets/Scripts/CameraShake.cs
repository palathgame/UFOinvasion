using UnityEngine;

// Zatreseni hlavni kamerou. Po skonceni vrati kameru presne na puvodni misto.
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private float remaining;
    private float strength;
    private Vector3 basePos;
    private bool shaking;

    public static void Shake(float strength, float duration)
    {
        var cam = Camera.main;
        if (cam == null) return;
        if (instance == null || instance.gameObject != cam.gameObject)
        {
            instance = cam.GetComponent<CameraShake>();
            if (instance == null) instance = cam.gameObject.AddComponent<CameraShake>();
        }
        instance.Begin(strength, duration);
    }

    private void Begin(float s, float d)
    {
        if (!shaking)
        {
            basePos = transform.localPosition;
            shaking = true;
        }
        strength = Mathf.Max(strength, s);
        remaining = Mathf.Max(remaining, d);
    }

    private void LateUpdate()
    {
        if (!shaking) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            transform.localPosition = basePos;
            shaking = false;
            strength = 0f;
            return;
        }
        Vector2 offset = Random.insideUnitCircle * strength;
        transform.localPosition = basePos + new Vector3(offset.x, offset.y, 0f);
    }
}

