using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PickableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Thông tin vật phẩm đưa vào Inventory")]
    public string itemID;        // Ví dụ: "kim_chi", "chia_khoa"
    public Sprite itemIcon;      // Kéo file ảnh icon của vật phẩm vào đây

    [Header("Đổi ảnh nền sau khi nhặt (Tùy chọn - ví dụ ngăn kéo trống)")]
    public Image targetUIImage;          // Dùng nếu nền là UI Image (như DrawerZoom_Panel)
    public SpriteRenderer targetSprite;  // Dùng nếu nền là 2D Sprite ngoài phòng
    public Sprite spriteAfterPickup;     // Ảnh thay thế sau khi đã lấy đồ

    // 1. Dành cho vật phẩm dạng UI Image (trong màn hình Zoom)
    public void OnPointerClick(PointerEventData eventData)
    {
        Collect();
    }

    // 2. Dành cho vật phẩm dạng 2D Sprite (ngoài phòng)
    private void OnMouseDown()
    {
        // Chặn click xuyên qua UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Collect();
    }

    private void Collect()
    {
        if (InventoryManager.Instance == null) return;

        // Gửi vật phẩm vào ô trống đầu tiên trong hàng đợi Inventory
        bool isAdded = InventoryManager.Instance.AddItem(itemIcon, itemID);

        if (isAdded)
        {
            // Đổi trạng thái hình nền (nếu có gán)
            if (spriteAfterPickup != null)
            {
                if (targetUIImage != null) targetUIImage.sprite = spriteAfterPickup;
                if (targetSprite != null) targetSprite.sprite = spriteAfterPickup;
            }

            // Ẩn vật phẩm khỏi màn chơi
            gameObject.SetActive(false);
        }
    }
}