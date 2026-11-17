using UnityEngine;

// Vẽ nhân vật chi tiết cho màn 3: khuôn mặt (mắt, lông mày, mũi, miệng, tóc), chân dung người nói,
// ảnh gia đình, bức chân dung trên tường và em trai trong cảnh kết. Phong cách nét mực như màn 1.
public static class M3CharacterArt
{
    public enum Hair { Short, Grey, Bun, Child, None }

    static readonly Color Black = new Color(0.07f, 0.05f, 0.05f);
    static readonly Color White = new Color(0.97f, 0.95f, 0.92f);
    static readonly Color Skin = new Color(0.90f, 0.76f, 0.64f);
    static readonly Color SkinPale = new Color(0.86f, 0.78f, 0.72f);
    static readonly Color Uniform = new Color(0.20f, 0.20f, 0.21f);
    static readonly Color UniformDark = new Color(0.13f, 0.13f, 0.14f);
    static readonly Color Gold = new Color(0.78f, 0.62f, 0.3f);
    static readonly Color Red = new Color(0.68f, 0.12f, 0.10f);
    static readonly Color HairBrown = new Color(0.30f, 0.20f, 0.13f);
    static readonly Color HairDark = new Color(0.16f, 0.12f, 0.10f);
    static readonly Color HairGrey = new Color(0.62f, 0.60f, 0.57f);

    public class FaceOpts
    {
        public Color skin = Skin;
        public Color hair = HairBrown;
        public Hair style = Hair.Short;
        public Color iris = new Color(0.30f, 0.22f, 0.15f);
        public bool glasses, mustache, smile, frown, scar, worried;
    }

    // cx, cy: tâm đầu (chuẩn hóa); r: bán kính đầu theo chiều ngang (chuẩn hóa theo chiều rộng)
    public static void Head(M3Pix p, float cx, float cy, float r, FaceOpts o)
    {
        float a = p.w / (float)p.h;
        float ry = r * a * 1.18f;
        // Nét tỉ lệ theo cỡ mặt (mặt nhỏ trong ảnh gia đình -> nét mảnh tương ứng)
        System.Func<float, float> T = t => t * Mathf.Clamp(r * p.w / 100f, 0.35f, 1.5f) / p.lineScale;
        Color shade = new Color(o.skin.r * 0.78f, o.skin.g * 0.68f, o.skin.b * 0.62f);
        Color browC = new Color(o.hair.r * 0.6f, o.hair.g * 0.6f, o.hair.b * 0.6f);
        if (o.style == Hair.Grey) browC = new Color(0.4f, 0.38f, 0.36f);

        if (o.style == Hair.Bun)
        {
            p.Ellipse(cx, cy + ry * 0.1f, r * 1.14f, ry * 1.02f, o.hair);           // tóc phía sau
            p.Ellipse(cx, cy + ry * 1.08f, r * 0.44f, ry * 0.32f, o.hair);          // búi tóc
        }

        p.Ellipse(cx - r * 0.97f, cy - ry * 0.02f, r * 0.17f, ry * 0.2f, o.skin);  // tai
        p.Ellipse(cx + r * 0.97f, cy - ry * 0.02f, r * 0.17f, ry * 0.2f, o.skin);
        p.Ellipse(cx, cy, r, ry, o.skin);                                           // mặt
        p.Ellipse(cx, cy - ry * 0.62f, r * 0.62f, ry * 0.28f, new Color(shade.r, shade.g, shade.b, 0.22f), 0.9f); // bóng cằm

        switch (o.style)
        {
            case Hair.Short:
            case Hair.Grey:
            case Hair.Child:
            {
                float recede = o.style == Hair.Grey ? 0.12f : 0f;
                p.Ellipse(cx, cy + ry * 0.5f, r * 1.06f, ry * 0.66f, o.hair);
                p.Ellipse(cx + r * 0.1f, cy + ry * (0.02f + recede), r * 0.86f, ry * 0.62f, o.skin); // trán
                p.Polygon(o.hair, new Vector2(cx - r * 0.95f, cy + ry * 0.55f), new Vector2(cx - r * 0.2f, cy + ry * (0.72f + recede)),
                    new Vector2(cx - r * 0.6f, cy + ry * (0.38f + recede)), new Vector2(cx - r * 1.0f, cy + ry * 0.1f));       // mái lệch
                if (o.style == Hair.Child)
                    for (int i = 0; i < 4; i++)
                        p.Polygon(o.hair, new Vector2(cx - r * 0.6f + i * r * 0.4f, cy + ry * 0.95f), new Vector2(cx - r * 0.45f + i * r * 0.4f, cy + ry * 1.18f), new Vector2(cx - r * 0.3f + i * r * 0.4f, cy + ry * 0.95f));
                break;
            }
        }

        float ey = cy + ry * 0.08f, ex = r * 0.38f;
        if (o.glasses)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                p.Ellipse(cx + s * ex, ey, r * 0.29f, ry * 0.2f, Black);
                p.Ellipse(cx + s * ex, ey, r * 0.24f, ry * 0.16f, o.skin);
            }
            p.Line(cx - ex + r * 0.27f, ey + ry * 0.03f, cx + ex - r * 0.27f, ey + ry * 0.03f, T(3), Black);
        }

        for (int s = -1; s <= 1; s += 2)
        {
            float x = cx + s * ex;
            p.Ellipse(x, ey, r * 0.19f, ry * 0.1f, White);                                       // lòng trắng
            p.Ellipse(x + r * 0.02f, ey, r * 0.095f, ry * 0.095f, o.iris);                     // mống mắt
            p.Ellipse(x + r * 0.02f, ey, r * 0.045f, ry * 0.045f, Black);                     // con ngươi
            p.Line(x - r * 0.21f, ey + ry * 0.06f, x + r * 0.21f, ey + ry * 0.08f, T(3), Black);  // mí trên
            float inner = o.frown ? -ry * 0.07f : o.worried ? ry * 0.06f : 0f;                  // lông mày
            float browY = ey + ry * 0.24f;
            p.Line(x - s * r * 0.2f, browY + inner, x + s * r * 0.22f, browY + ry * 0.02f, T(4.5f), browC);
            if (o.worried) p.Line(x - r * 0.13f, ey - ry * 0.12f, x + r * 0.13f, ey - ry * 0.12f, T(1.5f), new Color(shade.r, shade.g, shade.b, 0.7f)); // quầng thâm
        }
        if (o.glasses)
            for (int s = -1; s <= 1; s += 2)
                p.Ellipse(cx + s * ex - r * 0.07f, ey + ry * 0.07f, r * 0.06f, ry * 0.035f, new Color(1, 1, 1, 0.6f)); // ánh kính

        p.Line(cx + r * 0.02f, ey - ry * 0.04f, cx - r * 0.07f, cy - ry * 0.26f, T(3), shade);  // mũi
        p.Line(cx - r * 0.08f, cy - ry * 0.26f, cx + r * 0.07f, cy - ry * 0.29f, T(3), shade);

        if (o.mustache)
        {
            p.Polygon(o.style == Hair.Grey ? HairGrey : browC, new Vector2(cx - r * 0.34f, cy - ry * 0.42f), new Vector2(cx, cy - ry * 0.34f),
                new Vector2(cx + r * 0.34f, cy - ry * 0.42f), new Vector2(cx + r * 0.2f, cy - ry * 0.46f), new Vector2(cx, cy - ry * 0.41f), new Vector2(cx - r * 0.2f, cy - ry * 0.46f));
        }

        float my = cy - ry * 0.52f, mw = r * 0.26f;
        Color lip = new Color(0.55f, 0.25f, 0.22f);
        if (o.smile)
        {
            p.Line(cx - mw, my + ry * 0.05f, cx - mw * 0.4f, my - ry * 0.02f, T(3.5f), lip);
            p.Line(cx - mw * 0.4f, my - ry * 0.02f, cx + mw * 0.4f, my - ry * 0.02f, T(3.5f), lip);
            p.Line(cx + mw * 0.4f, my - ry * 0.02f, cx + mw, my + ry * 0.05f, T(3.5f), lip);
        }
        else if (o.frown)
        {
            p.Line(cx - mw, my - ry * 0.04f, cx - mw * 0.3f, my + ry * 0.01f, T(3.5f), lip);
            p.Line(cx - mw * 0.3f, my + ry * 0.01f, cx + mw * 0.3f, my + ry * 0.01f, T(3.5f), lip);
            p.Line(cx + mw * 0.3f, my + ry * 0.01f, cx + mw, my - ry * 0.04f, T(3.5f), lip);
        }
        else p.Line(cx - mw * 0.8f, my, cx + mw * 0.8f, my, T(3.5f), lip);

        if (o.scar) p.Line(cx + r * 0.5f, cy + ry * 0.2f, cx + r * 0.62f, cy - ry * 0.35f, T(3), new Color(0.65f, 0.35f, 0.32f));
    }

    // Vẽ phần đầu/mặt trên một lớp riêng với nét mực MẢNH rồi ghép lên thân (thân dùng nét dày).
    // Nếu dùng chung nét dày, mắt nhỏ sẽ bị tô đen kín như đeo kính râm.
    static void DrawHeadLayer(M3Pix body, System.Action<M3Pix> draw, int ink = 1)
    {
        var layer = new M3Pix(body.w, body.h) { lineScale = body.lineScale };
        draw(layer);
        layer.Ink(ink, 0.3f);
        body.Over(layer);
    }

    // Mũ kê-pi sĩ quan (đặt sau khi vẽ đầu với Hair.None)
    static void OfficerCap(M3Pix p, float cx, float cy, float r, Color band)
    {
        float a = p.w / (float)p.h, ry = r * a * 1.18f;
        p.Polygon(Uniform, new Vector2(cx - r * 1.05f, cy + ry * 0.5f), new Vector2(cx + r * 1.05f, cy + ry * 0.5f),
            new Vector2(cx + r * 1.3f, cy + ry * 1.05f), new Vector2(cx - r * 1.3f, cy + ry * 1.05f));
        p.Ellipse(cx, cy + ry * 1.05f, r * 1.3f, ry * 0.18f, Uniform);
        p.Rect(cx - r * 1.05f, cy + ry * 0.45f, cx + r * 1.05f, cy + ry * 0.62f, band);
        p.Polygon(Black, new Vector2(cx - r * 1.1f, cy + ry * 0.47f), new Vector2(cx + r * 1.1f, cy + ry * 0.47f), new Vector2(cx + r * 0.8f, cy + ry * 0.3f), new Vector2(cx - r * 0.8f, cy + ry * 0.3f));
        p.Ellipse(cx, cy + ry * 0.78f, r * 0.16f, ry * 0.12f, Gold);
        p.Ellipse(cx, cy + ry * 0.78f, r * 0.07f, ry * 0.04f, Black); // biểu tượng con mắt
    }

    // ------------------------------------------------------------------ Chân dung người nói (UI)
    public static M3Pix SpeakerElias()
    {
        var p = new M3Pix(512, 512) { lineScale = 1.4f };
        p.VGradient(new Color(0.30f, 0.25f, 0.21f), new Color(0.42f, 0.36f, 0.30f));
        p.Ellipse(0.5f, 0.08f, 0.46f, 0.3f, new Color(0.42f, 0.30f, 0.20f));                   // áo khoác vải tweed
        p.Polygon(White, new Vector2(0.38f, 0.3f), new Vector2(0.62f, 0.3f), new Vector2(0.5f, 0.06f));
        p.Polygon(new Color(0.45f, 0.1f, 0.1f), new Vector2(0.47f, 0.27f), new Vector2(0.53f, 0.27f), new Vector2(0.55f, 0.08f), new Vector2(0.5f, 0.02f), new Vector2(0.45f, 0.08f)); // cà vạt
        p.Rect(0.44f, 0.28f, 0.56f, 0.38f, Skin);                                               // cổ
        p.Ink(2, 0.3f);
        DrawHeadLayer(p, l => Head(l, 0.5f, 0.58f, 0.2f, new FaceOpts { style = Hair.Grey, hair = HairGrey, glasses = true, mustache = true, worried = true }));
        return p.Watercolor(0.08f, 71);
    }

    public static M3Pix SpeakerBrother()
    {
        var p = new M3Pix(512, 512) { lineScale = 1.4f };
        p.VGradient(new Color(0.25f, 0.05f, 0.05f), new Color(0.42f, 0.1f, 0.09f));
        p.Ellipse(0.5f, 0.08f, 0.48f, 0.3f, Uniform);
        p.Rect(0.05f, 0.24f, 0.24f, 0.29f, Gold).Rect(0.76f, 0.24f, 0.95f, 0.29f, Gold);       // cầu vai
        p.Polygon(UniformDark, new Vector2(0.5f, 0.32f), new Vector2(0.4f, 0.22f), new Vector2(0.5f, 0.0f), new Vector2(0.6f, 0.22f));
        for (int i = 0; i < 3; i++) p.Ellipse(0.68f + i * 0.06f, 0.14f, 0.022f, 0.022f, Gold);   // huân chương
        p.Rect(0.44f, 0.28f, 0.56f, 0.38f, SkinPale);
        p.Ink(2, 0.3f);
        DrawHeadLayer(p, l =>
        {
            Head(l, 0.5f, 0.56f, 0.19f, new FaceOpts { skin = SkinPale, hair = HairDark, style = Hair.None, frown = true, scar = true, iris = new Color(0.62f, 0.08f, 0.08f) });
            OfficerCap(l, 0.5f, 0.56f, 0.19f, Red);
        });
        return p.Watercolor(0.08f, 72);
    }

    public static M3Pix SpeakerGuard()
    {
        var p = new M3Pix(512, 512) { lineScale = 1.4f };
        p.VGradient(new Color(0.16f, 0.16f, 0.17f), new Color(0.28f, 0.28f, 0.29f));
        p.Ellipse(0.5f, 0.08f, 0.46f, 0.3f, new Color(0.3f, 0.32f, 0.26f));
        p.Rect(0.44f, 0.28f, 0.56f, 0.38f, Skin);
        p.Ink(2, 0.3f);
        DrawHeadLayer(p, l =>
        {
            Head(l, 0.5f, 0.55f, 0.18f, new FaceOpts { style = Hair.None, frown = true });
            l.Ellipse(0.5f, 0.68f, 0.24f, 0.2f, new Color(0.32f, 0.34f, 0.28f));                // mũ sắt
            l.Rect(0.24f, 0.62f, 0.76f, 0.66f, new Color(0.26f, 0.28f, 0.22f));
        });
        p.Rect(0.32f, 0.52f, 0.68f, 0.615f, new Color(0f, 0f, 0f, 0.4f));                        // bóng mũ che mắt
        return p.Watercolor(0.08f, 73);
    }

    public static M3Pix SpeakerUnknown()
    {
        var p = new M3Pix(512, 512) { lineScale = 1.4f };
        p.VGradient(new Color(0.12f, 0.12f, 0.13f), new Color(0.22f, 0.22f, 0.23f));
        p.Ellipse(0.5f, 0.1f, 0.44f, 0.3f, Black).Ellipse(0.5f, 0.55f, 0.18f, 0.22f, Black);
        return p;
    }

    // ------------------------------------------------------------------ Ảnh gia đình (sepia)
    public static M3Pix FamilyPhoto()
    {
        var p = new M3Pix(1080, 810) { lineScale = 2f };
        p.VGradient(new Color(0.42f, 0.38f, 0.34f), new Color(0.62f, 0.58f, 0.52f));           // phông studio
        for (int i = 0; i < 9; i++) p.Rect(i / 9f, 0.22f, i / 9f + 0.012f, 1f, new Color(0, 0, 0, 0.08f)); // nếp rèm
        p.Rect(0f, 0f, 1f, 0.22f, new Color(0.40f, 0.33f, 0.26f));                               // sàn
        p.Ellipse(0.82f, 0.24f, 0.07f, 0.03f, new Color(0.3f, 0.25f, 0.2f)).Rect(0.8f, 0.24f, 0.84f, 0.42f, new Color(0.3f, 0.25f, 0.2f)); // bệ hoa
        p.Ellipse(0.82f, 0.46f, 0.06f, 0.06f, new Color(0.35f, 0.45f, 0.3f));

        // Màu quần áo đủ tương phản với phông sau khi chuyển tông nâu (váy mẹ tối hẳn, áo thủy thủ sáng)
        Color suit = new Color(0.2f, 0.19f, 0.2f), dress = new Color(0.27f, 0.2f, 0.25f), jacket = new Color(0.33f, 0.27f, 0.22f), sailor = new Color(0.9f, 0.88f, 0.83f);

        // Thân (nét dày)
        // Cha (trái, cao, com-lê)
        p.Rect(0.18f, 0.12f, 0.215f, 0.42f, suit).Rect(0.235f, 0.12f, 0.27f, 0.42f, suit);
        p.Polygon(suit, new Vector2(0.15f, 0.4f), new Vector2(0.29f, 0.4f), new Vector2(0.3f, 0.66f), new Vector2(0.14f, 0.66f));
        p.Polygon(White, new Vector2(0.2f, 0.66f), new Vector2(0.24f, 0.66f), new Vector2(0.22f, 0.56f));
        p.Rect(0.205f, 0.66f, 0.245f, 0.7f, Skin);
        // Mẹ (phải, váy dài có thắt lưng, cổ áo ren)
        p.Polygon(dress, new Vector2(0.61f, 0.12f), new Vector2(0.79f, 0.12f), new Vector2(0.735f, 0.62f), new Vector2(0.665f, 0.62f));
        p.Rect(0.675f, 0.46f, 0.725f, 0.48f, new Color(0.5f, 0.42f, 0.4f));
        p.Polygon(White, new Vector2(0.67f, 0.62f), new Vector2(0.73f, 0.62f), new Vector2(0.7f, 0.58f));
        p.Rect(0.682f, 0.62f, 0.718f, 0.648f, Skin);
        // Elias lúc nhỏ (giữa trái, áo khoác, tất trắng)
        p.Rect(0.4f, 0.12f, 0.42f, 0.26f, White).Rect(0.44f, 0.12f, 0.46f, 0.26f, White);
        p.Rect(0.395f, 0.26f, 0.465f, 0.32f, jacket);
        p.Polygon(jacket, new Vector2(0.385f, 0.31f), new Vector2(0.475f, 0.31f), new Vector2(0.47f, 0.48f), new Vector2(0.39f, 0.48f));
        p.Rect(0.415f, 0.48f, 0.445f, 0.505f, Skin);
        // Em trai (giữa phải, nhỏ nhất, áo thủy thủ)
        p.Rect(0.535f, 0.12f, 0.552f, 0.22f, White).Rect(0.568f, 0.12f, 0.585f, 0.22f, White);
        p.Polygon(sailor, new Vector2(0.525f, 0.21f), new Vector2(0.595f, 0.21f), new Vector2(0.59f, 0.37f), new Vector2(0.53f, 0.37f));
        p.Polygon(new Color(0.25f, 0.25f, 0.35f), new Vector2(0.53f, 0.37f), new Vector2(0.59f, 0.37f), new Vector2(0.56f, 0.31f));
        p.Rect(0.548f, 0.37f, 0.572f, 0.39f, Skin);
        // Nắm tay nhau
        p.Line(0.29f, 0.5f, 0.39f, 0.42f, 5, suit).Line(0.47f, 0.4f, 0.53f, 0.33f, 4, jacket).Line(0.59f, 0.32f, 0.66f, 0.42f, 4, sailor);
        p.Ink(2, 0.28f);

        // Đầu & mặt (nét mảnh – mắt, mũi, miệng nhìn rõ)
        DrawHeadLayer(p, l =>
        {
            Head(l, 0.225f, 0.76f, 0.045f, new FaceOpts { hair = HairDark, mustache = true, smile = true });
            Head(l, 0.7f, 0.72f, 0.042f, new FaceOpts { style = Hair.Bun, hair = HairBrown, smile = true });
            Head(l, 0.43f, 0.56f, 0.04f, new FaceOpts { style = Hair.Short, hair = HairBrown, smile = true });
            Head(l, 0.56f, 0.44f, 0.036f, new FaceOpts { style = Hair.Child, hair = HairBrown, smile = true });
            l.Ellipse(0.56f, 0.505f, 0.032f, 0.018f, sailor);                                  // mũ thủy thủ
        });

        p.Sepia(0.92f).Grain(0.07f, 61).Vignette(0.55f);
        p.Border(26, new Color(0.93f, 0.9f, 0.82f));                                              // viền ảnh trắng ngà
        p.Line(0.035f, 0.05f, 0.965f, 0.05f, 2, new Color(0.6f, 0.55f, 0.45f)).Line(0.035f, 0.95f, 0.965f, 0.95f, 2, new Color(0.6f, 0.55f, 0.45f));
        p.Polygon(new Color(0.8f, 0.76f, 0.66f), new Vector2(0.9f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0.86f));     // góc ảnh quăn
        return p;
    }

    // ------------------------------------------------------------------ Chân dung trên tường (độ phân giải cao)
    public static M3Pix WallPortrait(bool torn)
    {
        var p = new M3Pix(660, 900) { lineScale = 1.5f };
        Color goldDark = new Color(0.55f, 0.42f, 0.2f);
        p.Fill(Gold).Rect(0.03f, 0.025f, 0.97f, 0.975f, goldDark).Rect(0.075f, 0.055f, 0.925f, 0.945f, new Color(0.32f, 0.13f, 0.12f));
        p.Ellipse(0.5f, 0.62f, 0.38f, 0.32f, new Color(0.42f, 0.18f, 0.15f), 0.9f);
        p.Ellipse(0.5f, 0.17f, 0.44f, 0.28f, Uniform).Rect(0.08f, 0.055f, 0.92f, 0.17f, Uniform);
        p.Rect(0.1f, 0.33f, 0.3f, 0.36f, Gold).Rect(0.7f, 0.33f, 0.9f, 0.36f, Gold);            // cầu vai
        p.Polygon(UniformDark, new Vector2(0.5f, 0.4f), new Vector2(0.4f, 0.28f), new Vector2(0.5f, 0.06f), new Vector2(0.6f, 0.28f));
        p.Rect(0.13f, 0.18f, 0.29f, 0.24f, Red).Ellipse(0.21f, 0.21f, 0.03f, 0.018f, Black);
        for (int i = 0; i < 3; i++) p.Ellipse(0.66f + i * 0.055f, 0.27f, 0.02f, 0.015f, Gold);
        p.Rect(0.45f, 0.38f, 0.55f, 0.45f, SkinPale);
        p.Ink(3, 0.24f);
        DrawHeadLayer(p, l =>
        {
            Head(l, 0.5f, 0.55f, 0.16f, new FaceOpts { skin = SkinPale, style = Hair.None, frown = true, scar = true, iris = new Color(0.62f, 0.08f, 0.08f) });
            OfficerCap(l, 0.5f, 0.55f, 0.16f, Red);
        }, 2);

        if (torn)
        {
            var t = new M3Pix(p.w, p.h) { lineScale = p.lineScale };
            t.Ellipse(0.5f, 0.5f, 0.35f, 0.36f, new Color(0.11f, 0.10f, 0.10f), 0f, 0.35f);       // lỗ rách lộ tường thép
            t.Polygon(new Color(0.80f, 0.70f, 0.58f), new Vector2(0.2f, 0.78f), new Vector2(0.32f, 0.7f), new Vector2(0.28f, 0.84f));
            t.Polygon(new Color(0.80f, 0.70f, 0.58f), new Vector2(0.78f, 0.24f), new Vector2(0.66f, 0.3f), new Vector2(0.7f, 0.18f));
            t.Polygon(new Color(0.36f, 0.16f, 0.14f), new Vector2(0.72f, 0.72f), new Vector2(0.84f, 0.62f), new Vector2(0.8f, 0.8f));
            p.Over(t.Ink(3, 0.24f));
        }
        return p.Watercolor(0.1f, torn ? 6 : 5);
    }

    // ------------------------------------------------------------------ Em trai toàn thân (cảnh kết)
    public static M3Pix Brother()
    {
        var b = new M3Pix(480, 1200) { lineScale = 1.6f };
        b.Rect(0.26f, 0f, 0.47f, 0.45f, UniformDark).Rect(0.53f, 0f, 0.74f, 0.45f, UniformDark);
        b.Rect(0.24f, 0f, 0.49f, 0.1f, Black).Rect(0.51f, 0f, 0.76f, 0.1f, Black);                // ủng
        b.Polygon(Uniform, new Vector2(0.16f, 0.43f), new Vector2(0.84f, 0.43f), new Vector2(0.88f, 0.79f), new Vector2(0.12f, 0.79f));
        b.Rect(0.16f, 0.45f, 0.84f, 0.48f, Black).Rect(0.46f, 0.452f, 0.54f, 0.478f, Gold);        // thắt lưng
        b.Rect(0.04f, 0.46f, 0.16f, 0.78f, Uniform).Rect(0.84f, 0.46f, 0.96f, 0.78f, Uniform);    // tay
        b.Rect(0.04f, 0.43f, 0.16f, 0.47f, SkinPale).Rect(0.84f, 0.43f, 0.96f, 0.47f, SkinPale);
        b.Rect(0.12f, 0.765f, 0.3f, 0.785f, Gold).Rect(0.7f, 0.765f, 0.88f, 0.785f, Gold);        // cầu vai
        b.Rect(0.12f, 0.64f, 0.27f, 0.69f, Red).Ellipse(0.195f, 0.665f, 0.035f, 0.011f, Black);
        b.Polygon(UniformDark, new Vector2(0.5f, 0.79f), new Vector2(0.42f, 0.72f), new Vector2(0.5f, 0.48f), new Vector2(0.58f, 0.72f));
        for (int i = 0; i < 3; i++) b.Ellipse(0.64f + i * 0.06f, 0.71f, 0.022f, 0.009f, Gold);
        b.Rect(0.45f, 0.79f, 0.55f, 0.83f, SkinPale);
        b.Ink(3, 0.24f);
        DrawHeadLayer(b, l =>
        {
            Head(l, 0.5f, 0.87f, 0.15f, new FaceOpts { skin = SkinPale, style = Hair.None, frown = true, scar = true, iris = new Color(0.62f, 0.08f, 0.08f) });
            OfficerCap(l, 0.5f, 0.87f, 0.15f, Red);
        });
        return b.Watercolor(0.1f, 15);
    }
}
