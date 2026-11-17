using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NurseCoatPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class StitchCluster
    {
        public string clusterName;          // VD: "So_1", "So_9", "So_1_B", "So_6"
        public List<Button> holeSequence;   // Thứ tự các lỗ cần bấm
        [HideInInspector] public int currentStep = 0;
        [HideInInspector] public bool isCompleted = false;
    }

    [Header("1. Danh sách 4 cụm lỗ (2 - 7 - 2 - 8 lỗ)")]
    public List<StitchCluster> clusters = new List<StitchCluster>();

    [Header("2. Vật phẩm yêu cầu & Hiệu ứng sáng từng nút")]
    public string requiredItemID = "kim_chi"; // Kiểm tra khớp với ID trong Inventory
    public Color glowColor = new Color(1f, 0.9f, 0.35f, 1f); // Màu vàng sáng gợi ý
    public float glowSize = 52f;
    public float glowPulseSpeed = 5f;

    [Header("3. Cài đặt sợi chỉ đỏ bao phủ lỗ đạn")]
    public RectTransform threadContainer;   // Kéo ThreadLines_Container vào đây
    public Sprite threadSprite;
    public Color threadColor = new Color(0.68f, 0.06f, 0.06f, 1f);
    public float threadThickness = 18f;

    [Header("4. Sự kiện hoàn thành 1916")]
    public UnityEvent onPuzzleSolved;

    private bool isPuzzleCompleted = false;
    private Dictionary<Button, Image> holeGlowMap = new Dictionary<Button, Image>();

    private void Start()
    {
        for (int cIndex = 0; cIndex < clusters.Count; cIndex++)
        {
            int clusterIdx = cIndex;
            StitchCluster cluster = clusters[clusterIdx];

            for (int hIndex = 0; hIndex < cluster.holeSequence.Count; hIndex++)
            {
                Button holeBtn = cluster.holeSequence[hIndex];
                if (holeBtn == null) continue;

                // Đăng ký nút và tạo/lấy đốm sáng Glow (tránh trùng lặp nếu 1 lỗ đi qua 2 lần)
                if (!holeGlowMap.ContainsKey(holeBtn))
                {
                    Image glowImg = GetOrCreateGlowImage(holeBtn);
                    glowImg.gameObject.SetActive(false);
                    holeGlowMap.Add(holeBtn, glowImg);

                    holeBtn.onClick.AddListener(() => OnHoleClicked(clusterIdx, holeBtn));
                }
            }
        }
    }

    private Image GetOrCreateGlowImage(Button holeBtn)
    {
        // Đảm bảo nền nút chính trong suốt nhưng vẫn nhận click
        Image btnBg = holeBtn.GetComponent<Image>();
        if (btnBg != null)
        {
            Color bgCol = btnBg.color;
            bgCol.a = 0f;
            btnBg.color = bgCol;
            btnBg.raycastTarget = true;
        }

        // Tìm object con tên "Glow", nếu chưa có thì tự động tạo mới
        Transform existingGlow = holeBtn.transform.Find("Glow");
        if (existingGlow != null && existingGlow.TryGetComponent<Image>(out Image existingImg))
        {
            existingImg.raycastTarget = false;
            return existingImg;
        }

        GameObject glowObj = new GameObject("Glow");
        RectTransform rect = glowObj.AddComponent<RectTransform>();
        rect.SetParent(holeBtn.transform, false);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(glowSize, glowSize);

        Image img = glowObj.AddComponent<Image>();
        img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        img.color = glowColor;
        img.raycastTarget = false; // Không chặn cú click chuột vào Button cha
        return img;
    }

    private void Update()
    {
        // Tắt toàn bộ đốm sáng trước khi kiểm tra trạng thái hiện tại
        foreach (var kvp in holeGlowMap)
        {
            if (kvp.Value != null && kvp.Value.gameObject.activeSelf)
            {
                kvp.Value.gameObject.SetActive(false);
            }
        }

        if (isPuzzleCompleted) return;

        // Chỉ bật sáng khi người chơi đang chọn đúng item kim chỉ trong hòm đồ
        bool isHoldingNeedle = InventoryManager.Instance != null
            && InventoryManager.Instance.selectedItemID == requiredItemID;

        if (!isHoldingNeedle) return;

        // Tìm cụm chưa hoàn thành đầu tiên và chỉ làm sáng ĐÚNG 1 NÚT tiếp theo cần bấm
        for (int i = 0; i < clusters.Count; i++)
        {
            StitchCluster cluster = clusters[i];
            if (!cluster.isCompleted && cluster.currentStep < cluster.holeSequence.Count)
            {
                Button nextTargetHole = cluster.holeSequence[cluster.currentStep];
                if (nextTargetHole != null && holeGlowMap.TryGetValue(nextTargetHole, out Image targetGlow))
                {
                    targetGlow.gameObject.SetActive(true);

                    // Hiệu ứng nhấp nháy nhẹ thu hút sự chú ý
                    Color c = glowColor;
                    c.a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * glowPulseSpeed));
                    targetGlow.color = c;
                }
                break; // Dừng vòng lặp để tại 1 thời điểm chỉ có duy nhất 1 nút sáng lên
            }
        }
    }

    private void OnHoleClicked(int clusterIndex, Button clickedHole)
    {
        if (isPuzzleCompleted) return;

        // Chưa chọn kim chỉ trong túi -> bấm bình thường không có hiệu ứng gì
        if (InventoryManager.Instance == null || InventoryManager.Instance.selectedItemID != requiredItemID)
        {
            return;
        }

        StitchCluster cluster = clusters[clusterIndex];
        if (cluster.isCompleted) return;

        // Chỉ nhận click khi bấm đúng vào nút đang phát sáng
        Button expectedHole = cluster.holeSequence[cluster.currentStep];
        if (clickedHole != expectedHole) return;

        // Nối chỉ đỏ từ lỗ trước sang lỗ vừa bấm
        if (cluster.currentStep > 0)
        {
            RectTransform prevHole = cluster.holeSequence[cluster.currentStep - 1].GetComponent<RectTransform>();
            RectTransform currHole = clickedHole.GetComponent<RectTransform>();
            DrawRedThreadSegment(prevHole, currHole);
        }

        cluster.currentStep++;

        if (cluster.currentStep >= cluster.holeSequence.Count)
        {
            cluster.isCompleted = true;
            CheckAllClustersCompleted();
        }
    }

    private void DrawRedThreadSegment(RectTransform holeA, RectTransform holeB)
    {
        if (threadContainer == null) return;

        GameObject stitchObj = new GameObject("RedThread_Segment");
        RectTransform rect = stitchObj.AddComponent<RectTransform>();
        rect.SetParent(threadContainer, false);

        Image img = stitchObj.AddComponent<Image>();
        img.sprite = threadSprite;
        img.color = threadColor;
        img.raycastTarget = false;

        Vector2 posA = threadContainer.InverseTransformPoint(holeA.position);
        Vector2 posB = threadContainer.InverseTransformPoint(holeB.position);

        Vector2 direction = posB - posA;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Kéo dư 2 đầu sợi chỉ để che phủ trọn vẹn lỗ đạn
        float capOverlap = threadThickness * 0.45f;
        Vector2 startPos = posA - direction.normalized * capOverlap;

        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = startPos;
        rect.sizeDelta = new Vector2(distance + capOverlap * 2f, threadThickness);
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void CheckAllClustersCompleted()
    {
        foreach (var cluster in clusters)
        {
            if (!cluster.isCompleted) return;
        }

        isPuzzleCompleted = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.DeselectAll();
        }

        onPuzzleSolved?.Invoke();
    }
}