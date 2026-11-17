using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Hiệu ứng không khí: nhấp nháy ánh sáng / độ mờ (đèn đỏ, khói tẩu thuốc, ngọn lửa).
public class M3Flicker : MonoBehaviour
{
    public Light2D light2D;
    public SpriteRenderer sprite;
    public Graphic graphic;          // chữ / ảnh UI (VD: dòng "Bấm để đóng" nhấp nháy)

    public float minValue = 0.6f;
    public float maxValue = 1f;
    public float speed = 3f;
    public bool bobUp = false;      // khói bay lên lắc lư nhẹ
    public float bobAmount = 0.08f;

    private Vector3 basePos;
    private float seed;

    private void Awake()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (graphic == null) graphic = GetComponent<Graphic>();
        basePos = transform.localPosition;
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        float v = Mathf.Lerp(minValue, maxValue, n);

        if (light2D != null) light2D.intensity = v;
        if (sprite != null)
        {
            Color c = sprite.color;
            c.a = v;
            sprite.color = c;
        }
        if (graphic != null)
        {
            Color g = graphic.color;
            g.a = v;
            graphic.color = g;
        }
        if (bobUp)
        {
            transform.localPosition = basePos + new Vector3(Mathf.Sin(Time.time * 1.7f + seed) * bobAmount, n * bobAmount * 2f, 0f);
        }
    }
}
