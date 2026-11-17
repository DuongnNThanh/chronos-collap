using System.Collections.Generic;
using UnityEngine;

// Vẽ toàn bộ ảnh tạm của màn 3 theo phong cách màn 1:
// mảng màu phẳng tông trầm + nét viền mực đen + vân màu nước loang.
// Ảnh được lưu vào Assets/Sprites/man_3/placeholder – họa sĩ ghi đè file cùng tên bằng ảnh thật.
public static class M3PlaceholderArt
{
    public const string Dir = "Assets/Sprites/man_3/placeholder";

    // ---- Dữ liệu câu đố bản đồ (dùng chung cho ảnh và nhãn chữ trong panel) ----
    // 5 dấu con mắt trên bản đồ, đánh số La Mã I..V, mỗi dấu có số vạch chữ thập = 1 chữ số.
    public static readonly string[] MapRoman = { "I", "II", "III", "IV", "V" };
    public static readonly Vector2[] MapMarks =
    {
        new Vector2(0.17f, 0.60f), new Vector2(0.34f, 0.30f), new Vector2(0.50f, 0.68f),
        new Vector2(0.67f, 0.36f), new Vector2(0.84f, 0.62f)
    };
    public static readonly int[] MapCounts = { 1, 8, 2, 6, 4 }; // I=1 II=8 III=2 IV=6 V=4

    // Bảng màu (tông trầm như màn 1)
    static readonly Color Wall = new Color(0.40f, 0.12f, 0.11f);
    static readonly Color WallDark = new Color(0.27f, 0.07f, 0.07f);
    static readonly Color Wood = new Color(0.30f, 0.19f, 0.12f);
    static readonly Color WoodDark = new Color(0.21f, 0.13f, 0.08f);
    static readonly Color WoodLight = new Color(0.40f, 0.27f, 0.17f);
    static readonly Color Gold = new Color(0.66f, 0.52f, 0.27f);
    static readonly Color GoldDark = new Color(0.50f, 0.38f, 0.18f);
    static readonly Color Uniform = new Color(0.20f, 0.20f, 0.21f);
    static readonly Color UniformDark = new Color(0.14f, 0.14f, 0.15f);
    static readonly Color Skin = new Color(0.86f, 0.74f, 0.64f);
    static readonly Color Red = new Color(0.66f, 0.12f, 0.10f);
    static readonly Color Blood = new Color(0.42f, 0.04f, 0.04f);
    static readonly Color Paper = new Color(0.90f, 0.86f, 0.76f);
    static readonly Color Steel = new Color(0.44f, 0.45f, 0.47f);
    static readonly Color SteelDark = new Color(0.30f, 0.31f, 0.33f);
    static readonly Color Black = new Color(0.07f, 0.05f, 0.05f);
    static readonly Color Grey = new Color(0.42f, 0.40f, 0.38f);

    static Dictionary<string, Sprite> S;

    static Sprite Save(M3Pix p, string name, float ppu, Vector2? pivot = null, int border = 0)
    {
        Sprite s = p.Save(Dir, name, ppu, pivot, border);
        S[name] = s;
        return s;
    }

    // Quy trình chuẩn cho vật thể có nét viền
    static void Finish(M3Pix p, string name, float ppu, int ink = 2, float wash = 0.12f, int seed = 1, Vector2? pivot = null, int border = 0)
    {
        p.Ink(ink).Watercolor(wash, seed);
        Save(p, name, ppu, pivot, border);
    }

    public static Dictionary<string, Sprite> Generate()
    {
        S = new Dictionary<string, Sprite>();
        Background();
        Portrait();
        Eye();
        Safe();
        Small();
        DoorWindowClock();
        Characters();
        MapAndFlag();
        DeskStuff();
        NewItems();
        Icons();
        UiPieces();
        return S;
    }

    // ------------------------------------------------------------------ Nền phòng
    static void Background()
    {
        // 2752 x 1536 (bằng ảnh nền màn 1) -> 28.67 x 16 đơn vị, phủ kín khung camera, nét cả ở màn 1440p
        var p = new M3Pix(2752, 1536);
        p.VGradient(Wall, WallDark);
        for (int i = 0; i < 34; i++)                                     // giấy dán tường sọc mờ
            if (i % 2 == 0) p.Rect(i / 34f, 0.36f, i / 34f + 0.012f, 0.95f, new Color(0, 0, 0, 0.07f));
        for (int i = 0; i < 17; i++)                                     // họa tiết chấm mờ
            for (int j = 0; j < 6; j++)
                p.Ellipse(i / 17f + 0.03f, 0.42f + j * 0.09f, 0.004f, 0.008f, new Color(0, 0, 0, 0.08f));
        p.Rect(0f, 0.95f, 1f, 1f, WoodDark);                             // phào trần
        p.Rect(0f, 0.935f, 1f, 0.95f, Wood);
        p.Rect(0f, 0.22f, 1f, 0.36f, Wood);                              // ốp gỗ chân tường
        for (int i = 0; i < 8; i++) p.Rect(0.06f + i * 0.115f, 0.245f, 0.15f + i * 0.115f, 0.335f, WoodDark);
        p.Rect(0f, 0.355f, 1f, 0.365f, WoodLight);
        p.Rect(0f, 0f, 1f, 0.22f, Wood);                                 // sàn gỗ phối cảnh
        for (int k = -12; k <= 12; k++)
            p.Line(0.5f + k * 0.045f, 0.22f, 0.5f + k * 0.14f, 0f, 5f, WoodDark);
        p.Rect(0f, 0.13f, 1f, 0.135f, WoodDark).Rect(0f, 0.06f, 1f, 0.066f, WoodDark);
        p.Rect(0f, 0.212f, 1f, 0.222f, WoodDark);                        // chân tường
        p.Polygon(new Color(0, 0, 0, 0.28f), new Vector2(0, 0.22f), new Vector2(0.045f, 0.26f), new Vector2(0.045f, 0.95f), new Vector2(0, 1));       // tường hông trái
        p.Polygon(new Color(0, 0, 0, 0.28f), new Vector2(1, 0.22f), new Vector2(0.955f, 0.26f), new Vector2(0.955f, 0.95f), new Vector2(1, 1));       // tường hông phải
        p.Line(0.045f, 0.26f, 0.045f, 0.95f, 6, Black).Line(0.955f, 0.26f, 0.955f, 0.95f, 6, Black);
        p.Ink(2).Watercolor(0.14f, 3).Vignette(0.5f);
        Save(p, "bg_wall", 2752f / 28.67f);
    }

    // ------------------------------------------------------------------ Chân dung
    static M3Pix PortraitBase()
    {
        var p = new M3Pix(440, 600);
        p.Fill(Gold).Rect(0.03f, 0.025f, 0.97f, 0.975f, GoldDark).Rect(0.075f, 0.055f, 0.925f, 0.945f, new Color(0.30f, 0.13f, 0.12f));
        p.Ellipse(0.5f, 0.66f, 0.36f, 0.3f, new Color(0.36f, 0.16f, 0.14f), 0.9f);                     // quầng sáng sau đầu
        p.Ellipse(0.5f, 0.2f, 0.42f, 0.3f, Uniform).Rect(0.08f, 0.055f, 0.92f, 0.2f, Uniform);        // vai
        p.Polygon(UniformDark, new Vector2(0.5f, 0.42f), new Vector2(0.4f, 0.3f), new Vector2(0.5f, 0.06f), new Vector2(0.6f, 0.3f)); // ve áo
        p.Rect(0.14f, 0.19f, 0.3f, 0.25f, Red).Ellipse(0.22f, 0.22f, 0.03f, 0.022f, Black);           // băng tay con mắt
        for (int i = 0; i < 3; i++) p.Ellipse(0.66f + i * 0.055f, 0.3f, 0.02f, 0.015f, Gold);          // huân chương
        p.Rect(0.45f, 0.42f, 0.55f, 0.47f, Skin);                                                     // cổ
        p.Ellipse(0.5f, 0.57f, 0.15f, 0.13f, Skin);                                                   // mặt
        p.Polygon(Uniform, new Vector2(0.3f, 0.68f), new Vector2(0.7f, 0.68f), new Vector2(0.66f, 0.79f), new Vector2(0.34f, 0.79f)); // mũ
        p.Ellipse(0.5f, 0.8f, 0.19f, 0.05f, Uniform).Rect(0.27f, 0.665f, 0.73f, 0.69f, Black);
        p.Ellipse(0.5f, 0.74f, 0.03f, 0.02f, Gold);                                                   // phù hiệu mũ
        p.Line(0.41f, 0.615f, 0.47f, 0.6f, 3, Black).Line(0.59f, 0.615f, 0.53f, 0.6f, 3, Black);       // lông mày cau
        p.Ellipse(0.44f, 0.585f, 0.022f, 0.011f, new Color(0.7f, 0.08f, 0.08f));                       // mắt đỏ lạnh lẽo
        p.Ellipse(0.56f, 0.585f, 0.022f, 0.011f, new Color(0.7f, 0.08f, 0.08f));
        p.Line(0.45f, 0.51f, 0.55f, 0.51f, 3, new Color(0.45f, 0.2f, 0.18f));                         // miệng mím
        p.Line(0.5f, 0.57f, 0.49f, 0.535f, 2, new Color(0.7f, 0.55f, 0.48f));                          // sống mũi
        return p;
    }

    static void Portrait()
    {
        // Chân dung độ phân giải cao (660x900, ppu 150 -> vẫn 4.4 x 6 đơn vị), khuôn mặt chi tiết
        Save(M3CharacterArt.WallPortrait(false), "portrait_intact", 150);
        Save(M3CharacterArt.WallPortrait(true), "portrait_torn", 150);

        var slash = new M3Pix(520, 24).Ellipse(0.5f, 0.5f, 0.5f, 0.5f, new Color(1f, 0.92f, 0.85f), 0.6f);
        Save(slash, "slash", 100);
    }

    // ------------------------------------------------------------------ Con mắt
    static void Eye()
    {
        var w = new M3Pix(160, 100);
        w.Ellipse(0.5f, 0.5f, 0.5f, 0.5f, new Color(0.62f, 0.22f, 0.22f));                 // mí thịt
        w.Ellipse(0.5f, 0.5f, 0.42f, 0.38f, new Color(0.95f, 0.88f, 0.82f));               // lòng trắng
        for (int i = 0; i < 6; i++)
            w.Jagged(i < 3 ? 0.1f : 0.9f, 0.3f + (i % 3) * 0.2f, 0.5f, 0.5f, 4, 0.04f, 2.5f, new Color(0.8f, 0.15f, 0.15f, 0.8f), 20 + i);
        w.Ink(2, 0.6f).Watercolor(0.1f, 8);
        Save(w, "eye_white", 100);

        var pu = new M3Pix(56, 56).Ellipse(0.5f, 0.5f, 0.5f, 0.5f, new Color(0.5f, 0.1f, 0.08f)).Ellipse(0.5f, 0.5f, 0.24f, 0.24f, Black);
        pu.Ink(2);
        pu.Ellipse(0.62f, 0.64f, 0.09f, 0.09f, new Color(1, 1, 1, 0.85f));
        Save(pu, "eye_pupil", 100);
    }

    // ------------------------------------------------------------------ Két sắt
    static void Safe()
    {
        var c = new M3Pix(180, 160).Fill(Steel).Rect(0.06f, 0.06f, 0.94f, 0.94f, new Color(0.40f, 0.41f, 0.43f));
        c.Ellipse(0.42f, 0.5f, 0.2f, 0.22f, SteelDark).Ellipse(0.42f, 0.5f, 0.12f, 0.135f, new Color(0.7f, 0.66f, 0.55f));
        for (int i = 0; i < 10; i++)
        {
            float a = i * Mathf.PI / 5f;
            c.Line(0.42f + Mathf.Cos(a) * 0.15f, 0.5f + Mathf.Sin(a) * 0.17f, 0.42f + Mathf.Cos(a) * 0.19f, 0.5f + Mathf.Sin(a) * 0.21f, 2, Black);
        }
        c.Rect(0.75f, 0.35f, 0.83f, 0.65f, new Color(0.7f, 0.66f, 0.55f));
        c.Ellipse(0.2f, 0.85f, 0.03f, 0.03f, SteelDark).Ellipse(0.8f, 0.85f, 0.03f, 0.03f, SteelDark).Ellipse(0.2f, 0.15f, 0.03f, 0.03f, SteelDark).Ellipse(0.8f, 0.15f, 0.03f, 0.03f, SteelDark);
        Finish(c, "safe_closed", 100, 2, 0.1f, 9);

        var o = new M3Pix(180, 160).Fill(Steel).Rect(0.06f, 0.06f, 0.94f, 0.94f, new Color(0.10f, 0.09f, 0.09f));
        o.Rect(0.2f, 0.12f, 0.75f, 0.22f, Paper).Rect(0.24f, 0.22f, 0.7f, 0.3f, new Color(0.8f, 0.72f, 0.5f)).Rect(0.22f, 0.3f, 0.68f, 0.36f, Paper);
        o.Polygon(new Color(0.40f, 0.41f, 0.43f), new Vector2(0, 0.02f), new Vector2(0.14f, 0.1f), new Vector2(0.14f, 0.9f), new Vector2(0, 0.98f));
        Finish(o, "safe_open", 100, 2, 0.1f, 10);
    }

    // ------------------------------------------------------------------ Vật nhỏ trong phòng giữa
    static void Small()
    {
        var plate = new M3Pix(240, 52).Fill(Gold).Rect(0.03f, 0.12f, 0.97f, 0.88f, new Color(0.72f, 0.58f, 0.32f));
        plate.Rect(0.12f, 0.44f, 0.88f, 0.56f, GoldDark);
        Finish(plate, "nameplate", 100, 2, 0.08f, 11);

        var crack = new M3Pix(320, 80).Jagged(0.02f, 0.5f, 0.98f, 0.5f, 12, 0.12f, 9, Black, 31)
            .Jagged(0.4f, 0.5f, 0.6f, 0.95f, 4, 0.08f, 5, Black, 32).Jagged(0.65f, 0.5f, 0.8f, 0.05f, 4, 0.08f, 5, Black, 33);
        Save(crack, "crack", 100);
        Save(new M3Pix(280, 100).Ellipse(0.5f, 0.5f, 0.48f, 0.46f, new Color(0.06f, 0.01f, 0.02f, 0.95f), 0.25f, 0.4f), "ooze", 100);
        var drop = new M3Pix(28, 40).Ellipse(0.5f, 0.35f, 0.42f, 0.32f, Blood).Polygon(Blood, new Vector2(0.15f, 0.4f), new Vector2(0.5f, 0.98f), new Vector2(0.85f, 0.4f));
        Save(drop.Ink(1), "blood_drop", 100);
    }

    // ------------------------------------------------------------------ Cửa, cửa sổ, đồng hồ
    static void DoorWindowClock()
    {
        var d = new M3Pix(320, 700).Fill(WoodDark).Rect(0.05f, 0f, 0.95f, 0.97f, Wood);
        d.Rect(0.15f, 0.55f, 0.85f, 0.9f, WoodDark).Rect(0.19f, 0.58f, 0.81f, 0.87f, Wood);
        d.Rect(0.15f, 0.08f, 0.85f, 0.48f, WoodDark).Rect(0.19f, 0.11f, 0.81f, 0.45f, Wood);
        d.Ellipse(0.8f, 0.51f, 0.05f, 0.022f, Gold).Rect(0.76f, 0.47f, 0.84f, 0.49f, GoldDark);
        Finish(d, "door_closed", 100, 2, 0.14f, 7);

        var o = new M3Pix(320, 700).Fill(WoodDark).Rect(0.05f, 0f, 0.95f, 0.97f, new Color(0.03f, 0.03f, 0.03f));
        o.Polygon(Wood, new Vector2(0.05f, 0f), new Vector2(0.2f, 0.04f), new Vector2(0.2f, 0.93f), new Vector2(0.05f, 0.97f));
        o.Polygon(new Color(0.35f, 0.3f, 0.25f, 0.6f), new Vector2(0.2f, 0f), new Vector2(0.95f, 0f), new Vector2(0.95f, 0.08f), new Vector2(0.3f, 0.05f)); // vệt sáng hành lang
        Finish(o, "door_open", 100, 2, 0.1f, 12);

        var win = new M3Pix(300, 360).Fill(WoodDark).Rect(0.06f, 0.05f, 0.94f, 0.95f, new Color(0.16f, 0.16f, 0.2f));
        win.Rect(0.06f, 0.05f, 0.94f, 0.32f, new Color(0.5f, 0.18f, 0.1f));                    // ánh lửa xa
        for (int i = 0; i < 6; i++) win.Rect(0.08f + i * 0.145f, 0.05f, 0.18f + i * 0.145f, 0.12f + (i % 3) * 0.07f, Black); // nhà đổ nát
        win.Ellipse(0.6f, 0.68f, 0.32f, 0.12f, new Color(0.55f, 0.55f, 0.55f, 0.35f), 0.8f);
        win.Line(0.85f, 0.95f, 0.35f, 0.55f, 6, new Color(0.9f, 0.9f, 0.7f, 0.25f));          // đèn pha
        win.Rect(0.47f, 0.05f, 0.53f, 0.95f, WoodDark).Rect(0.06f, 0.48f, 0.94f, 0.53f, WoodDark);
        win.Rect(0f, 0f, 1f, 0.05f, WoodLight);
        Finish(win, "window", 100, 2, 0.12f, 13);

        var sh = new M3Pix(120, 220);
        sh.Ellipse(0.5f, 0.78f, 0.2f, 0.12f, Black).Polygon(Black, new Vector2(0.22f, 0.84f), new Vector2(0.78f, 0.84f), new Vector2(0.7f, 0.96f), new Vector2(0.3f, 0.96f));
        sh.Ellipse(0.5f, 0.32f, 0.46f, 0.38f, Black).Line(0.25f, 0.55f, 0f, 0.6f, 9, Black);
        Save(sh, "window_shadow", 100);
        Save(new M3Pix(80, 80).Ellipse(0.5f, 0.5f, 0.5f, 0.5f, new Color(1f, 0.85f, 0.3f), 0.7f).Ellipse(0.5f, 0.5f, 0.2f, 0.2f, Color.white), "muzzle_flash", 100);

        var face = new M3Pix(160, 160).Ellipse(0.5f, 0.5f, 0.5f, 0.5f, WoodDark).Ellipse(0.5f, 0.5f, 0.42f, 0.42f, Paper);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6f;
            face.Line(0.5f + Mathf.Cos(a) * 0.33f, 0.5f + Mathf.Sin(a) * 0.33f, 0.5f + Mathf.Cos(a) * 0.4f, 0.5f + Mathf.Sin(a) * 0.4f, i % 3 == 0 ? 5 : 3, Black);
        }
        face.Ellipse(0.5f, 0.5f, 0.04f, 0.04f, Black);
        Finish(face, "clock_face", 100, 2, 0.08f, 14);
        Save(new M3Pix(12, 44).Fill(Black), "hand_hour", 100, new Vector2(0.5f, 0.05f));
        Save(new M3Pix(8, 64).Fill(Black), "hand_minute", 100, new Vector2(0.5f, 0.05f));
        Save(new M3Pix(4, 68).Fill(new Color(0.7f, 0.05f, 0.05f)), "hand_second", 100, new Vector2(0.5f, 0.1f));
    }

    // ------------------------------------------------------------------ Nhân vật & cảnh kết
    static void Characters()
    {
        Save(M3CharacterArt.Brother(), "brother", 200); // 480x1200 -> 2.4 x 6 đơn vị

        var gun = new M3Pix(80, 32).Rect(0f, 0.5f, 1f, 0.88f, Black).Rect(0.05f, 0f, 0.3f, 0.6f, new Color(0.3f, 0.2f, 0.12f));
        Finish(gun, "gun", 100, 1, 0.05f, 16);
        Save(new M3Pix(280, 80).Ellipse(0.5f, 0.5f, 0.48f, 0.45f, new Color(0.10f, 0.0f, 0.01f), 0.2f, 0.4f), "blood_pool", 100);

        var sc = new M3Pix(140, 88);
        sc.Ellipse(0.47f, 0.5f, 0.22f, 0.24f, Black);
        for (int i = 0; i < 5; i++) sc.Ellipse(0.27f - i * 0.04f, 0.5f + i * 0.09f, 0.07f, 0.09f, Black);
        sc.Line(0.1f, 0.92f, 0.2f, 0.98f, 5, new Color(0.5f, 0.02f, 0.02f));
        sc.Ellipse(0.86f, 0.75f, 0.1f, 0.11f, Black).Ellipse(0.86f, 0.25f, 0.1f, 0.11f, Black);
        sc.Line(0.6f, 0.55f, 0.82f, 0.72f, 5, Black).Line(0.6f, 0.45f, 0.82f, 0.28f, 5, Black);
        for (int i = 0; i < 3; i++) { sc.Line(0.4f + i * 0.08f, 0.5f, 0.35f + i * 0.08f, 0.08f, 3, Black); sc.Line(0.4f + i * 0.08f, 0.5f, 0.35f + i * 0.08f, 0.92f, 3, Black); }
        Save(sc, "scorpion", 100);
    }

    // ------------------------------------------------------------------ Bản đồ & cờ
    static readonly Color Sea = new Color(0.56f, 0.60f, 0.56f);
    static readonly Color Land = new Color(0.82f, 0.74f, 0.55f);

    static M3Pix MapPix(int w, int h, bool big)
    {
        var m = new M3Pix(w, h).Fill(WoodDark).Rect(0.015f, 0.025f, 0.985f, 0.975f, Sea);
        m.Continents(Land, 0.52f, big ? 0.0062f : 0.0082f, 17);
        // các vùng bị bôi đen (tránh che 5 dấu con mắt)
        m.Ellipse(0.76f, 0.86f, 0.2f, 0.12f, new Color(0.06f, 0.05f, 0.05f, 0.92f), 0.1f, 0.5f);
        m.Ellipse(0.25f, 0.13f, 0.12f, 0.08f, new Color(0.06f, 0.05f, 0.05f, 0.9f), 0.15f, 0.6f);
        m.Ellipse(0.6f, 0.08f, 0.1f, 0.05f, new Color(0.06f, 0.05f, 0.05f, 0.9f), 0.15f, 0.6f);
        m.Ellipse(0.95f, 0.85f, 0.05f, 0.1f, new Color(0.06f, 0.05f, 0.05f, 0.9f), 0.15f, 0.6f);
        for (int i = 1; i < 6; i++) m.Line(i / 6f, 0.03f, i / 6f, 0.97f, 1.5f, new Color(0.2f, 0.18f, 0.15f, 0.25f)); // kinh tuyến
        for (int i = 1; i < 4; i++) m.Line(0.02f, i / 4f, 0.98f, i / 4f, 1.5f, new Color(0.2f, 0.18f, 0.15f, 0.25f));
        m.Ink(big ? 2 : 1, 0.3f);

        float aspect = w / (float)h;
        for (int i = 0; i < MapMarks.Length; i++)
        {
            Vector2 c = MapMarks[i];
            float r = big ? 0.032f : 0.03f;
            m.Ellipse(c.x, c.y, r, r * aspect, Black).Ellipse(c.x, c.y, r * 0.82f, r * 0.82f * aspect, Red);
            m.Ellipse(c.x, c.y, r * 0.6f, r * 0.26f * aspect, Paper).Ellipse(c.x, c.y, r * 0.2f, r * 0.2f * aspect, Black);
            if (big)
            {
                // mảnh giấy ghi chú dưới mỗi con mắt – ký hiệu li ti, mắt thường không đọc được
                m.Rect(c.x - 0.06f, c.y - 0.2f, c.x + 0.075f, c.y - 0.05f, new Color(0.9f, 0.86f, 0.75f));
                for (int k = 0; k < 5; k++)
                    m.Rect(c.x - 0.05f, c.y - 0.19f + k * 0.027f, c.x + 0.065f, c.y - 0.183f + k * 0.027f, new Color(0.5f, 0.47f, 0.42f, 0.5f));
            }
        }
        return m.Watercolor(0.08f, 18);
    }

    // Lớp phủ khi soi bằng kính lúp: vạch chữ thập rõ ràng trên từng mảnh giấy
    static M3Pix MapTallies(int w, int h)
    {
        var m = new M3Pix(w, h);
        for (int i = 0; i < MapMarks.Length; i++)
        {
            Vector2 c = MapMarks[i];
            m.Rect(c.x - 0.06f, c.y - 0.2f, c.x + 0.075f, c.y - 0.05f, new Color(0.93f, 0.9f, 0.8f));
            int n = MapCounts[i];
            for (int k = 0; k < n; k++)
            {
                float x = c.x - 0.04f + (k % 4) * 0.03f;
                float y = c.y - 0.085f - (k / 4) * 0.06f;
                m.Line(x - 0.01f, y - 0.018f, x + 0.01f, y + 0.018f, 6, Black);
                m.Line(x - 0.01f, y + 0.018f, x + 0.01f, y - 0.018f, 6, Black);
            }
        }
        return m.Ink(2);
    }

    static void MapAndFlag()
    {
        Save(MapPix(900, 500, false), "map", 100);
        Save(MapPix(1200, 700, true), "ui_map_zoom", 100);
        Save(MapTallies(1200, 700), "ui_map_tallies", 100);

        var f = new M3Pix(200, 400).Rect(0.05f, 0f, 0.11f, 1f, WoodDark).Ellipse(0.08f, 0.985f, 0.05f, 0.02f, Gold);
        f.Polygon(Red, new Vector2(0.12f, 0.97f), new Vector2(0.95f, 0.97f), new Vector2(0.95f, 0.3f), new Vector2(0.535f, 0.38f), new Vector2(0.12f, 0.3f));
        f.Ellipse(0.535f, 0.66f, 0.3f, 0.15f, Black).Ellipse(0.535f, 0.66f, 0.24f, 0.12f, Red);                 // vòng mắt xích
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4f;
            f.Ellipse(0.535f + Mathf.Cos(a) * 0.27f, 0.66f + Mathf.Sin(a) * 0.135f, 0.04f, 0.02f, Black);
        }
        f.Ellipse(0.535f, 0.66f, 0.17f, 0.045f, Paper).Ellipse(0.535f, 0.66f, 0.045f, 0.022f, Black);           // con mắt
        Finish(f, "flag", 100, 2, 0.14f, 19);

        // Giá treo + áo khoác sĩ quan (túi ngực khâu kín, giấu chìa khóa ngăn kéo)
        var coat = new M3Pix(240, 600);
        coat.Rect(0.47f, 0f, 0.53f, 0.97f, WoodDark).Ellipse(0.5f, 0.02f, 0.3f, 0.02f, WoodDark);
        coat.Line(0.5f, 0.92f, 0.3f, 0.97f, 8, WoodDark).Line(0.5f, 0.92f, 0.7f, 0.97f, 8, WoodDark);
        coat.Polygon(Uniform, new Vector2(0.38f, 0.9f), new Vector2(0.62f, 0.9f), new Vector2(0.86f, 0.78f), new Vector2(0.9f, 0.3f), new Vector2(0.1f, 0.3f), new Vector2(0.14f, 0.78f));
        coat.Polygon(UniformDark, new Vector2(0.5f, 0.88f), new Vector2(0.42f, 0.75f), new Vector2(0.5f, 0.32f), new Vector2(0.58f, 0.75f));
        coat.Rect(0.1f, 0.6f, 0.2f, 0.66f, Red);
        coat.Rect(0.6f, 0.62f, 0.78f, 0.7f, UniformDark);
        for (int i = 0; i < 6; i++) coat.Line(0.61f + i * 0.03f, 0.7f, 0.625f + i * 0.03f, 0.715f, 2, new Color(0.75f, 0.65f, 0.3f)); // đường chỉ khâu
        for (int i = 0; i < 4; i++) coat.Ellipse(0.5f, 0.68f - i * 0.09f, 0.02f, 0.008f, Gold);
        coat.Rect(0.62f, 0.8f, 0.8f, 0.83f, Gold);
        Finish(coat, "coat", 100, 2, 0.12f, 40);
    }

    // ------------------------------------------------------------------ Vật phẩm mở rộng (phase mới)
    static void NewItems()
    {
        var dc = new M3Pix(280, 100).Fill(WoodDark).Rect(0.04f, 0.1f, 0.96f, 0.9f, Wood);
        dc.Rect(0.4f, 0.42f, 0.6f, 0.58f, Gold).Ellipse(0.5f, 0.5f, 0.025f, 0.08f, Black);
        Finish(dc, "drawer_closed", 100, 2, 0.12f, 41);
        var dop = new M3Pix(280, 100).Fill(WoodDark).Rect(0.04f, 0.1f, 0.96f, 0.9f, new Color(0.08f, 0.05f, 0.04f));
        dop.Rect(0f, 0f, 1f, 0.3f, Wood).Rect(0.42f, 0.08f, 0.58f, 0.2f, Gold);
        Finish(dop, "drawer_open", 100, 2, 0.12f, 42);

        var note = new M3Pix(140, 100).Fill(Paper).Rect(0.08f, 0.78f, 0.92f, 0.88f, new Color(0.3f, 0.25f, 0.2f));
        for (int i = 0; i < 5; i++) note.Rect(0.08f, 0.15f + i * 0.11f, 0.85f - (i % 2) * 0.15f, 0.19f + i * 0.11f, new Color(0.5f, 0.47f, 0.42f));
        note.Ellipse(0.78f, 0.2f, 0.1f, 0.13f, new Color(0.65f, 0.1f, 0.1f, 0.8f));
        Finish(note, "note", 100, 2, 0.08f, 43);

        var key = new M3Pix(128, 128).Ellipse(0.3f, 0.65f, 0.2f, 0.2f, Gold).Ellipse(0.3f, 0.65f, 0.08f, 0.08f, WoodDark);
        key.Line(0.42f, 0.55f, 0.88f, 0.2f, 14, Gold).Line(0.75f, 0.3f, 0.82f, 0.4f, 10, Gold).Line(0.85f, 0.22f, 0.92f, 0.32f, 10, Gold);
        Finish(key, "icon_key", 100, 2, 0.08f, 44);

        var mag = new M3Pix(128, 128).Line(0.55f, 0.45f, 0.9f, 0.1f, 16, WoodDark);
        mag.Ellipse(0.4f, 0.6f, 0.3f, 0.3f, new Color(0.55f, 0.55f, 0.58f)).Ellipse(0.4f, 0.6f, 0.24f, 0.24f, new Color(0.75f, 0.85f, 0.9f));
        mag.Ellipse(0.33f, 0.68f, 0.07f, 0.05f, Color.white);
        Finish(mag, "icon_magnifier", 100, 2, 0.06f, 45);

        var drawer = new M3Pix(1000, 560).Fill(WoodDark).Rect(0.03f, 0.05f, 0.97f, 0.95f, new Color(0.26f, 0.17f, 0.11f));
        drawer.Polygon(new Color(0.20f, 0.13f, 0.08f), new Vector2(0.03f, 0.95f), new Vector2(0.97f, 0.95f), new Vector2(0.9f, 0.82f), new Vector2(0.1f, 0.82f));
        Finish(drawer, "ui_drawer", 100, 2, 0.14f, 46);

        var book = new M3Pix(96, 96).Rect(0.15f, 0.1f, 0.85f, 0.9f, new Color(0.45f, 0.18f, 0.12f)).Rect(0.2f, 0.1f, 0.26f, 0.9f, new Color(0.3f, 0.1f, 0.08f));
        book.Rect(0.38f, 0.55f, 0.75f, 0.7f, Paper);
        Finish(book, "icon_journal", 100, 2, 0.08f, 47);
    }

    // ------------------------------------------------------------------ Bàn làm việc
    static void DeskStuff()
    {
        var d = new M3Pix(1000, 300).Fill(Wood);
        d.Rect(0f, 0.84f, 1f, 1f, WoodLight).Rect(0.04f, 0.08f, 0.32f, 0.76f, WoodDark).Rect(0.68f, 0.08f, 0.96f, 0.76f, WoodDark);
        for (int i = 0; i < 3; i++)
        {
            d.Rect(0.06f, 0.12f + i * 0.21f, 0.3f, 0.29f + i * 0.21f, Wood).Rect(0.15f, 0.19f + i * 0.21f, 0.21f, 0.22f + i * 0.21f, Gold);
            d.Rect(0.7f, 0.12f + i * 0.21f, 0.94f, 0.29f + i * 0.21f, Wood).Rect(0.79f, 0.19f + i * 0.21f, 0.85f, 0.22f + i * 0.21f, Gold);
        }
        Finish(d, "desk", 100, 2, 0.14f, 20);

        var pipe = new M3Pix(128, 64).Polygon(new Color(0.38f, 0.2f, 0.1f), new Vector2(0.62f, 0.15f), new Vector2(0.9f, 0.15f), new Vector2(0.92f, 0.8f), new Vector2(0.6f, 0.8f));
        pipe.Polygon(new Color(0.18f, 0.1f, 0.05f), new Vector2(0.03f, 0.32f), new Vector2(0.62f, 0.3f), new Vector2(0.62f, 0.45f), new Vector2(0.03f, 0.42f));
        pipe.Ellipse(0.76f, 0.8f, 0.15f, 0.08f, new Color(0.15f, 0.07f, 0.03f));
        pipe.Ink(2);
        pipe.Ellipse(0.76f, 0.82f, 0.1f, 0.06f, new Color(1f, 0.5f, 0.12f));
        Save(pipe, "pipe", 100);
        Save(new M3Pix(60, 120).Ellipse(0.5f, 0.3f, 0.4f, 0.2f, new Color(0.75f, 0.75f, 0.75f, 0.6f), 1f)
            .Ellipse(0.4f, 0.6f, 0.35f, 0.18f, new Color(0.75f, 0.75f, 0.75f, 0.5f), 1f).Ellipse(0.55f, 0.85f, 0.3f, 0.13f, new Color(0.75f, 0.75f, 0.75f, 0.4f), 1f), "smoke", 100);
        Save(new M3Pix(120, 180).Ellipse(0.5f, 0.35f, 0.45f, 0.33f, new Color(0.1f, 0.75f, 0.2f, 0.9f), 0.5f, 0.3f)
            .Ellipse(0.5f, 0.55f, 0.25f, 0.42f, new Color(0.3f, 1f, 0.4f, 0.9f), 0.6f, 0.3f).Ellipse(0.5f, 0.35f, 0.15f, 0.18f, new Color(0.85f, 1f, 0.85f), 0.5f), "green_fire", 100);

        var n = new M3Pix(200, 120).Fill(Paper).Rect(0.06f, 0.74f, 0.94f, 0.88f, new Color(0.25f, 0.22f, 0.2f));
        for (int i = 0; i < 6; i++) n.Rect(0.06f, 0.1f + i * 0.095f, i % 2 == 0 ? 0.5f : 0.46f, 0.13f + i * 0.095f, new Color(0.5f, 0.47f, 0.42f));
        n.Rect(0.55f, 0.1f, 0.94f, 0.65f, new Color(0.55f, 0.52f, 0.46f));
        Finish(n, "newspaper", 100, 2, 0.08f, 21);

        var sh = new M3Pix(400, 640).Fill(WoodDark);
        Color[] spines = { new Color(0.5f, 0.12f, 0.1f), new Color(0.2f, 0.2f, 0.2f), new Color(0.42f, 0.35f, 0.22f), new Color(0.3f, 0.1f, 0.08f) };
        for (int row = 0; row < 4; row++)
        {
            float y0 = 0.05f + row * 0.235f;
            sh.Rect(0.05f, y0, 0.95f, y0 + 0.2f, new Color(0.1f, 0.06f, 0.04f));
            for (int k = 0; k < 9; k++) sh.Rect(0.07f + k * 0.098f, y0, 0.15f + k * 0.098f, y0 + 0.14f + (k % 3) * 0.018f, spines[(k + row) % spines.Length]);
        }
        Finish(sh, "bookshelf", 100, 2, 0.12f, 22);

        var wallet = new M3Pix(120, 80).Fill(new Color(0.42f, 0.25f, 0.13f)).Rect(0.05f, 0.08f, 0.95f, 0.92f, new Color(0.48f, 0.3f, 0.16f));
        wallet.Rect(0.05f, 0.47f, 0.95f, 0.53f, new Color(0.3f, 0.17f, 0.08f));
        Finish(wallet, "wallet", 100, 2, 0.1f, 23);
    }

    // ------------------------------------------------------------------ Icon túi đồ
    static void Icons()
    {
        var s = new M3Pix(128, 128).Line(0.15f, 0.15f, 0.55f, 0.55f, 16, new Color(0.35f, 0.35f, 0.38f));
        s.Polygon(new Color(0.85f, 0.88f, 0.92f), new Vector2(0.52f, 0.6f), new Vector2(0.6f, 0.52f), new Vector2(0.92f, 0.9f));
        Finish(s, "icon_scalpel", 100, 2, 0.06f, 24);

        Save(M3CharacterArt.FamilyPhoto(), "icon_photo", 100);

        // Chân dung người nói (hiện bên cạnh hộp thoại)
        Save(M3CharacterArt.SpeakerElias(), "speaker_elias", 100);
        Save(M3CharacterArt.SpeakerBrother(), "speaker_brother", 100);
        Save(M3CharacterArt.SpeakerGuard(), "speaker_guard", 100);
        Save(M3CharacterArt.SpeakerUnknown(), "speaker_unknown", 100);

        var t = new M3Pix(128, 160).Fill(Paper);
        t.Rect(0.15f, 0.8f, 0.85f, 0.88f, new Color(0.2f, 0.15f, 0.12f));
        for (int i = 0; i < 6; i++) t.Rect(0.15f, 0.25f + i * 0.08f, 0.85f - (i % 2) * 0.1f, 0.28f + i * 0.08f, new Color(0.45f, 0.4f, 0.35f));
        t.Ellipse(0.7f, 0.13f, 0.14f, 0.1f, Red).Ellipse(0.7f, 0.13f, 0.06f, 0.035f, Black);
        Finish(t, "icon_treaty", 100, 2, 0.08f, 26);
        S["ui_treaty"] = S["icon_treaty"];
    }

    // ------------------------------------------------------------------ Khung UI (9-slice, giống ô túi đồ màn 1)
    static void UiPieces()
    {
        var panel = new M3Pix(192, 192).Fill(Black).Rect(0.03f, 0.03f, 0.97f, 0.97f, Grey);
        Finish(panel, "ui_panel", 100, 1, 0.16f, 27, null, 28);

        // Nền chữ đặc, màu phẳng tuyệt đối (không vân, không trong suốt) + viền vàng mảnh:
        // chữ đặt lên đây luôn rõ, không phụ thuộc màu/ánh sáng của cảnh phía sau.
        var box = new M3Pix(64, 64).Fill(new Color(0.03f, 0.03f, 0.03f));
        box.Rect(2 / 64f, 2 / 64f, 62 / 64f, 62 / 64f, new Color(0.62f, 0.5f, 0.3f));
        box.Rect(5 / 64f, 5 / 64f, 59 / 64f, 59 / 64f, new Color(0.09f, 0.08f, 0.08f));
        Save(box, "ui_textbox", 100, null, 12);

        var key = new M3Pix(96, 96).Fill(Black).Rect(0.05f, 0.05f, 0.95f, 0.95f, new Color(0.55f, 0.53f, 0.5f));
        Finish(key, "ui_key", 100, 1, 0.12f, 28, null, 18);

        var folder = new M3Pix(190, 250).Fill(new Color(0.78f, 0.64f, 0.42f));
        folder.Polygon(new Color(0.7f, 0.56f, 0.35f), new Vector2(0f, 0.9f), new Vector2(0.4f, 0.9f), new Vector2(0.45f, 1f), new Vector2(0f, 1f));
        folder.Rect(0.1f, 0.62f, 0.9f, 0.82f, Paper);
        Finish(folder, "ui_folder", 100, 2, 0.1f, 29);

        var wo = new M3Pix(600, 390).Fill(new Color(0.40f, 0.24f, 0.12f)).Rect(0.02f, 0.03f, 0.98f, 0.97f, new Color(0.46f, 0.28f, 0.15f));
        wo.Rect(0.495f, 0.03f, 0.505f, 0.97f, new Color(0.3f, 0.17f, 0.08f));
        wo.Rect(0.05f, 0.08f, 0.47f, 0.4f, new Color(0.38f, 0.22f, 0.11f)).Rect(0.53f, 0.08f, 0.95f, 0.4f, new Color(0.38f, 0.22f, 0.11f));
        Finish(wo, "ui_wallet_open", 100, 2, 0.12f, 30);
    }
}
