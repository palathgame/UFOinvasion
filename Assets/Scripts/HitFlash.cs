using System.Collections.Generic;
using UnityEngine;

// Kratke bile probliknuti spritu pri zasahu (pouziva Resources/flash.mat).
public class HitFlash : MonoBehaviour
{
    private static Material flashMat;
    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private readonly List<Material> originals = new List<Material>();
    private float until;
    private bool flashing;

    public static void Flash(GameObject go, float duration = 0.07f)
    {
        if (go == null) return;
        var hf = go.GetComponent<HitFlash>();
        if (hf == null) hf = go.AddComponent<HitFlash>();
        hf.Begin(duration);
    }

    private void Begin(float duration)
    {
        if (flashMat == null) flashMat = Resources.Load<Material>("flash");
        if (flashMat == null) return;

        if (!flashing)
        {
            renderers.Clear();
            originals.Clear();
            foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            {
                // Stit si materialy ridi sam.
                if (r.GetComponent<Shield>() != null) continue;
                renderers.Add(r);
                originals.Add(r.sharedMaterial);
                r.sharedMaterial = flashMat;
            }
            flashing = true;
        }
        until = Time.unscaledTime + duration;
    }

    private void Update()
    {
        if (flashing && Time.unscaledTime >= until) Restore();
    }

    private void OnDisable()
    {
        if (flashing) Restore();
    }

    private void Restore()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null) renderers[i].sharedMaterial = originals[i];
        }
        flashing = false;
    }
}

