using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Vật thể bấm được của màn 3 (dùng cho cả Sprite ngoài phòng lẫn UI Image trong panel).
// - Bấm tay không      -> Elias nói inspectLine, gọi onInspect (VD: mở panel cận cảnh)
// - Cầm đúng vật phẩm  -> giải xong, gọi onSolved
// - Cầm sai vật phẩm   -> Elias phản ứng bằng wrongItemLine
// Sprite ngoài phòng cần Collider2D, và camera cần Physics2DRaycaster (Man3SceneBuilder tự thêm).
public class M3Interactable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Phóng nhẹ khi rê chuột (gợi ý bấm được)")]
    public bool growOnHover = true;
    public float hoverScale = 1.04f;
    private Vector3 baseScale;
    private bool hasBaseScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!growOnHover) return;
        if (!hasBaseScale) { baseScale = transform.localScale; hasBaseScale = true; }
        transform.localScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hasBaseScale) transform.localScale = baseScale;
    }

    private void OnDisable()
    {
        if (hasBaseScale) transform.localScale = baseScale;
    }

    [Header("Lời thoại khi xem (bấm tay không)")]
    [TextArea] public string inspectLine;

    [Header("Vật phẩm yêu cầu (để trống nếu chỉ để xem)")]
    public string requiredItemID;
    public bool consumeItem = true;
    [TextArea] public string solvedLine;
    [TextArea] public string wrongItemLine = "Không được. Thứ này không dùng ở đây.";

    [Header("Sau khi đã giải")]
    [TextArea] public string afterSolvedLine;

    [Header("Bỏ qua vật phẩm đang cầm (dùng cho đồ nhặt được)")]
    public bool ignoreHeldItem = false;

    [Header("Sự kiện")]
    public UnityEvent onInspect = new UnityEvent();
    public UnityEvent onSolved = new UnityEvent();
    public UnityEvent onInspectAfterSolved = new UnityEvent(); // bấm lại sau khi đã giải (VD: mở lại ngăn kéo)

    private bool isSolved;
    public bool IsSolved => isSolved;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Interact();
    }

    public void Interact()
    {
        InventoryManager inv = InventoryManager.Instance;
        string held = (inv != null && !ignoreHeldItem) ? inv.selectedItemID : "";
        bool needsItem = !string.IsNullOrEmpty(requiredItemID);

        if (isSolved)
        {
            M3Dialogue.Say(string.IsNullOrEmpty(afterSolvedLine) ? inspectLine : afterSolvedLine);
            onInspectAfterSolved?.Invoke();
            return;
        }

        if (!string.IsNullOrEmpty(held))
        {
            if (needsItem && held == requiredItemID)
            {
                isSolved = true;
                if (consumeItem) inv.ConsumeSelectedItem();
                else inv.DeselectAll();

                M3Dialogue.Say(solvedLine);
                onSolved?.Invoke();
            }
            else
            {
                M3Dialogue.Say(wrongItemLine);
            }
            return;
        }

        M3Dialogue.Say(inspectLine);
        onInspect?.Invoke();
    }
}
