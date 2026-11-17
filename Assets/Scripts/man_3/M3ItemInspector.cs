using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Xem chi tiết vật phẩm trong túi đồ: CHUỘT PHẢI hoặc NHẤP ĐÚP vào ô -> bảng hiện ảnh lớn, tên, mô tả, công dụng.
// Rê chuột lên ô -> hiện tên vật phẩm. Không cần sửa prefab core: lúc chạy tự gắn M3SlotHook vào các ô túi đồ.
public class M3ItemInspector : MonoBehaviour
{
    [System.Serializable]
    public class ItemInfo
    {
        public string itemID;
        public string displayName;
        [TextArea] public string description;
        [TextArea] public string usage;
        public Sprite image;
    }

    public static M3ItemInspector Instance { get; private set; }

    public ItemInfo[] items;

    [Header("Bảng chi tiết")]
    public GameObject panel;
    public Image bigImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text usageText;

    [Header("Nhãn tên khi rê chuột")]
    public RectTransform tooltip;
    public TMP_Text tooltipText;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private IEnumerator Start()
    {
        yield return null; // chờ InventoryManager khởi tạo
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null || inv.slots == null) yield break;

        for (int i = 0; i < inv.slots.Length; i++)
        {
            if (inv.slots[i] == null) continue;
            M3SlotHook hook = inv.slots[i].GetComponent<M3SlotHook>();
            if (hook == null) hook = inv.slots[i].gameObject.AddComponent<M3SlotHook>();
            hook.slotIndex = i;
        }
    }

    public ItemInfo Find(string itemID)
    {
        if (items == null || string.IsNullOrEmpty(itemID)) return null;
        foreach (ItemInfo it in items)
        {
            if (it != null && it.itemID == itemID) return it;
        }
        return null;
    }

    private static string ItemAt(int slotIndex)
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null || inv.slotItemIDs == null || slotIndex < 0 || slotIndex >= inv.slotItemIDs.Length) return null;
        return inv.slotItemIDs[slotIndex];
    }

    public void ShowSlot(int slotIndex)
    {
        ItemInfo info = Find(ItemAt(slotIndex));
        if (info == null) return;
        HideTooltip();

        if (bigImage != null)
        {
            bigImage.sprite = info.image;
            bigImage.preserveAspect = true;
        }
        if (titleText != null) titleText.text = info.displayName;
        if (descriptionText != null) descriptionText.text = info.description;
        if (usageText != null) usageText.text = "<b>Công dụng:</b> " + info.usage;
        if (panel != null) panel.SetActive(true);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void ShowTooltip(int slotIndex, RectTransform slot)
    {
        ItemInfo info = Find(ItemAt(slotIndex));
        if (info == null || tooltip == null) return;

        tooltipText.text = info.displayName + "\n<size=70%><color=#C8B48C>Chuột phải để xem chi tiết</color></size>";
        tooltip.gameObject.SetActive(true);
        // Nhãn có pivot ở mép phải -> đặt sát mép trái của ô, hiện bên trái thanh túi đồ
        float halfWidth = slot.rect.width * 0.5f * slot.lossyScale.x;
        tooltip.position = slot.position + Vector3.left * (halfWidth + 12f * slot.lossyScale.x);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }
}
