using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Giao diện 6 ô icon (kéo Slot_1_Icon -> Slot_6_Icon vào đây)")]
    public Image[] slotIconImages;

    [Header("Giao diện 6 ô Button cha (kéo Slot_1 -> Slot_6 vào đây)")]
    public Transform[] slots;

    [Header("Hiệu ứng Chọn đồ (Grow)")]
    public float selectScale = 1.15f; // Phóng to 15% khi được chọn
    public Color selectColor = new Color(1f, 1.0f, 1.0f, 1f); // Giữ nguyên màu gốc (hoặc xám/vàng nếu muốn)

    [HideInInspector]
    public string[] slotItemIDs;
    [HideInInspector]
    public string selectedItemID = ""; // ID món đồ đang được chọn cầm trên tay
    private int currentSelectedIndex = -1; // Index ô đang được chọn

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        slotItemIDs = new string[slotIconImages.Length];

        // Reset trạng thái ban đầu của các ô chứa
        for (int i = 0; i < slotIconImages.Length; i++)
        {
            if (slotIconImages[i] != null)
            {
                slotIconImages[i].sprite = null;
                slotIconImages[i].color = Color.white; // Đảm bảo vật phẩm giữ nguyên màu gốc
                slotIconImages[i].gameObject.SetActive(false);
            }
            slotItemIDs[i] = "";

            if (slots != null && i < slots.Length && slots[i] != null)
            {
                slots[i].localScale = Vector3.one; // Trở về kích thước 1 bình thường
            }
        }
    }

    // Hàm tự động nhét đồ vào ô trống đầu tiên
    public bool AddItem(Sprite itemSprite, string itemID)
    {
        for (int i = 0; i < slotIconImages.Length; i++)
        {
            if (string.IsNullOrEmpty(slotItemIDs[i]))
            {
                slotItemIDs[i] = itemID;
                slotIconImages[i].sprite = itemSprite;
                slotIconImages[i].color = Color.white; // Ép giữ nguyên màu gốc của ảnh khi nhặt
                slotIconImages[i].preserveAspect = true;
                slotIconImages[i].gameObject.SetActive(true);
                return true; // Nhặt đồ thành công
            }
        }
        Debug.Log("Túi đồ đã đầy!");
        return false;
    }

    // Hàm gọi khi Click vào ô đồ (Được Button của Slot gọi)
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotItemIDs.Length) return;
        if (string.IsNullOrEmpty(slotItemIDs[slotIndex])) return; // Ô trống thì không làm gì

        // Nếu bấm lại vào ô đang chọn -> Bỏ chọn (nhỏ lại)
        if (currentSelectedIndex == slotIndex)
        {
            DeselectAll();
        }
        else
        {
            // Bỏ chọn ô cũ trước
            DeselectAll();

            // Chọn ô mới: Phóng to (Grow)
            currentSelectedIndex = slotIndex;
            selectedItemID = slotItemIDs[slotIndex];

            if (slots[slotIndex] != null)
            {
                slots[slotIndex].localScale = new Vector3(selectScale, selectScale, 1f); // Hiệu ứng Grow
            }
        }
    }

    // Hàm bỏ chọn toàn bộ các ô (Trở lại kích thước bình thường)
    public void DeselectAll()
    {
        selectedItemID = "";
        currentSelectedIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].localScale = Vector3.one; // Reset về tỷ lệ 1x1 thông thường
            }
        }
    }
    // 1. Kiểm tra người chơi có đang chọn đúng vật phẩm này không
    public bool IsHoldingItem(string itemID)
    {
        return !string.IsNullOrEmpty(selectedItemID) && selectedItemID == itemID;
    }

// 2. Xóa vật phẩm đang cầm sau khi đã sử dụng xong (VD: dùng chìa khóa mở cửa)
    public void ConsumeSelectedItem()
    {
        if (string.IsNullOrEmpty(selectedItemID)) return;

        int index = currentSelectedIndex;
        DeselectAll();
        ClearSlot(index);
    }

// 3. Làm trống toàn bộ túi đồ khi chuyển sang màn chơi mới
    public void ClearAllItems()
    {
        DeselectAll();
        if (slotItemIDs == null) return;
        for (int i = 0; i < slotItemIDs.Length; i++)
        {
            ClearSlot(i);
        }
    }

    // 4. Kiểm tra trong túi có vật phẩm này không (không cần đang cầm)
    public bool HasItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID) || slotItemIDs == null) return false;
        return System.Array.IndexOf(slotItemIDs, itemID) >= 0;
    }

    // Làm trống 1 ô: xóa ID và ẩn icon
    private void ClearSlot(int index)
    {
        if (slotItemIDs == null || index < 0 || index >= slotItemIDs.Length) return;

        slotItemIDs[index] = "";
        if (slotIconImages[index] != null)
        {
            slotIconImages[index].sprite = null;
            slotIconImages[index].gameObject.SetActive(false);
        }
    }
}