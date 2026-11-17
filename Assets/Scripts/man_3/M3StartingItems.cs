using System.Collections;
using UnityEngine;

// Túi đồ bị xóa khi qua màn, nên các vật phẩm mang từ màn trước (VD: dao mổ)
// được cấp lại ở đầu màn 3, kèm câu thoại mở màn.
public class M3StartingItems : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string itemID;
        public Sprite icon;
    }

    public Entry[] items;

    [Header("Câu thoại mở màn")]
    public float introDelay = 1.5f;
    [TextArea] public string[] introLines;
    public float secondsBetweenLines = 4.5f;

    private IEnumerator Start()
    {
        yield return null; // chờ InventoryManager khởi tạo xong

        if (InventoryManager.Instance != null && items != null)
        {
            foreach (Entry e in items)
            {
                if (!string.IsNullOrEmpty(e.itemID) && !InventoryManager.Instance.HasItem(e.itemID))
                {
                    InventoryManager.Instance.AddItem(e.icon, e.itemID);
                }
            }
        }

        yield return new WaitForSeconds(introDelay);

        if (introLines == null) yield break;
        foreach (string line in introLines)
        {
            M3Dialogue.Say(line);
            yield return new WaitForSeconds(secondsBetweenLines);
        }
    }
}
