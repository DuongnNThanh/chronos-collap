using System.Collections;
using UnityEngine;

// Các hiệu ứng nhỏ dùng chung trong màn 3.
public static class M3Fx
{
    // Rung một Transform (thường là camera) rồi trả về vị trí cũ.
    public static IEnumerator Shake(Transform target, float duration, float magnitude)
    {
        if (target == null) yield break;

        Vector3 origin = target.localPosition;
        float t = 0f;
        while (t < duration)
        {
            float fade = 1f - (t / duration) * 0.5f;
            Vector2 offset = Random.insideUnitCircle * magnitude * fade;
            target.localPosition = origin + new Vector3(offset.x, offset.y, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        target.localPosition = origin;
    }

    public static IEnumerator FadeSprite(SpriteRenderer sr, float from, float to, float duration)
    {
        if (sr == null) yield break;

        Color c = sr.color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, t / duration);
            sr.color = c;
            yield return null;
        }
        c.a = to;
        sr.color = c;
    }

    public static Transform MainCameraTransform()
    {
        return Camera.main != null ? Camera.main.transform : null;
    }
}
