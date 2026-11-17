using UnityEngine;

// Đồng hồ treo tường: kim giây nhảy từng nhịp "tích tắc"; trong cảnh kết sẽ quay ngược thật nhanh.
public class M3Clock : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;

    [Header("Quay ngược (bật trong cảnh kết)")]
    public bool reverse = false;
    public float speedMultiplier = 1f;

    public AudioSource tickSource; // tùy chọn: tiếng tích tắc

    private float tickTimer;

    private void Update()
    {
        if (!reverse && speedMultiplier <= 1f)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                Step(6f);
                if (tickSource != null) tickSource.Play();
            }
        }
        else
        {
            float dir = reverse ? -1f : 1f;
            Step(dir * 6f * speedMultiplier * Time.deltaTime * 10f);
        }
    }

    // degrees = số độ kim giây quay (chiều kim đồng hồ là dương)
    private void Step(float degrees)
    {
        if (secondHand != null) secondHand.Rotate(0f, 0f, -degrees);
        if (minuteHand != null) minuteHand.Rotate(0f, 0f, -degrees / 60f);
        if (hourHand != null) hourHand.Rotate(0f, 0f, -degrees / 720f);
    }
}
