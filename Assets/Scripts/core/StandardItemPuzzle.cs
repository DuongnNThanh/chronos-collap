using UnityEngine;
using UnityEngine.Events;

public class StandardItemPuzzle : MonoBehaviour
{
    [Header("Cấu hình vật phẩm yêu cầu")]
    public string requiredItemID;           // VD: "m3_chia_khoa"
    public bool consumeItemOnUse = true;    // Có mất vật phẩm sau khi dùng không?

    [Header("Sự kiện khi giải thành công")]
    public UnityEvent onPuzzleSolved;

    private bool isSolved = false;

    // Gọi hàm này từ Button OnClick() hoặc OnMouseDown() của vật thể có Collider 2D
    public void TryInteract()
    {
        if (isSolved) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.IsHoldingItem(requiredItemID))
        {
            isSolved = true;

            if (consumeItemOnUse)
            {
                InventoryManager.Instance.ConsumeSelectedItem();
            }

            onPuzzleSolved?.Invoke();
        }
    }

    private void OnMouseDown()
    {
        TryInteract();
    }
}