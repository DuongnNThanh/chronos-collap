using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cài đặt màu và độ tỏa viền sáng")]
    public Color glowColor = new Color(0.357f, 0.357f, 0.357f, 0.55f); // Mã màu #5B5B5B
    public float glowScale = 1.03f; // Phóng nhẹ 3% để tạo viền sáng bao quanh

    private GameObject glowOverlay;

    private void Start()
    {
        // Trường hợp 1: Đối tượng là UI Image (nằm trong Canvas)
        Image uiImage = GetComponent<Image>();
        if (uiImage != null)
        {
            glowOverlay = new GameObject(gameObject.name + "_Glow");
            RectTransform glowRect = glowOverlay.AddComponent<RectTransform>();
            glowRect.SetParent(transform, false);
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
            glowRect.localScale = new Vector3(glowScale, glowScale, 1f);

            Image glowImg = glowOverlay.AddComponent<Image>();
            glowImg.sprite = uiImage.sprite;
            glowImg.preserveAspect = uiImage.preserveAspect;
            glowImg.color = glowColor;
            glowImg.raycastTarget = false; // Không chặn tia chuột của vật phẩm gốc

            glowOverlay.SetActive(false);
            return;
        }

        // Trường hợp 2: Đối tượng là 2D Sprite ngoài phòng
        SpriteRenderer mainSr = GetComponent<SpriteRenderer>();
        if (mainSr != null)
        {
            glowOverlay = new GameObject(gameObject.name + "_Glow");
            glowOverlay.transform.SetParent(transform, false);
            glowOverlay.transform.localPosition = Vector3.zero;
            glowOverlay.transform.localScale = new Vector3(glowScale, glowScale, 1f);

            SpriteRenderer glowSr = glowOverlay.AddComponent<SpriteRenderer>();
            glowSr.sprite = mainSr.sprite;
            glowSr.sortingLayerID = mainSr.sortingLayerID;
            glowSr.sortingOrder = mainSr.sortingOrder + 1;
            glowSr.color = glowColor;

            glowOverlay.SetActive(false);
        }
    }

    // Dành cho UI Image (trong Canvas)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glowOverlay != null) glowOverlay.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glowOverlay != null) glowOverlay.SetActive(false);
    }

    // Dành cho 2D Sprite (ngoài phòng)
    private void OnMouseEnter()
    {
        if (glowOverlay != null) glowOverlay.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (glowOverlay != null) glowOverlay.SetActive(false);
    }
}