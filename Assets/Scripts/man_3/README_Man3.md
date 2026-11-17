# Màn 3 – Nghịch lý quyền lực

Văn phòng đỏ-đen của nhà độc tài (em trai Elias). Mục tiêu: mở két sau bức chân dung và đốt "Hiệp ước Thanh trừng".

## Lời giải – 8 phase (mở rộng từ tài liệu thiết kế)

Phần lõi trong tài liệu (dao mổ → chân dung → ảnh → con mắt → két → hiệp ước → tẩu thuốc) được giữ nguyên. Ngoài ra thêm phần **đồ bị tịch thu** và **kính lúp** để màn dài hơn.

| Phase | Phòng | Người chơi làm gì | Kết quả |
|---|---|---|---|
| 0 | – | Vào màn, có sẵn **dao mổ** (`m3_dao_mo`) giấu trong tay áo | Độc thoại mở màn; HUD hiện **Mục tiêu** và nút **Sổ tay** |
| 1 | Phải | Đọc **biên bản tịch thu** trên bàn | Biết ví + kính lúp bị khóa trong ngăn kéo, chìa khóa ở chỗ sĩ quan trực |
| 2 | Trái | Dao mổ → **áo khoác sĩ quan** (túi khâu kín) | Rơi ra **chìa khóa** (`m3_chia_khoa`) |
| 3 | Phải | Chìa khóa → **ngăn kéo bàn** | Mở ngăn kéo: lấy **ảnh gia đình** (`m3_anh_gia_dinh`) và **kính lúp** (`m3_kinh_lup`) |
| 4 | Giữa | Dao mổ → **bức chân dung** | Tranh rách, lộ **con mắt thịt** và **két sắt**; máu đen rỉ từ khe sàn |
| 5 | Giữa | Ảnh gia đình → **con mắt** | Máu viết `V · I · IV · II · III`; lính gác gõ cửa |
| 6 | Trái | Bấm **bản đồ**, cầm kính lúp bấm vào bản đồ phóng to | Hiện số **dấu chữ thập** dưới từng con mắt |
| 7 | Giữa | Giải mã → **két sắt** → nhập **28614** | Lấy **Hiệp ước Thanh trừng** (`m3_hiep_uoc`) |
| 8 | Phải | Hiệp ước → **tẩu thuốc** | Cảnh kết → sang Màn 4 |

Dao mổ không bao giờ bị mất (dùng 2 lần), và thứ tự phase 2–4 có thể đảo, nên **không có chỗ kẹt màn**.

### Giải mã két (3 bước, manh mối nằm ở cả 3 phòng)

1. **Máu (phòng giữa)**: `V · I · IV · II · III`. Đây không phải chữ số mà là **số hiệu** các con mắt.
2. **Bản đồ (phòng trái)**: bấm vào để phóng to. Có 5 con mắt đỏ đánh số I–V. Phải **cầm kính lúp** bấm vào mới đọc được số **dấu chữ thập**: I=1, II=8, III=2, IV=6, V=4.
3. **Tấm biển đồng (phòng giữa)**: *"Đế chế được viết ngược từ cái kết"*, nghĩa là phải đọc chuỗi máu **từ phải sang trái**: III, II, IV, I, V → **2 8 6 1 4**.

- Bẫy: đọc xuôi sẽ ra `41682` (sai).
- Mở được két, Elias nhận ra **28/6/1914**, ngày vụ ám sát.
- Cứ sai 2 lần thì mở thêm một gợi ý (4 mức, từ mơ hồ đến gần như chỉ thẳng đáp án), xem `M3SafeKeypad.hintLines`.
- Muốn đổi mã: sửa `MapCounts` trong `M3PlaceholderArt.cs`, chuỗi máu trong `Man3SceneBuilder.cs` và `M3SafeKeypad.correctCode`.

## Giao diện

- **Hộp thoại**: có chân dung người nói. Bấm vào hộp (hoặc Space/Enter) lần 1 để hiện hết chữ, lần 2 để đóng.
- **Xem chi tiết vật phẩm**: chuột phải hoặc nhấp đúp vào ô túi đồ để mở bảng có ảnh lớn, tên, mô tả và **công dụng**. Rê chuột lên ô sẽ hiện tên vật phẩm.
- **Mục tiêu và Sổ tay**: góc trên trái. Sổ tay tự ghi lại manh mối.
- Mọi chữ nằm trên **nền đặc** (`ui_textbox`) và dùng font tiếng Việt riêng (`Assets/Sprites/man_3/Fonts`), nên luôn rõ, không phụ thuộc màu của cảnh.

## Cách dựng lại cảnh

Cảnh **tự dựng lại** khi `Man3SceneBuilder.BuildVersion` tăng (xem `M3AutoRebuild`). Muốn dựng lại thủ công thì dùng menu bên dưới.

Menu **Chrono → Màn 3 → Dựng lại cảnh Man_3**: xóa sạch rồi dựng lại toàn bộ cảnh, gồm core_system, 3 phòng, UI, nối sự kiện và Build Settings.

> ⚠️ Lệnh này ghi đè mọi chỉnh sửa tay trong Man_3. Khi đã bắt đầu chỉnh tay trong Unity, đừng chạy lại. Chỉ dùng lệnh **Chỉ vẽ lại ảnh tạm** nếu cần.

## Thay ảnh thật

Ảnh tạm nằm trong `Assets/Sprites/man_3/placeholder/`. Họa sĩ vẽ ảnh thật, đặt **đúng tên file** (VD `portrait_intact.png`) rồi ghi đè vào đó, Unity sẽ tự cập nhật. Kích thước nên giữ cùng tỉ lệ:

| File | Vật thể | Kích thước trong game (đơn vị) |
|---|---|---|
| `bg_wall` | Tường + sàn (dùng chung 3 phòng, tô màu khác nhau) | 28.6 × 16 |
| `portrait_intact` / `portrait_torn` | Chân dung trước / sau khi rạch | 4.4 × 6 |
| `eye_white`, `eye_pupil` | Con mắt thịt, con ngươi | 1.6 × 1 |
| `safe_closed` / `safe_open` | Két sắt | 1.8 × 1.6 |
| `brother` | Em trai (cảnh kết) | 2.4 × 6 |
| `icon_scalpel`, `icon_photo`, `icon_treaty` | Icon túi đồ | 128px vuông/đứng |

## Script

| Script | Việc |
|---|---|
| `M3Interactable` | Bấm để xem / dùng vật phẩm, có câu thoại khi dùng sai đồ |
| `M3Dialogue` | Phụ đề lời thoại cuối màn hình |
| `M3StartingItems` | Cấp dao mổ đầu màn + độc thoại mở màn |
| `M3PortraitCut` | Phase 1 – rạch tranh |
| `M3Eye`, `M3BloodNumerals` | Phase 2 – con mắt chớp, dõi theo chuột, nhỏ máu thành số La Mã |
| `M3SafeKeypad` | Phase 3 – bàn phím két (gõ được bằng bàn phím máy tính) |
| `M3EndingCutscene` | Phase 4 – cảnh kết, gọi `GameManager.CompleteCurrentLevel()` |
| `M3Clock`, `M3Flicker`, `M3Grow`, `M3ScreenFader`, `M3Fx` | Hiệu ứng không khí |

Âm thanh: `M3EndingCutscene` có sẵn các ô `fireClip`, `rumbleClip`, `doorClip`, `gunshotClip` và `M3Clock.tickSource`. Kéo file âm thanh vào là chạy.
