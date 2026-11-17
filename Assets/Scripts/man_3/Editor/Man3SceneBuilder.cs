using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Dựng toàn bộ cảnh Man_3 "Nghịch lý quyền lực" theo tài liệu thiết kế:
//   Menu: Chrono > Màn 3 > Dựng lại cảnh Man_3
//   Batch: Unity.exe -batchmode -projectPath . -executeMethod Man3SceneBuilder.BuildBatch -quit
// Ảnh dùng tạm được vẽ tự động vào Assets/Sprites/man_3/placeholder – thay bằng ảnh thật cùng tên là xong.
public static class Man3SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Man_3.unity";
    private const string CorePrefabPath = "Assets/Prefabs/Core/core_system.prefab";
    private const string SpriteDir = "Assets/Sprites/man_3/placeholder";

    // ID vật phẩm màn 3
    public const string ItemScalpel = "m3_dao_mo";
    public const string ItemPhoto = "m3_anh_gia_dinh";
    public const string ItemTreaty = "m3_hiep_uoc";
    public const string ItemKey = "m3_chia_khoa";

    // Tăng số này mỗi khi sửa cách dựng cảnh/ảnh -> Unity sẽ tự dựng lại Man_3 (xem M3AutoRebuild)
    public const string BuildVersion = "9";
    public const string ItemMagnifier = "m3_kinh_lup";

    // Thanh túi đồ của core chiếm 200px mép phải – nền tối của panel không được che nó,
    // để người chơi vẫn chọn được vật phẩm khi đang mở panel (VD: cầm kính lúp soi bản đồ)
    private const float InventoryBarWidth = 200f;

    private const float CX = -1.5f; // tâm phòng (lệch trái vì thanh túi đồ chiếm mép phải)

    private static Dictionary<string, Sprite> S = new Dictionary<string, Sprite>();

    [MenuItem("Chrono/Màn 3/Dựng lại cảnh Man_3")]
    public static void BuildFromMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Dựng lại Man_3", "Hãy bấm Stop (thoát Play mode) trước.", "OK");
            return;
        }
        // Chạy ở khung hình sau, ngoài vòng vẽ giao diện của Unity
        // (tránh lỗi "EndLayoutGroup: BeginLayoutGroup must be called first" khi nạp cảnh giữa chừng)
        EditorApplication.delayCall += () =>
        {
            if (!EditorUtility.DisplayDialog("Dựng lại Man_3",
                    "Toàn bộ nội dung hiện có trong Man_3 sẽ bị xóa và dựng lại từ đầu. Tiếp tục?", "Dựng lại", "Hủy"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        };
    }

    [MenuItem("Chrono/Màn 3/Chỉ vẽ lại ảnh tạm")]
    public static void RegenerateSpritesOnly()
    {
        GenerateSprites();
        AssetDatabase.SaveAssets();
    }

    public static void BuildBatch()
    {
        Build();
    }

    private static void Build()
    {
        GenerateSprites();
        VietFont = M3FontMaker.GetOrCreate();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        // ---------- Core ----------
        var corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CorePrefabPath);
        var core = (GameObject)PrefabUtility.InstantiatePrefab(corePrefab, scene);
        foreach (Transform t in core.GetComponentsInChildren<Transform>(true))
        {
            // Panel riêng của màn 1 nằm trong core: tắt đi ở màn 3
            if (t.name == "DrawerZoom_Panel" || t.name == "NurseCoatZoom_Panel") t.gameObject.SetActive(false);
        }
        Camera cam = core.GetComponentInChildren<Camera>(true);
        if (cam.GetComponent<Physics2DRaycaster>() == null) cam.gameObject.AddComponent<Physics2DRaycaster>();
        foreach (Light2D l in core.GetComponentsInChildren<Light2D>(true))
        {
            if (l.lightType == Light2D.LightType.Global)
            {
                l.intensity = 0.9f;                  // đủ sáng để nét vẽ rõ, đèn đỏ chỉ tạo không khí
                l.color = new Color(1f, 0.93f, 0.9f);
            }
        }
        RoomManager roomManager = core.GetComponentInChildren<RoomManager>(true);

        // ---------- Phòng ----------
        var roomLeft = new GameObject("room_left");
        var roomCenter = new GameObject("room_center");
        var roomRight = new GameObject("room_right");

        var so = new SerializedObject(roomManager);
        so.FindProperty("roomLeft").objectReferenceValue = roomLeft;
        so.FindProperty("roomCenter").objectReferenceValue = roomCenter;
        so.FindProperty("roomRight").objectReferenceValue = roomRight;
        so.ApplyModifiedProperties();

        // ---------- UI riêng màn 3 ----------
        var ui = BuildCanvas();
        M3Dialogue dialogue = BuildDialogue(ui);
        M3Journal journal = BuildJournal(ui);
        GameObject drawerPanel = BuildDrawerPanel(ui);
        GameObject mapPanel = BuildMapPanel(ui, journal);
        M3SafeKeypad keypad = BuildSafePanel(ui);
        BuildItemInspector(ui);
        M3ScreenFader fader = BuildFader(ui); // luôn là con cuối cùng -> phủ lên tất cả
        dialogue.transform.SetSiblingIndex(fader.transform.GetSiblingIndex()); // hộp thoại nằm trên mọi panel, dưới màn đen

        // Object luôn bật: chứa các chuỗi hiệu ứng dài, để chúng không bị ngắt khi người chơi chuyển phòng
        var flow = new GameObject("M3_GameFlow");

        // ---------- Phòng giữa: chân dung, két, cửa, cửa sổ, đồng hồ ----------
        Transform c = roomCenter.transform;
        Spr("bg", c, new Vector2(0, 0), S["bg_wall"], 0);

        var portraitGroup = new GameObject("portrait").transform;
        portraitGroup.SetParent(c, false);
        portraitGroup.localPosition = new Vector3(CX, 1.6f, 0);

        GameObject portraitIntact = Spr("portrait_intact", portraitGroup, Vector2.zero, S["portrait_intact"], 5, true);
        GameObject portraitTorn = Spr("portrait_torn", portraitGroup, Vector2.zero, S["portrait_torn"], 5);
        portraitTorn.SetActive(false);
        GameObject slash = Spr("slash", portraitGroup, new Vector2(0, 0.5f), S["slash"], 8);
        slash.transform.localRotation = Quaternion.Euler(0, 0, -50f);
        slash.SetActive(false);

        // Con mắt
        var eyeRoot = new GameObject("eye");
        eyeRoot.transform.SetParent(c, false);
        eyeRoot.transform.localPosition = new Vector3(CX, 2.75f, 0);
        GameObject eyeBody = Spr("eye_body", eyeRoot.transform, Vector2.zero, S["eye_white"], 9);
        GameObject pupil = Spr("eye_pupil", eyeBody.transform, Vector2.zero, S["eye_pupil"], 10);
        var eyeCol = eyeRoot.AddComponent<BoxCollider2D>();
        eyeCol.size = new Vector2(1.7f, 1.1f);
        M3Eye eye = eyeRoot.AddComponent<M3Eye>();
        eye.eyeBody = eyeBody.transform;
        eye.pupil = pupil.transform;
        M3Interactable eyeInt = eyeRoot.AddComponent<M3Interactable>();
        eyeInt.inspectLine = "Nó chớp... nó thật sự đang sống. Có lẽ nó phản ứng với ký ức nào đó của mình.";
        eyeInt.requiredItemID = ItemPhoto;
        eyeInt.wrongItemLine = "Con mắt chỉ trừng trừng nhìn. Nó không quan tâm đến thứ này.";
        eyeInt.afterSolvedLine = "Con mắt đã nhắm lại. Những con số bằng máu vẫn còn dưới sàn.";
        eyeRoot.SetActive(false);

        // Két sắt
        var safeRoot = new GameObject("safe");
        safeRoot.transform.SetParent(c, false);
        safeRoot.transform.localPosition = new Vector3(CX, 0.35f, 0);
        GameObject safeClosed = Spr("safe_closed", safeRoot.transform, Vector2.zero, S["safe_closed"], 9);
        GameObject safeOpen = Spr("safe_open", safeRoot.transform, Vector2.zero, S["safe_open"], 9);
        safeOpen.SetActive(false);
        var safeCol = safeRoot.AddComponent<BoxCollider2D>();
        safeCol.size = new Vector2(1.8f, 1.6f);
        M3Interactable safeInt = safeRoot.AddComponent<M3Interactable>();
        safeInt.inspectLine = "Một chiếc két sắt giấu sau bức tranh. Khóa bằng mật mã năm chữ số.";
        safeInt.wrongItemLine = "Thứ này không mở được két. Mình cần mật mã.";
        UnityEventTools.AddPersistentListener(safeInt.onInspect, keypad.Open);
        UnityEventTools.AddBoolPersistentListener(keypad.onUnlocked, safeClosed.SetActive, false);
        UnityEventTools.AddBoolPersistentListener(keypad.onUnlocked, safeOpen.SetActive, true);
        safeRoot.SetActive(false);

        GameObject nameplate = Spr("nameplate", c, new Vector2(CX, -1.95f), S["nameplate"], 6, true);
        M3Interactable plateInt = Inspect(nameplate, "Tấm biển đồng khắc: \"Đế chế được viết ngược từ cái kết.\"");
        UnityEventTools.AddStringPersistentListener(plateInt.onInspect, journal.AddEntry,
            "Tấm biển đồng dưới chân dung: \"Đế chế được viết ngược từ cái kết.\"");

        // Máu đen rỉ từ khe sàn
        GameObject crackA = Spr("floor_crack_a", c, new Vector2(6f, -6.6f), S["crack"], 2);
        M3Grow crackAGrow = Grow(crackA, new Vector3(0.35f, 1, 1), new Vector3(1.6f, 1.4f, 1), 2.5f);
        GameObject crackB = Spr("floor_crack_b", c, new Vector2(-8f, -7f), S["crack"], 2);
        crackB.transform.localRotation = Quaternion.Euler(0, 0, 160f);
        M3Grow crackBGrow = Grow(crackB, new Vector3(0.35f, 1, 1), new Vector3(1.5f, 1.3f, 1), 2.5f);
        GameObject oozeA = Spr("ooze_a", c, new Vector2(6f, -6.7f), S["ooze"], 3);
        M3Grow oozeAGrow = Grow(oozeA, new Vector3(0.05f, 0.05f, 1), Vector3.one, 30f);
        GameObject oozeB = Spr("ooze_b", c, new Vector2(-8f, -7.1f), S["ooze"], 3);
        M3Grow oozeBGrow = Grow(oozeB, new Vector3(0.05f, 0.05f, 1), new Vector3(0.8f, 0.8f, 1), 30f);

        // 5 ký hiệu La Mã bằng máu: V · I · IV · II · III
        // Đây KHÔNG phải chữ số – là số hiệu 5 con mắt trên bản đồ (phòng trái).
        // Đọc ngược (tấm biển) -> III, II, IV, I, V -> đếm vạch chữ thập ở từng con mắt -> 2 8 6 1 4
        string[] roman = { "V", "I", "IV", "II", "III" };
        var numerals = new TMP_Text[roman.Length];
        for (int i = 0; i < roman.Length; i++)
        {
            numerals[i] = WorldText("blood_" + roman[i], c, roman[i], new Vector2(CX - 4.4f + i * 2.2f, -5.5f), 11f,
                new Color(0.5f, 0.02f, 0.02f), 15);
        }
        GameObject drop = Spr("blood_drop_template", c, new Vector2(CX, 2.6f), S["blood_drop"], 14);
        M3BloodNumerals blood = flow.AddComponent<M3BloodNumerals>();
        blood.eye = eye;
        blood.numerals = numerals;
        blood.dropTemplate = drop.GetComponent<SpriteRenderer>();
        blood.startLine = "Bức ảnh gia đình... nó co giật... nó đang khóc ra máu!";
        blood.endLine = "V, I, IV, II, III... Số La Mã, nhưng không phải mật mã – năm số này chỉ là thứ tự. Chúng đánh dấu thứ gì đó trong căn phòng này.";
        UnityEventTools.AddPersistentListener(eyeInt.onSolved, blood.Reveal);
        UnityEventTools.AddStringPersistentListener(blood.onRevealed, journal.AddEntry,
            "Máu từ con mắt viết dưới sàn: V · I · IV · II · III");
        UnityEventTools.AddStringPersistentListener(blood.onRevealed, journal.MarkDone, "blood");

        // Lính gác gõ cửa sau khi con mắt chảy máu – tạo áp lực thời gian (chỉ là không khí, không giới hạn giờ)
        M3LineSequence guard = flow.AddComponent<M3LineSequence>();
        guard.lines = new[]
        {
            new M3LineSequence.Line { delay = 3f, speaker = "Lính gác", text = "Thưa Tổng tư lệnh? Có tiếng động trong đó... Ngài ổn chứ?", shakeCamera = true },
            new M3LineSequence.Line { delay = 4.5f, speaker = "Elias", text = "Hắn sắp quay lại... Phải mở chiếc két đó thật nhanh." }
        };
        UnityEventTools.AddPersistentListener(blood.onRevealed, guard.Play);

        // Rạch tranh
        M3PortraitCut cut = flow.AddComponent<M3PortraitCut>();
        cut.portraitIntact = portraitIntact;
        cut.portraitTorn = portraitTorn;
        cut.slashLine = slash.transform;
        cut.revealAfterCut = new[] { eyeRoot, safeRoot };
        cut.startOozing = new[] { oozeAGrow, oozeBGrow };
        cut.afterCutLine = "Một con mắt... bằng thịt?! Nó đang nhìn mình. Và bên dưới là một chiếc két sắt!";
        M3Interactable portraitInt = portraitIntact.AddComponent<M3Interactable>();
        portraitInt.inspectLine = "Em trai mình... trong bộ quân phục độc tài. Ánh mắt lạnh lẽo như đang dõi theo mình. Lớp vải cộm lên, như có thứ gì phía sau.";
        portraitInt.requiredItemID = ItemScalpel;
        portraitInt.wrongItemLine = "Thứ này không làm gì được bức tranh. Cần thứ gì đó thật sắc.";
        portraitInt.solvedLine = "Xin lỗi em...";
        portraitInt.consumeItem = false; // dao mổ còn dùng cho áo khoác – không được mất, tránh kẹt màn
        UnityEventTools.AddPersistentListener(portraitInt.onSolved, cut.Play);
        UnityEventTools.AddStringPersistentListener(portraitInt.onSolved, journal.MarkDone, "portrait_cut");
        UnityEventTools.AddStringPersistentListener(keypad.onUnlocked, journal.MarkDone, "safe_open");
        UnityEventTools.AddStringPersistentListener(keypad.onUnlocked, journal.MarkDone, "map_read"); // phòng khi đoán trúng mã

        // Cửa chính, đồng hồ, cửa sổ
        var door = new GameObject("door").transform;
        door.SetParent(c, false);
        door.localPosition = new Vector3(-10.2f, -0.9f, 0);
        GameObject doorClosed = Spr("door_closed", door, Vector2.zero, S["door_closed"], 4, true);
        Inspect(doorClosed, "Khóa chặt từ bên ngoài. Có tiếng giày lính gác ngoài hành lang.");
        GameObject doorOpen = Spr("door_open", door, Vector2.zero, S["door_open"], 4);
        doorOpen.SetActive(false);

        var clockRoot = new GameObject("clock");
        clockRoot.transform.SetParent(c, false);
        clockRoot.transform.localPosition = new Vector3(-10.2f, 5.2f, 0);
        GameObject clockFace = Spr("clock_face", clockRoot.transform, Vector2.zero, S["clock_face"], 4, true);
        Inspect(clockFace, "Đồng hồ vẫn chạy... tích tắc... như đang đếm ngược điều gì đó.");
        M3Clock clock = clockRoot.AddComponent<M3Clock>();
        clock.hourHand = Hand("hour", clockRoot.transform, S["hand_hour"], 30f);
        clock.minuteHand = Hand("minute", clockRoot.transform, S["hand_minute"], 160f);
        clock.secondHand = Hand("second", clockRoot.transform, S["hand_second"], 0f);

        GameObject window = Spr("window", c, new Vector2(7.2f, 2.3f), S["window"], 4, true);
        Inspect(window, "Bên ngoài chỉ có khói và đèn pha quét qua những dãy nhà đổ nát.");
        GameObject shadow = Spr("window_shadow", window.transform, new Vector2(0.3f, -0.3f), S["window_shadow"], 5);
        shadow.SetActive(false);
        GameObject flash = Spr("muzzle_flash", window.transform, new Vector2(-0.6f, 0.1f), S["muzzle_flash"], 6);
        flash.SetActive(false);

        // Em trai + bọ cạp
        GameObject brother = Spr("brother", c, new Vector2(-10.2f, -1.6f), S["brother"], 30);
        brother.SetActive(false);
        GameObject gun = Spr("brother_gun", brother.transform, new Vector2(1.0f, 0.4f), S["gun"], 31);
        gun.SetActive(false);
        GameObject pool = Spr("blood_pool", c, new Vector2(-10.6f, -5.6f), S["blood_pool"], 3);
        pool.SetActive(false);
        GameObject scorpion = Spr("scorpion", c, new Vector2(-10.6f, -5.6f), S["scorpion"], 32);
        scorpion.SetActive(false);
        var crackTarget = new GameObject("scorpion_target").transform;
        crackTarget.SetParent(c, false);
        crackTarget.localPosition = new Vector3(6f, -6.6f, 0);

        // Đèn đỏ nhấp nháy
        var redLight = new GameObject("red_light");
        redLight.transform.SetParent(c, false);
        redLight.transform.localPosition = new Vector3(CX, 4.5f, 0);
        AddRedLight(redLight, 13f);

        // ---------- Phòng trái: bản đồ, cờ, áo khoác sĩ quan ----------
        Transform lft = roomLeft.transform;
        GameObject bgL = Spr("bg", lft, Vector2.zero, S["bg_wall"], 0);
        bgL.GetComponent<SpriteRenderer>().color = new Color(0.92f, 0.86f, 0.86f);
        M3Interactable mapInt = Inspect(Spr("world_map", lft, new Vector2(CX, 2.2f), S["map"], 4, true),
            "Bản đồ thế giới bị bôi đen từng mảng. Có những dấu con mắt đỏ... lại gần xem kỹ hơn.");
        UnityEventTools.AddBoolPersistentListener(mapInt.onInspect, mapPanel.SetActive, true);
        const string flagLine = "Biểu tượng mắt xích và con mắt. Ở đâu trong căn phòng này cũng có con mắt đang nhìn mình.";
        Inspect(Spr("flag_left", lft, new Vector2(-9.6f, 1.6f), S["flag"], 4, true), flagLine);
        Inspect(Spr("flag_right", lft, new Vector2(8.6f, 1.6f), S["flag"], 4, true), flagLine);

        // Phase 1: áo khoác sĩ quan – túi ngực khâu kín giấu chìa khóa ngăn kéo. Dùng dao mổ rạch chỉ.
        GameObject coat = Spr("officer_coat", lft, new Vector2(4.7f, -1.4f), S["coat"], 5, true);
        M3Interactable coatInt = coat.AddComponent<M3Interactable>();
        coatInt.inspectLine = "Áo khoác sĩ quan trực. Túi ngực cộm lên một vật nhỏ... nhưng miệng túi bị khâu kín.";
        coatInt.requiredItemID = ItemScalpel;
        coatInt.consumeItem = false;
        coatInt.wrongItemLine = "Không được, đường chỉ khâu quá chắc. Cần thứ gì sắc để rạch.";
        coatInt.solvedLine = "Rạch đường chỉ...";
        coatInt.afterSolvedLine = "Túi áo đã trống.";
        M3GiveItem giveKey = coat.AddComponent<M3GiveItem>();
        giveKey.itemID = ItemKey;
        giveKey.icon = S["icon_key"];
        giveKey.line = "Một chiếc chìa khóa nhỏ bằng đồng! Hẳn là chìa khóa ngăn kéo bàn làm việc.";
        UnityEventTools.AddPersistentListener(coatInt.onSolved, giveKey.Give);
        UnityEventTools.AddStringPersistentListener(coatInt.onSolved, journal.MarkDone, "has_key");
        GameObject crackL = Spr("floor_crack", lft, new Vector2(4f, -7f), S["crack"], 2);
        crackL.transform.localScale = new Vector3(1.2f, 1f, 1f);
        var redLightL = new GameObject("red_light");
        redLightL.transform.SetParent(lft, false);
        redLightL.transform.localPosition = new Vector3(CX, 4.5f, 0);
        AddRedLight(redLightL, 11f);

        // ---------- Phòng phải: bàn làm việc, tẩu thuốc, tờ báo ----------
        Transform rgt = roomRight.transform;
        GameObject bgR = Spr("bg", rgt, Vector2.zero, S["bg_wall"], 0);
        bgR.GetComponent<SpriteRenderer>().color = new Color(0.88f, 0.86f, 0.92f);
        Inspect(Spr("bookshelf", rgt, new Vector2(-9.4f, 0.6f), S["bookshelf"], 3, true),
            "Toàn điều lệ quân đội và danh sách người bị bắt giữ. Có cả tên những người mình từng quen.");
        Spr("desk", rgt, new Vector2(CX, -4.2f), S["desk"], 4);

        // Phase 1–2: biên bản tịch thu + ngăn kéo khóa chứa đồ của Elias
        GameObject note = Spr("confiscation_note", rgt, new Vector2(-1.4f, -2.5f), S["note"], 6, true);
        M3Interactable noteInt = Inspect(note, "Biên bản tịch thu: \"Tù nhân E. Voss – 01 ví da, 01 kính lúp. Niêm phong trong ngăn kéo bàn. Chìa khóa: sĩ quan trực giữ.\"");
        UnityEventTools.AddStringPersistentListener(noteInt.onInspect, journal.AddEntry,
            "Biên bản tịch thu: ví và kính lúp của mình bị khóa trong ngăn kéo bàn. Chìa khóa do sĩ quan trực giữ.");
        UnityEventTools.AddStringPersistentListener(noteInt.onInspect, journal.MarkDone, "note_read");

        var drawer = new GameObject("desk_drawer").transform;
        drawer.SetParent(rgt, false);
        drawer.localPosition = new Vector3(-4.7f, -3.55f, 0);
        GameObject drawerClosed = Spr("drawer_closed", drawer, Vector2.zero, S["drawer_closed"], 6);
        GameObject drawerOpen = Spr("drawer_open", drawer, Vector2.zero, S["drawer_open"], 6);
        drawerOpen.SetActive(false);
        var drawerCol = drawer.gameObject.AddComponent<BoxCollider2D>();
        drawerCol.size = new Vector2(2.8f, 1f);
        M3Interactable drawerInt = drawer.gameObject.AddComponent<M3Interactable>();
        drawerInt.inspectLine = "Ngăn kéo bị khóa. Nghe như có đồ bên trong.";
        drawerInt.requiredItemID = ItemKey;
        drawerInt.wrongItemLine = "Không vừa ổ khóa.";
        drawerInt.solvedLine = "Cạch! Ngăn kéo mở ra.";
        drawerInt.afterSolvedLine = "Đồ của mình vẫn nằm trong ngăn kéo.";
        UnityEventTools.AddBoolPersistentListener(drawerInt.onSolved, drawerClosed.SetActive, false);
        UnityEventTools.AddBoolPersistentListener(drawerInt.onSolved, drawerOpen.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(drawerInt.onSolved, drawerPanel.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(drawerInt.onInspectAfterSolved, drawerPanel.SetActive, true);
        UnityEventTools.AddStringPersistentListener(drawerInt.onSolved, journal.MarkDone, "drawer_open");
        Inspect(Spr("newspaper", rgt, new Vector2(-4.2f, -2.4f), S["newspaper"], 6, true),
            "Tờ báo cũ ố vàng: \"SARAJEVO – THÁI TỬ THOÁT NẠN\". Ở dòng thời gian này, viên đạn ấy chưa từng trúng đích... và mọi thứ đã bắt đầu từ đó.");
        GameObject pipe = Spr("pipe", rgt, new Vector2(1.6f, -2.45f), S["pipe"], 6, true);
        GameObject smoke = Spr("pipe_smoke", rgt, new Vector2(1.95f, -1.55f), S["smoke"], 7);
        M3Flicker smokeFx = smoke.AddComponent<M3Flicker>();
        smokeFx.minValue = 0.15f;
        smokeFx.maxValue = 0.6f;
        smokeFx.speed = 1.2f;
        smokeFx.bobUp = true;
        GameObject fire = Spr("green_fire", rgt, new Vector2(1.6f, -1.5f), S["green_fire"], 8);
        M3Flicker fireFx = fire.AddComponent<M3Flicker>();
        fireFx.minValue = 0.7f;
        fireFx.speed = 8f;
        fire.SetActive(false);
        M3Interactable pipeInt = pipe.AddComponent<M3Interactable>();
        pipeInt.inspectLine = "Tẩu thuốc vẫn còn âm ỉ cháy. Hắn vừa mới rời khỏi đây...";
        pipeInt.requiredItemID = ItemTreaty;
        pipeInt.wrongItemLine = "Đốt thứ này chẳng để làm gì.";
        pipeInt.solvedLine = "Cháy đi... cùng với mọi tội ác của nó.";
        var redLightR = new GameObject("red_light");
        redLightR.transform.SetParent(rgt, false);
        redLightR.transform.localPosition = new Vector3(CX, 4.5f, 0);
        AddRedLight(redLightR, 11f);

        // ---------- Luồng màn chơi ----------
        M3StartingItems start = flow.AddComponent<M3StartingItems>();
        start.items = new[] { new M3StartingItems.Entry { itemID = ItemScalpel, icon = S["icon_scalpel"] } };
        start.introLines = new[]
        {
            "...Mình đang ở đâu? Lần trước là hầm trú ẩn... còn đây là một văn phòng.",
            "Cờ đỏ, biểu tượng con mắt... và bức chân dung kia. Không thể nào... là em trai mình?",
            "Chúng đã lục soát mình... May mà con dao mổ vẫn giấu trong tay áo.",
            "Phải tìm hiểu chuyện gì đã xảy ra ở dòng thời gian này. Sổ tay ở góc trên sẽ ghi lại mọi manh mối."
        };

        M3EndingCutscene ending = flow.AddComponent<M3EndingCutscene>();
        ending.roomManager = roomManager;
        ending.fader = fader;
        ending.greenFire = fire;
        ending.clock = clock;
        ending.cracksToSpread = new[] { crackAGrow, crackBGrow };
        ending.doorClosed = doorClosed;
        ending.doorOpen = doorOpen;
        ending.brother = brother.GetComponent<SpriteRenderer>();
        ending.brotherGun = gun;
        ending.windowShadow = shadow;
        ending.muzzleFlash = flash;
        ending.bloodPool = pool.GetComponent<SpriteRenderer>();
        ending.scorpion = scorpion.transform;
        ending.crackTarget = crackTarget;
        ending.audioSource = flow.AddComponent<AudioSource>();
        ending.audioSource.playOnAwake = false;
        UnityEventTools.AddPersistentListener(pipeInt.onSolved, ending.Play);
        UnityEventTools.AddStringPersistentListener(pipeInt.onSolved, journal.MarkDone, "burned");

        // ---------- Gọn Scene view: chỉ hiện phòng giữa, màn đen trong suốt (lúc chạy sẽ tự bật lại) ----------
        roomLeft.SetActive(false);
        roomRight.SetActive(false);
        fader.GetComponent<CanvasGroup>().alpha = 0f;

        // ---------- Lưu ----------
        AddScenesToBuildSettings();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        M3AutoRebuild.WriteStamp();
        Debug.Log("[Man3SceneBuilder] Đã dựng xong Man_3 (phiên bản " + BuildVersion + ").");
    }

    // ======================================================================
    // UI
    // ======================================================================

    private static Transform BuildCanvas()
    {
        var go = new GameObject("M3_Canvas", typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvas.pixelPerfect = true; // chữ khớp lưới điểm ảnh -> mép rõ, không nhòe
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go.transform;
    }

    private static M3Dialogue BuildDialogue(Transform canvas)
    {
        RectTransform root = UI("M3_Dialogue", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        // Hộp thoại 1440x230: khung chân dung bên trái, tên người nói + lời thoại bên phải
        RectTransform panel = UI("Panel", root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-100, 22), new Vector2(1440, 250));
        panel.pivot = new Vector2(0.5f, 0);
        panel.anchoredPosition = new Vector2(-100, 22);
        // Nền đặc 100% (không trong suốt) -> chữ không bị ảnh hưởng bởi màu cảnh; nhận click để tua/đóng
        TextBox(panel.gameObject, true);
        panel.gameObject.AddComponent<M3DialogueClick>();

        RectTransform frame = UI("PortraitFrame", panel, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(118, 0), new Vector2(196, 196));
        Frame(frame.gameObject, new Color(0.75f, 0.62f, 0.4f), false);
        RectTransform portraitRect = UI("Portrait", frame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Stretch(portraitRect, new Vector4(10, 10, 10, 10));
        Image portrait = Img(portraitRect.gameObject, Color.white, S["speaker_elias"], false);
        portrait.preserveAspect = true;

        TextMeshProUGUI speaker = Txt("Speaker", panel, "Elias", 36, new Color(1f, 0.8f, 0.4f), TextAlignmentOptions.TopLeft);
        speaker.fontStyle = FontStyles.Bold;
        Stretch(speaker.rectTransform, new Vector4(244, 18, 36, 184));
        TextMeshProUGUI line = Txt("Line", panel, "", 38, new Color(1f, 0.98f, 0.94f), TextAlignmentOptions.TopLeft);
        line.lineSpacing = 8f;
        Stretch(line.rectTransform, new Vector4(244, 70, 40, 34));

        TextMeshProUGUI hint = Txt("ContinueHint", panel, "Bấm để đóng »", 24, new Color(0.78f, 0.7f, 0.55f), TextAlignmentOptions.BottomRight);
        Stretch(hint.rectTransform, new Vector4(244, 208, 26, 8));
        M3Flicker hintBlink = hint.gameObject.AddComponent<M3Flicker>();
        hintBlink.minValue = 0.35f;
        hintBlink.maxValue = 1f;
        hintBlink.speed = 2.5f;

        M3Dialogue d = root.gameObject.AddComponent<M3Dialogue>();
        d.continueHint = hint.gameObject;
        d.panel = panel.gameObject;
        d.speakerText = speaker;
        d.lineText = line;
        d.portraitImage = portrait;
        d.unknownPortrait = S["speaker_unknown"];
        d.portraits = new[]
        {
            new M3Dialogue.SpeakerPortrait { speaker = "Elias", portrait = S["speaker_elias"] },
            new M3Dialogue.SpeakerPortrait { speaker = "Em trai", portrait = S["speaker_brother"] },
            new M3Dialogue.SpeakerPortrait { speaker = "Lính gác", portrait = S["speaker_guard"] }
        };
        panel.gameObject.SetActive(false);
        return d;
    }

    // Xem chi tiết vật phẩm (chuột phải / nhấp đúp vào ô túi đồ) + nhãn tên khi rê chuột
    private static void BuildItemInspector(Transform canvas)
    {
        RectTransform root = UI("M3_ItemInspector", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        M3ItemInspector insp = root.gameObject.AddComponent<M3ItemInspector>();

        RectTransform panel = UI("Item_Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button dim = Dim(panel);
        UnityEventTools.AddPersistentListener(dim.onClick, insp.Close);

        RectTransform window = UI("Window", panel, Center, Center, new Vector2(-100, 30), new Vector2(1180, 600));
        TextBox(window.gameObject, true);

        RectTransform imgFrame = UI("ImageFrame", window, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(270, 0), new Vector2(460, 460));
        Frame(imgFrame.gameObject, new Color(0.85f, 0.82f, 0.76f), false);
        RectTransform imgRect = UI("Image", imgFrame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Stretch(imgRect, new Vector4(30, 30, 30, 30));
        Image big = Img(imgRect.gameObject, Color.white, null, false);
        big.preserveAspect = true;

        TextMeshProUGUI title = Txt("Title", window, "", 46, new Color(1f, 0.82f, 0.45f), TextAlignmentOptions.TopLeft);
        title.fontStyle = FontStyles.Bold;
        Stretch(title.rectTransform, new Vector4(540, 50, 50, 470));
        TextMeshProUGUI desc = Txt("Description", window, "", 32, new Color(0.96f, 0.94f, 0.9f), TextAlignmentOptions.TopLeft);
        desc.lineSpacing = 6f;
        Stretch(desc.rectTransform, new Vector4(540, 135, 50, 240));
        TextMeshProUGUI usage = Txt("Usage", window, "", 31, new Color(0.7f, 0.9f, 0.68f), TextAlignmentOptions.TopLeft);
        usage.lineSpacing = 6f;
        Stretch(usage.rectTransform, new Vector4(540, 380, 50, 40));
        Button close = CloseButton(window, null);
        UnityEventTools.AddPersistentListener(close.onClick, insp.Close);

        // Nhãn tên khi rê chuột lên ô túi đồ (pivot mép phải, không chặn chuột)
        RectTransform tip = UI("Tooltip", root, Center, Center, Vector2.zero, new Vector2(400, 92));
        tip.pivot = new Vector2(1f, 0.5f);
        TextBox(tip.gameObject, false);
        TextMeshProUGUI tipText = Txt("Text", tip, "", 30, new Color(1f, 0.95f, 0.85f), TextAlignmentOptions.MidlineRight);
        Stretch(tipText.rectTransform, new Vector4(18, 8, 20, 8));

        insp.panel = panel.gameObject;
        insp.bigImage = big;
        insp.titleText = title;
        insp.descriptionText = desc;
        insp.usageText = usage;
        insp.tooltip = tip;
        insp.tooltipText = tipText;
        insp.items = new[]
        {
            new M3ItemInspector.ItemInfo
            {
                itemID = ItemScalpel, displayName = "Dao mổ", image = S["icon_scalpel"],
                description = "Con dao mổ của Clara, lấy từ hộp kỷ vật. Elias giấu nó trong tay áo nên lính gác lục soát không tìm thấy.",
                usage = "Rạch những thứ bị khâu kín hay che phủ. Không bị mất sau khi dùng."
            },
            new M3ItemInspector.ItemInfo
            {
                itemID = ItemKey, displayName = "Chìa khóa đồng", image = S["icon_key"],
                description = "Chiếc chìa nhỏ giấu trong túi ngực áo khoác của sĩ quan trực.",
                usage = "Vừa với một ổ khóa nhỏ... có lẽ của một ngăn kéo."
            },
            new M3ItemInspector.ItemInfo
            {
                itemID = ItemPhoto, displayName = "Ảnh gia đình", image = S["icon_photo"],
                description = "Cha, mẹ, Elias và em trai chụp ở studio trước chiến tranh. Lần cuối cả nhà còn bình yên – khi em trai còn cười.",
                usage = "Một ký ức quý giá. Có thứ gì đó trong căn phòng này phản ứng với ký ức của Elias."
            },
            new M3ItemInspector.ItemInfo
            {
                itemID = ItemMagnifier, displayName = "Kính lúp", image = S["icon_magnifier"],
                description = "Kính lúp của Elias, bị tịch thu cùng chiếc ví.",
                usage = "Soi những ký hiệu li ti mà mắt thường không đọc được."
            },
            new M3ItemInspector.ItemInfo
            {
                itemID = ItemTreaty, displayName = "Hiệp ước Thanh trừng", image = S["icon_treaty"],
                description = "Văn bản đóng dấu đỏ, ký tên Tổng tư lệnh: kế hoạch thanh trừng hàng nghìn người.",
                usage = "Phải phá hủy nó. Cần một ngọn lửa."
            }
        };

        panel.gameObject.SetActive(false);
        tip.gameObject.SetActive(false);
    }

    // Ngăn kéo bàn (mở bằng chìa khóa): ví của Elias (ảnh gia đình + giấy tờ) và kính lúp
    private static GameObject BuildDrawerPanel(Transform canvas)
    {
        RectTransform panel = UI("Drawer_Panel", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button dim = Dim(panel);
        UnityEventTools.AddBoolPersistentListener(dim.onClick, panel.gameObject.SetActive, false);

        RectTransform window = UI("Window", panel, Center, Center, new Vector2(-100, -10), new Vector2(1100, 616));
        Img(window.gameObject, Color.white, S["ui_drawer"], true);
        Txt("Title", window, "Ngăn kéo bàn làm việc", 36, new Color(0.95f, 0.88f, 0.7f), TextAlignmentOptions.Center)
            .rectTransform.anchoredPosition = new Vector2(0, 345);

        RectTransform wallet = UI("Wallet_Open", window, Center, Center, new Vector2(-170, -20), new Vector2(620, 403));
        Img(wallet.gameObject, Color.white, S["ui_wallet_open"], true);

        RectTransform photo = UI("Item_anh_gia_dinh", wallet, Center, Center, new Vector2(150, 10), new Vector2(260, 200));
        Img(photo.gameObject, Color.white, S["icon_photo"], true).preserveAspect = true;
        PickableItem pick = photo.gameObject.AddComponent<PickableItem>();
        pick.itemID = ItemPhoto;
        pick.itemIcon = S["icon_photo"];
        M3Interactable photoInt = photo.gameObject.AddComponent<M3Interactable>();
        photoInt.ignoreHeldItem = true;
        photoInt.inspectLine = "Ảnh gia đình hồi nhỏ... cả nhà còn bình yên. Khi ấy em trai mình vẫn còn cười.";

        RectTransform card = UI("IdCard", wallet, Center, Center, new Vector2(-150, 10), new Vector2(240, 160));
        Img(card.gameObject, new Color(0.86f, 0.81f, 0.69f), null, true);
        TextMeshProUGUI cardText = Txt("Text", card, "ELIAS VOSS\nGiấy phép nghiên cứu\n1950", 22, new Color(0.2f, 0.15f, 0.1f), TextAlignmentOptions.Center);
        Stretch(cardText.rectTransform, new Vector4(8, 8, 8, 8));
        M3Interactable cardInt = card.gameObject.AddComponent<M3Interactable>();
        cardInt.ignoreHeldItem = true;
        cardInt.inspectLine = "Giấy tờ của mình, năm 1950. Ở dòng thời gian này, chắc chẳng còn ai nhớ cái tên Elias Voss.";

        RectTransform mag = UI("Item_kinh_lup", window, Center, Center, new Vector2(370, -10), new Vector2(210, 210));
        Img(mag.gameObject, Color.white, S["icon_magnifier"], true).preserveAspect = true;
        PickableItem pickMag = mag.gameObject.AddComponent<PickableItem>();
        pickMag.itemID = ItemMagnifier;
        pickMag.itemIcon = S["icon_magnifier"];
        M3Interactable magInt = mag.gameObject.AddComponent<M3Interactable>();
        magInt.ignoreHeldItem = true;
        magInt.inspectLine = "Kính lúp của mình! Dùng để soi những chi tiết mắt thường không thấy được.";

        CloseButton(window, panel.gameObject);
        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    // HUD góc trên trái: nút Sổ tay + dòng Mục tiêu; panel sổ tay ghi lại manh mối
    private static M3Journal BuildJournal(Transform canvas)
    {
        RectTransform root = UI("M3_Journal", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        M3Journal journal = root.gameObject.AddComponent<M3Journal>();

        // Dòng mục tiêu (không chặn click)
        RectTransform obj = UI("Objective", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(860, 74));
        obj.pivot = new Vector2(0, 1);
        obj.anchoredPosition = new Vector2(130, -22);
        TextBox(obj.gameObject, false);
        TextMeshProUGUI objText = Txt("Text", obj, "", 30, new Color(0.97f, 0.94f, 0.88f), TextAlignmentOptions.MidlineLeft);
        Stretch(objText.rectTransform, new Vector4(24, 8, 20, 8));
        journal.objectiveText = objText;

        // Nút sổ tay
        RectTransform btnRect = UI("Btn_Journal", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(92, 92));
        btnRect.pivot = new Vector2(0, 1);
        btnRect.anchoredPosition = new Vector2(22, -14);
        Image btnBg = Img(btnRect.gameObject, new Color(0.85f, 0.82f, 0.78f), S["ui_key"], true);
        btnBg.type = Image.Type.Sliced;
        Button btn = btnRect.gameObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, journal.ToggleJournal);
        RectTransform icon = UI("Icon", btnRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Stretch(icon, new Vector4(12, 12, 12, 12));
        Img(icon.gameObject, Color.white, S["icon_journal"], false).preserveAspect = true;
        RectTransform badge = UI("NewBadge", btnRect, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -6), new Vector2(26, 26));
        Img(badge.gameObject, new Color(0.85f, 0.15f, 0.12f), null, false);
        journal.newBadge = badge.gameObject;

        // Panel sổ tay
        RectTransform panel = UI("Journal_Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button dim = Dim(panel);
        UnityEventTools.AddPersistentListener(dim.onClick, journal.CloseJournal);
        RectTransform window = UI("Window", panel, Center, Center, new Vector2(-100, -20), new Vector2(1000, 760));
        TextBox(window.gameObject, true); // sổ tay: nền chữ đặc cho dễ đọc
        Txt("Title", window, "Sổ tay của Elias", 40, new Color(0.98f, 0.92f, 0.78f), TextAlignmentOptions.Center)
            .rectTransform.anchoredPosition = new Vector2(0, 330);
        TextMeshProUGUI entries = Txt("Entries", window, "", 31, new Color(0.97f, 0.95f, 0.9f), TextAlignmentOptions.TopLeft);
        Stretch(entries.rectTransform, new Vector4(56, 110, 56, 50));
        journal.entriesText = entries;
        Button close = CloseButton(window, null);
        UnityEventTools.AddPersistentListener(close.onClick, journal.CloseJournal);
        journal.journalPanel = panel.gameObject;

        journal.objectives = new List<M3Journal.Objective>
        {
            new M3Journal.Objective { doneFlag = "note_read", text = "Lục soát văn phòng." },
            new M3Journal.Objective { doneFlag = "has_key", requiresFlag = "note_read", text = "Tìm chìa khóa ngăn kéo – sĩ quan trực đang giữ nó. Áo của hắn ở đâu nhỉ?" },
            new M3Journal.Objective { doneFlag = "drawer_open", requiresFlag = "has_key", text = "Mở ngăn kéo bàn làm việc, lấy lại đồ bị tịch thu." },
            new M3Journal.Objective { doneFlag = "portrait_cut", text = "Bức chân dung có gì đó bất thường... thứ gì nằm sau lớp vải?" },
            new M3Journal.Objective { doneFlag = "blood", text = "Con mắt phản ứng với ký ức – cho nó xem thứ gợi nhớ gia đình." },
            new M3Journal.Objective { doneFlag = "map_read", text = "Năm ký hiệu máu là số thứ tự. Thứ gì khác trong phòng cũng được đánh số La Mã?" },
            new M3Journal.Objective { doneFlag = "safe_open", text = "Giải mật mã két sắt 5 chữ số – xem lại sổ tay." },
            new M3Journal.Objective { doneFlag = "burned", text = "Phá hủy Hiệp ước Thanh trừng – cần một ngọn lửa." }
        };
        journal.allDoneText = "...";
        return journal;
    }

    private static M3SafeKeypad BuildSafePanel(Transform canvas)
    {
        RectTransform panel = UI("Safe_Panel", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button dim = Dim(panel);

        RectTransform window = UI("Window", panel, Center, Center, new Vector2(-100, 0), new Vector2(720, 880));
        Frame(window.gameObject, new Color(0.85f, 0.85f, 0.9f), true);
        Txt("Title", window, "KÉT SẮT", 40, new Color(0.95f, 0.92f, 0.85f), TextAlignmentOptions.Center)
            .rectTransform.anchoredPosition = new Vector2(0, 385);

        // --- Màn nhập mã ---
        RectTransform keypadView = UI("KeypadView", window, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform screen = UI("Screen", keypadView, Center, Center, new Vector2(0, 250), new Vector2(560, 120));
        Frame(screen.gameObject, new Color(0.18f, 0.2f, 0.18f), false);
        TextMeshProUGUI display = Txt("Display", screen, "_ _ _ _ _", 72, new Color(0.85f, 0.8f, 0.65f), TextAlignmentOptions.Center);
        Stretch(display.rectTransform, Vector4.zero);

        var digits = new Button[10];
        string[] layout = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
        Button clear = null, enter = null;
        for (int i = 0; i < layout.Length; i++)
        {
            int row = i / 3, col = i % 3;
            Vector2 pos = new Vector2(-180 + col * 180, 100 - row * 130);
            Color bg = layout[i] == "OK" ? new Color(0.75f, 1f, 0.75f) : layout[i] == "C" ? new Color(1f, 0.7f, 0.7f) : Color.white;
            Button b = LabeledButton("Key_" + layout[i], keypadView, layout[i], pos, new Vector2(160, 110), bg, 48);
            if (layout[i] == "C") clear = b;
            else if (layout[i] == "OK") enter = b;
            else digits[int.Parse(layout[i])] = b;
        }

        // --- Bên trong két ---
        RectTransform contents = UI("ContentsView", window, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform inner = UI("Interior", contents, Center, Center, new Vector2(0, -20), new Vector2(660, 700));
        Frame(inner.gameObject, new Color(0.35f, 0.33f, 0.33f), true);
        Txt("Hint", contents, "Bên trong là những tập hồ sơ mật.", 28, new Color(0.8f, 0.75f, 0.7f), TextAlignmentOptions.Center)
            .rectTransform.anchoredPosition = new Vector2(0, 300);

        Folder(contents, "Folder_DanhSach", "DANH SÁCH\nTHANH TRỪNG", new Vector2(-215, -20),
            "Danh sách thanh trừng... hàng nghìn cái tên, có cả trẻ con. Ký tên: Tổng tư lệnh – chính là em trai mình.");
        Folder(contents, "Folder_PhongToa", "KẾ HOẠCH\nPHONG TỎA", new Vector2(0, -20),
            "Kế hoạch phong tỏa các thành phố \"bất tuân\". Không lương thực, không thuốc men.");
        GameObject treaty = Folder(contents, "Item_hiep_uoc", "HIỆP ƯỚC\nTHANH TRỪNG", new Vector2(215, -20),
            "\"Hiệp ước Thanh trừng\"... chính là nó. Phải hủy nó đi. Mình cần một ngọn lửa.");
        treaty.GetComponent<Image>().sprite = S["ui_treaty"];
        treaty.GetComponent<Image>().color = Color.white;
        PickableItem pick = treaty.AddComponent<PickableItem>();
        pick.itemID = ItemTreaty;
        pick.itemIcon = S["icon_treaty"];
        contents.gameObject.SetActive(false);

        M3SafeKeypad keypad = window.gameObject.AddComponent<M3SafeKeypad>();
        keypad.panelRoot = panel.gameObject;
        keypad.keypadView = keypadView.gameObject;
        keypad.contentsView = contents.gameObject;
        keypad.display = display;
        keypad.digitButtons = digits;
        keypad.clearButton = clear;
        keypad.enterButton = enter;

        UnityEventTools.AddPersistentListener(dim.onClick, keypad.Close);
        Button close = CloseButton(window, null);
        UnityEventTools.AddPersistentListener(close.onClick, keypad.Close);

        panel.gameObject.SetActive(false);
        return keypad;
    }

    // Khung UI kiểu ô túi đồ màn 1 (nền xám loang màu nước, viền mực đen), co giãn 9-slice
    private static Image Frame(GameObject go, Color tint, bool raycast)
    {
        Image img = Img(go, tint, S["ui_panel"], raycast);
        img.type = Image.Type.Sliced;
        return img;
    }

    // Bản đồ phóng to: 5 con mắt đánh số La Mã. Dưới mỗi con mắt có mảnh giấy ký hiệu li ti –
    // phải cầm KÍNH LÚP (lấy ở ngăn kéo) rồi bấm vào bản đồ mới đọc được số dấu chữ thập.
    private static GameObject BuildMapPanel(Transform canvas, M3Journal journal)
    {
        RectTransform panel = UI("Map_Panel", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button dim = Dim(panel);
        UnityEventTools.AddBoolPersistentListener(dim.onClick, panel.gameObject.SetActive, false);

        RectTransform window = UI("Window", panel, Center, Center, new Vector2(-100, 0), new Vector2(1240, 860));
        Frame(window.gameObject, new Color(0.8f, 0.78f, 0.75f), true);
        Txt("Title", window, "Bản đồ thanh trừng", 38, new Color(0.95f, 0.92f, 0.85f), TextAlignmentOptions.Center)
            .rectTransform.anchoredPosition = new Vector2(0, 375);

        Vector2 mapSize = new Vector2(1140, 665);
        RectTransform map = UI("MapImage", window, Center, Center, new Vector2(0, -25), mapSize);
        Img(map.gameObject, Color.white, S["ui_map_zoom"], true);
        RectTransform tallies = UI("Tallies_Magnified", map, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Img(tallies.gameObject, Color.white, S["ui_map_tallies"], false);
        tallies.gameObject.SetActive(false);

        M3Interactable mapInt = map.gameObject.AddComponent<M3Interactable>();
        mapInt.growOnHover = false;
        mapInt.inspectLine = "Năm con mắt đỏ đánh số I đến V. Dưới mỗi con mắt có mảnh giấy ghi ký hiệu li ti... quá nhỏ để đọc. Cần thứ gì đó phóng to.";
        mapInt.requiredItemID = ItemMagnifier;
        mapInt.consumeItem = false;
        mapInt.wrongItemLine = "Thứ này không giúp mình đọc được những ký hiệu li ti.";
        mapInt.solvedLine = "Qua kính lúp... mỗi mảnh giấy là những dấu chữ thập. Mỗi dấu là một đợt thanh trừng.";
        mapInt.afterSolvedLine = "Những dấu chữ thập dưới mỗi con mắt... số lượng khác nhau.";
        UnityEventTools.AddBoolPersistentListener(mapInt.onSolved, tallies.gameObject.SetActive, true);
        UnityEventTools.AddStringPersistentListener(mapInt.onSolved, journal.AddEntry,
            "Bản đồ (phòng trái): 5 con mắt đánh số I–V, dưới mỗi con mắt có một số dấu chữ thập khác nhau.");
        UnityEventTools.AddStringPersistentListener(mapInt.onSolved, journal.MarkDone, "map_read");

        for (int i = 0; i < M3PlaceholderArt.MapMarks.Length; i++)
        {
            Vector2 m = M3PlaceholderArt.MapMarks[i];
            Vector2 pos = new Vector2((m.x - 0.5f) * mapSize.x, (m.y - 0.5f) * mapSize.y + 52f);
            TextMeshProUGUI label = Txt("Mark_" + M3PlaceholderArt.MapRoman[i], map, M3PlaceholderArt.MapRoman[i], 40, new Color(0.45f, 0.04f, 0.04f), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            label.rectTransform.sizeDelta = new Vector2(120, 50);
            label.rectTransform.anchoredPosition = pos;
        }

        CloseButton(window, panel.gameObject);
        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    private static M3ScreenFader BuildFader(Transform canvas)
    {
        RectTransform r = UI("M3_Fader", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Img(r.gameObject, Color.black, null, true);
        r.gameObject.AddComponent<CanvasGroup>();
        return r.gameObject.AddComponent<M3ScreenFader>();
    }

    private static GameObject Folder(Transform parent, string name, string label, Vector2 pos, string line)
    {
        RectTransform r = UI(name, parent, Center, Center, pos, new Vector2(190, 250));
        Img(r.gameObject, Color.white, S["ui_folder"], true);
        TextMeshProUGUI t = Txt("Label", r, label, 24, new Color(0.25f, 0.1f, 0.08f), TextAlignmentOptions.Center);
        Stretch(t.rectTransform, new Vector4(12, 40, 12, 40));
        M3Interactable i = r.gameObject.AddComponent<M3Interactable>();
        i.ignoreHeldItem = true;
        i.inspectLine = line;
        return r.gameObject;
    }

    private static Button CloseButton(RectTransform window, GameObject panelToHide)
    {
        Vector2 half = window.sizeDelta * 0.5f;
        Button b = LabeledButton("Btn_Close", window, "X", new Vector2(half.x - 40, half.y - 40), new Vector2(64, 64), new Color(1f, 0.6f, 0.6f), 36);
        if (panelToHide != null) UnityEventTools.AddBoolPersistentListener(b.onClick, panelToHide.SetActive, false);
        return b;
    }

    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    private static RectTransform UI(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return r;
    }

    // padding: trái, trên, phải, dưới
    private static void Stretch(RectTransform r, Vector4 padding)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(padding.x, padding.w);
        r.offsetMax = new Vector2(-padding.z, -padding.y);
    }

    private static Image Img(GameObject go, Color color, Sprite sprite, bool raycast)
    {
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = sprite;
        img.raycastTarget = raycast;
        return img;
    }

    // Nền tối phía sau cửa sổ (bấm vào để đóng). Phải là object con riêng, không gắn lên panel cha,
    // nếu không mọi cú click bên trong cửa sổ sẽ "nổi bọt" lên Button này và đóng panel.
    private static Button Dim(RectTransform panel)
    {
        RectTransform r = UI("Dim", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        r.offsetMin = Vector2.zero;
        r.offsetMax = new Vector2(-InventoryBarWidth, 0f);
        Img(r.gameObject, new Color(0, 0, 0, 0.75f), null, true);
        Button b = r.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        return b;
    }

    private static TextMeshProUGUI Txt(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        RectTransform r = UI(name, parent, Center, Center, Vector2.zero, new Vector2(600, 60));
        TextMeshProUGUI t = r.gameObject.AddComponent<TextMeshProUGUI>();
        ApplyFont(t, color);
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    // Font tiếng Việt riêng; chữ sáng màu dùng material có viền + bóng để nổi rõ trên mọi nền,
    // chữ tối (nhãn nút, giấy tờ) giữ material thường cho nét gọn.
    private static TMP_FontAsset VietFont;

    // Mọi chữ dùng material trơn của font (không viền/bóng): viền + bóng vượt quá vùng đệm SDF
    // làm mép chữ bị răng cưa ("gai"). Độ rõ đến từ nền chữ đặc (TextBox) thay vì viền.
    private static void ApplyFont(TMP_Text t, Color color)
    {
        if (VietFont == null) return;
        t.font = VietFont;
        t.fontSharedMaterial = VietFont.material;
        t.extraPadding = true;
    }

    // Nền chữ đặc, màu phẳng, viền vàng mảnh (9-slice)
    private static Image TextBox(GameObject go, bool raycast)
    {
        Image img = Img(go, Color.white, S["ui_textbox"], raycast);
        img.type = Image.Type.Sliced;
        return img;
    }

    private static Button LabeledButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Color tint, float fontSize)
    {
        RectTransform r = UI(name, parent, Center, Center, pos, size);
        Image img = Img(r.gameObject, tint, S["ui_key"], true);
        img.type = Image.Type.Sliced;
        Button b = r.gameObject.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1f, 0.95f, 0.85f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        b.colors = cb;
        TextMeshProUGUI t = Txt("Label", r, label, fontSize, new Color(0.1f, 0.07f, 0.06f), TextAlignmentOptions.Center);
        t.fontStyle = FontStyles.Bold;
        Stretch(t.rectTransform, Vector4.zero);
        return b;
    }

    // ======================================================================
    // Đối tượng trong phòng
    // ======================================================================

    private static GameObject Spr(string name, Transform parent, Vector2 pos, Sprite sprite, int order, bool collider = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        if (collider) go.AddComponent<BoxCollider2D>(); // tự khớp kích thước sprite
        return go;
    }

    private static M3Interactable Inspect(GameObject go, string line)
    {
        M3Interactable i = go.AddComponent<M3Interactable>();
        i.inspectLine = line;
        i.wrongItemLine = "Không dùng thứ này ở đây được.";
        return i;
    }

    private static M3Grow Grow(GameObject go, Vector3 from, Vector3 to, float seconds)
    {
        M3Grow g = go.AddComponent<M3Grow>();
        g.startScale = from;
        g.endScale = to;
        g.duration = seconds;
        go.transform.localScale = from;
        return g;
    }

    private static Transform Hand(string name, Transform clock, Sprite sprite, float angle)
    {
        var pivot = new GameObject(name + "_pivot").transform;
        pivot.SetParent(clock, false);
        pivot.localRotation = Quaternion.Euler(0, 0, -angle);
        var sr = new GameObject(name).AddComponent<SpriteRenderer>();
        sr.transform.SetParent(pivot, false);
        sr.sprite = sprite; // pivot của sprite đặt ở chân kim
        sr.sortingOrder = 5;
        return pivot;
    }

    private static TMP_Text WorldText(string name, Transform parent, string text, Vector2 pos, float size, Color color, int order)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        TextMeshPro t = go.AddComponent<TextMeshPro>();
        if (VietFont != null) t.font = VietFont;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(2.2f, 1.4f);
        t.sortingOrder = order;
        return t;
    }

    private static void AddRedLight(GameObject go, float radius)
    {
        Light2D l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = new Color(1f, 0.15f, 0.1f);
        l.intensity = 0.5f;
        l.pointLightOuterRadius = radius * 0.8f;
        l.pointLightInnerRadius = 1f;
        M3Flicker f = go.AddComponent<M3Flicker>();
        f.light2D = l;
        f.minValue = 0.45f;
        f.maxValue = 0.9f;
        f.speed = 2f;
    }

    private static void AddScenesToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        bool changed = false;
        for (int i = 1; i <= 5; i++)
        {
            string path = "Assets/Scenes/Man_" + i + ".unity";
            if (scenes.Any(s => s.path == path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            changed = true;
        }
        if (changed) EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void GenerateSprites()
    {
        S = M3PlaceholderArt.Generate();
    }
}
