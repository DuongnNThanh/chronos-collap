using UnityEngine;
using UnityEngine.EventSystems;

// Được M3ItemInspector tự gắn vào từng ô túi đồ lúc chạy.
// Chuột trái vẫn do Button của ô xử lý (chọn đồ); ở đây chỉ thêm chuột phải / nhấp đúp / rê chuột.
public class M3SlotHook : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (M3ItemInspector.Instance == null) return;
        bool rightClick = eventData.button == PointerEventData.InputButton.Right;
        bool doubleClick = eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2;
        if (rightClick || doubleClick) M3ItemInspector.Instance.ShowSlot(slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (M3ItemInspector.Instance != null) M3ItemInspector.Instance.ShowTooltip(slotIndex, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (M3ItemInspector.Instance != null) M3ItemInspector.Instance.HideTooltip();
    }
}
