using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class OpenZoomView : MonoBehaviour
{
    [Header("Kéo Panel cận cảnh cần mở vào đây")]
    public GameObject zoomPanel;

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (zoomPanel != null && !zoomPanel.activeSelf)
        {
            zoomPanel.SetActive(true);
        }
    }
}