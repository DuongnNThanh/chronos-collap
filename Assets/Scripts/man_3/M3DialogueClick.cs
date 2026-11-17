using UnityEngine;
using UnityEngine.EventSystems;

// Gắn lên panel hộp thoại để nhận cú click (panel phải có Image raycastTarget = true)
public class M3DialogueClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (M3Dialogue.Instance != null) M3Dialogue.Instance.Advance();
    }
}
