using UnityEngine;

// Lớn dần theo thời gian: máu đen rỉ ra từ khe nứt, vết nứt lan rộng...
// Dùng Update (không dùng coroutine) để khi người chơi chuyển phòng rồi quay lại, hiệu ứng vẫn chạy tiếp.
public class M3Grow : MonoBehaviour
{
    public Vector3 startScale = new Vector3(0.05f, 0.05f, 1f);
    public Vector3 endScale = Vector3.one;
    public float duration = 12f;
    public bool beginOnStart = false;

    private bool growing;
    private bool initialized;
    private float elapsed;
    private float currentDuration;
    private Vector3 from;

    private void Awake()
    {
        if (!initialized)
        {
            transform.localScale = startScale;
            initialized = true;
        }
    }

    private void Start()
    {
        if (beginOnStart) Begin();
    }

    public void Begin()
    {
        if (growing || elapsed > 0f) return;
        StartGrow(duration);
    }

    // Lan nhanh tới kích thước cuối (dùng trong cảnh kết)
    public void Rush(float seconds)
    {
        StartGrow(seconds);
    }

    private void StartGrow(float seconds)
    {
        initialized = true;
        from = transform.localScale;
        currentDuration = Mathf.Max(0.01f, seconds);
        elapsed = 0.0001f;
        growing = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!growing) return;

        elapsed += Time.deltaTime;
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / currentDuration));
        transform.localScale = Vector3.Lerp(from, endScale, k);
        if (elapsed >= currentDuration) growing = false;
    }
}
