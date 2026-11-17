using UnityEngine;

// Cho vật phẩm vào túi đồ từ một sự kiện (VD: rạch túi áo -> rơi ra chìa khóa).
public class M3GiveItem : MonoBehaviour
{
    public string itemID;
    public Sprite icon;
    [TextArea] public string line;

    private bool given;

    public void Give()
    {
        if (given || InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.AddItem(icon, itemID))
        {
            given = true;
            M3Dialogue.Say(line);
        }
    }
}
