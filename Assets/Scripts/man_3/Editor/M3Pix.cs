using System.IO;
using UnityEditor;
using UnityEngine;

// Bộ vẽ ảnh tạm (placeholder) cho màn 3. Tọa độ chuẩn hóa 0..1, gốc ở góc dưới trái.
// Họa sĩ chỉ cần thay file PNG cùng tên trong Assets/Sprites/man_3/placeholder là xong.
public class M3Pix
{
    public readonly int w, h;
    private readonly Color[] p;

    // Nhân độ dày mọi nét Line/Jagged – đặt 2 khi vẽ ảnh độ phân giải gấp đôi để nét giữ tỉ lệ
    public float lineScale = 1f;

    public M3Pix(int width, int height)
    {
        w = width;
        h = height;
        p = new Color[w * h];
    }

    private void Blend(int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        int i = y * w + x;
        Color dst = p[i];
        float a = c.a + dst.a * (1f - c.a);
        if (a <= 0f) { p[i] = Color.clear; return; }
        Color o = (c * c.a + dst * dst.a * (1f - c.a)) / a;
        o.a = a;
        p[i] = o;
    }

    public M3Pix Fill(Color c)
    {
        for (int i = 0; i < p.Length; i++) p[i] = c;
        return this;
    }

    public M3Pix VGradient(Color bottom, Color top)
    {
        for (int y = 0; y < h; y++)
        {
            Color c = Color.Lerp(bottom, top, y / (float)(h - 1));
            for (int x = 0; x < w; x++) Blend(x, y, c);
        }
        return this;
    }

    public M3Pix Rect(float x0, float y0, float x1, float y1, Color c)
    {
        int ax = Mathf.RoundToInt(x0 * w), bx = Mathf.RoundToInt(x1 * w);
        int ay = Mathf.RoundToInt(y0 * h), by = Mathf.RoundToInt(y1 * h);
        for (int y = ay; y < by; y++)
            for (int x = ax; x < bx; x++) Blend(x, y, c);
        return this;
    }

    public M3Pix Border(int px, Color c)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (x < px || y < px || x >= w - px || y >= h - px) Blend(x, y, c);
        return this;
    }

    // rx tính theo chiều rộng, ry theo chiều cao (cả hai chuẩn hóa)
    public M3Pix Ellipse(float cx, float cy, float rx, float ry, Color c, float soft = 0f, float noise = 0f)
    {
        float grow = 1f + noise;
        int x0 = Mathf.Max(0, Mathf.FloorToInt((cx - rx * grow) * w)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt((cx + rx * grow) * w));
        int y0 = Mathf.Max(0, Mathf.FloorToInt((cy - ry * grow) * h)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt((cy + ry * grow) * h));
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                float dx = (u - cx) / rx, dy = (v - cy) / ry;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (noise > 0f)
                {
                    float ang = Mathf.Atan2(dy, dx);
                    d += (Mathf.PerlinNoise(ang * 2.3f + 10f, cx * 7f) - 0.5f) * noise;
                }
                if (d > 1f) continue;
                Color col = c;
                if (soft > 0f) col.a *= Mathf.Clamp01((1f - d) / soft);
                Blend(x, y, col);
            }
        }
        return this;
    }

    // Đường thẳng dày thick (pixel)
    public M3Pix Line(float x0, float y0, float x1, float y1, float thick, Color c)
    {
        thick *= lineScale;
        Vector2 a = new Vector2(x0 * w, y0 * h), b = new Vector2(x1 * w, y1 * h);
        int minX = Mathf.FloorToInt(Mathf.Min(a.x, b.x) - thick), maxX = Mathf.CeilToInt(Mathf.Max(a.x, b.x) + thick);
        int minY = Mathf.FloorToInt(Mathf.Min(a.y, b.y) - thick), maxY = Mathf.CeilToInt(Mathf.Max(a.y, b.y) + thick);
        Vector2 ab = b - a;
        float len2 = Mathf.Max(ab.sqrMagnitude, 0.0001f);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(pt - a, ab) / len2);
                if (Vector2.Distance(pt, a + ab * t) <= thick * 0.5f) Blend(x, y, c);
            }
        }
        return this;
    }

    // Đường gấp khúc ngẫu nhiên (vết nứt, vết rách)
    public M3Pix Jagged(float x0, float y0, float x1, float y1, int segments, float wobble, float thick, Color c, int seed)
    {
        Random.State old = Random.state;
        Random.InitState(seed);
        Vector2 prev = new Vector2(x0, y0);
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 next = Vector2.Lerp(new Vector2(x0, y0), new Vector2(x1, y1), t);
            if (i < segments) next += new Vector2(Random.Range(-wobble, wobble), Random.Range(-wobble, wobble));
            Line(prev.x, prev.y, next.x, next.y, thick * (1f - t * 0.6f), c);
            prev = next;
        }
        Random.state = old;
        return this;
    }

    public M3Pix Vignette(float strength)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)w - 0.5f, v = y / (float)h - 0.5f;
                float d = Mathf.Clamp01(Mathf.Sqrt(u * u + v * v) * 1.6f);
                Blend(x, y, new Color(0f, 0f, 0f, d * d * strength));
            }
        }
        return this;
    }

    public M3Pix Grain(float amount, int seed)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (p[i].a <= 0f) continue;
                float n = (Mathf.PerlinNoise(x * 0.37f + seed, y * 0.37f) - 0.5f) * amount;
                p[i] = new Color(Mathf.Clamp01(p[i].r + n), Mathf.Clamp01(p[i].g + n), Mathf.Clamp01(p[i].b + n), p[i].a);
            }
        }
        return this;
    }

    // Đa giác lồi/lõm bất kỳ (tọa độ chuẩn hóa) – dùng cho mép rách, ván sàn phối cảnh...
    public M3Pix Polygon(Color c, params Vector2[] pts)
    {
        float minY = 1f, maxY = 0f, minX = 1f, maxX = 0f;
        foreach (Vector2 v in pts) { minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y); minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x); }
        int ya = Mathf.Max(0, Mathf.FloorToInt(minY * h)), yb = Mathf.Min(h - 1, Mathf.CeilToInt(maxY * h));
        int xa = Mathf.Max(0, Mathf.FloorToInt(minX * w)), xb = Mathf.Min(w - 1, Mathf.CeilToInt(maxX * w));
        for (int y = ya; y <= yb; y++)
        {
            for (int x = xa; x <= xb; x++)
            {
                float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                bool inside = false;
                for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                {
                    if ((pts[i].y > v) != (pts[j].y > v) &&
                        u < (pts[j].x - pts[i].x) * (v - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                        inside = !inside;
                }
                if (inside) Blend(x, y, c);
            }
        }
        return this;
    }

    // Nét mực kiểu hoạt hình vẽ tay như màn 1: tìm ranh giới giữa các mảng màu / mép trong suốt
    // rồi tô đen. radius = nửa độ dày nét (pixel). Gọi SAU khi tô mảng màu, TRƯỚC khi phủ vân màu nước.
    public M3Pix Ink(int radius, float threshold = 0.22f)
    {
        Color ink = new Color(0.07f, 0.05f, 0.05f, 1f);
        var src = (Color[])p.Clone();
        int r = Mathf.Max(1, radius);
        int[] ox = { r, -r, 0, 0, r, -r, r, -r };
        int[] oy = { 0, 0, r, -r, r, r, -r, -r };
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = src[y * w + x];
                bool edge = false;
                for (int k = 0; k < 8 && !edge; k++)
                {
                    int nx = Mathf.Clamp(x + ox[k], 0, w - 1), ny = Mathf.Clamp(y + oy[k], 0, h - 1);
                    Color n = src[ny * w + nx];
                    float d = Mathf.Abs(c.a - n.a);
                    if (c.a > 0.5f && n.a > 0.5f)
                        d += Mathf.Abs(c.r - n.r) + Mathf.Abs(c.g - n.g) + Mathf.Abs(c.b - n.b);
                    edge = d > threshold;
                }
                if (edge && (c.a > 0.5f || MaxAlphaAround(src, x, y, r) > 0.5f)) p[y * w + x] = ink;
            }
        }
        return this;
    }

    private float MaxAlphaAround(Color[] src, int x, int y, int r)
    {
        float m = 0f;
        for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                m = Mathf.Max(m, src[ny * w + nx].a);
            }
        return m;
    }

    // Vân màu nước loang lổ (giống nền ô túi đồ màn 1)
    public M3Pix Watercolor(float amount, int seed)
    {
        float s1 = 3.5f / Mathf.Max(w, h), s2 = 11f / Mathf.Max(w, h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (p[i].a <= 0f) continue;
                float n = Mathf.PerlinNoise(x * s1 + seed, y * s1 + seed * 0.7f) * 0.65f
                        + Mathf.PerlinNoise(x * s2 + seed * 1.3f, y * s2) * 0.35f;
                float k = 1f + (n - 0.5f) * amount * 2f;
                p[i] = new Color(Mathf.Clamp01(p[i].r * k), Mathf.Clamp01(p[i].g * k), Mathf.Clamp01(p[i].b * k), p[i].a);
            }
        }
        return this;
    }

    // Ghép một lớp khác (cùng kích thước) đè lên – dùng để vẽ mặt với nét mảnh riêng, không bị nét dày của thân làm đen mắt
    public M3Pix Over(M3Pix top)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = top.p[y * w + x];
                if (c.a > 0f) Blend(x, y, c);
            }
        return this;
    }

    // Chuyển sang tông ảnh cũ màu nâu đỏ (sepia)
    public M3Pix Sepia(float strength = 1f)
    {
        for (int i = 0; i < p.Length; i++)
        {
            Color c = p[i];
            if (c.a <= 0f) continue;
            float l = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
            Color s = new Color(Mathf.Clamp01(l * 1.08f + 0.04f), Mathf.Clamp01(l * 0.9f + 0.02f), Mathf.Clamp01(l * 0.68f), c.a);
            p[i] = Color.Lerp(c, s, strength);
        }
        return this;
    }

    // Lục địa giả bằng nhiễu Perlin (bản đồ)
    public M3Pix Continents(Color land, float threshold, float scale, int seed)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (Mathf.PerlinNoise(x * scale + seed, y * scale * 1.3f + seed) > threshold) Blend(x, y, land);
        return this;
    }

    public Sprite Save(string folder, string name, float pixelsPerUnit, Vector2? pivot = null, int sliceBorder = 0)
    {
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string path = folder + "/" + name + ".png";

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels(p);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = pivot.HasValue ? (int)SpriteAlignment.Custom : (int)SpriteAlignment.Center;
        settings.spritePivot = pivot ?? new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(settings);
        if (sliceBorder > 0) importer.spriteBorder = new Vector4(sliceBorder, sliceBorder, sliceBorder, sliceBorder); // 9-slice cho UI
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
