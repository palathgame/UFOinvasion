using UnityEngine;

// Jednoduche sprity generovane kodem (hvezda, kruh) - bez nutnosti importovat obrazky.
public static class ProceduralSprites
{
    private const int Size = 128;
    private static Texture2D starTex;
    private static Texture2D circleTex;
    private static readonly System.Collections.Generic.Dictionary<float, Sprite> starsBySize =
        new System.Collections.Generic.Dictionary<float, Sprite>();
    private static Sprite circle;

    // Bila peticipa hvezda (barva se nastavi pres SpriteRenderer.color / Image.color).
    // worldSize = velikost ve svetovych jednotkach.
    public static Sprite Star(float worldSize = 1f)
    {
        if (starTex == null) starTex = MakeTexture(InStar);
        float key = Mathf.Round(Mathf.Max(0.01f, worldSize) * 100f) / 100f;
        Sprite s;
        if (!starsBySize.TryGetValue(key, out s) || s == null)
        {
            s = Sprite.Create(starTex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size / key);
            starsBySize[key] = s;
        }
        return s;
    }

    public static Sprite Circle()
    {
        if (circleTex == null) circleTex = MakeTexture(InCircle);
        if (circle == null) circle = Sprite.Create(circleTex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        return circle;
    }

    private static Texture2D MakeTexture(System.Func<float, float, bool> inside)
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color32[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // 4x supersampling -> vyhlazene okraje
                int hits = 0;
                for (int sy = 0; sy < 2; sy++)
                    for (int sx = 0; sx < 2; sx++)
                    {
                        float u = (x + 0.25f + 0.5f * sx) / Size * 2f - 1f;
                        float v = (y + 0.25f + 0.5f * sy) / Size * 2f - 1f;
                        if (inside(u, v)) hits++;
                    }
                px[y * Size + x] = new Color32(255, 255, 255, (byte)(hits * 255 / 4));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    private static bool InCircle(float u, float v)
    {
        return u * u + v * v <= 0.92f * 0.92f;
    }

    private static bool InStar(float u, float v)
    {
        // Polygon hvezdy: 10 vrcholu, stridave vnejsi a vnitrni polomer, spicka nahoru.
        const float outer = 0.95f, inner = 0.42f;
        bool inside = false;
        for (int i = 0, j = 9; i < 10; j = i++)
        {
            Vector2 a = StarPoint(i, outer, inner);
            Vector2 b = StarPoint(j, outer, inner);
            if ((a.y > v) != (b.y > v) && u < (b.x - a.x) * (v - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    private static Vector2 StarPoint(int i, float outer, float inner)
    {
        float r = i % 2 == 0 ? outer : inner;
        float ang = Mathf.PI / 2f + i * Mathf.PI / 5f;
        return new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
    }
}
