using System.Collections;
using UnityEngine;

// Màn đen toàn màn hình: mờ dần vào đầu màn, tối sầm ở cuối màn, và chặn click trong cảnh kết.
[RequireComponent(typeof(CanvasGroup))]
public class M3ScreenFader : MonoBehaviour
{
    public bool fadeInOnStart = true;
    public float fadeInSeconds = 1.5f;

    private CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (fadeInOnStart)
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }
        else
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    private IEnumerator Start()
    {
        if (!fadeInOnStart) yield break;
        yield return FadeTo(0f, fadeInSeconds);
        group.blocksRaycasts = false;
    }

    // Chặn/không chặn mọi thao tác của người chơi (dùng trong cảnh cắt)
    public void BlockInput(bool block)
    {
        group.blocksRaycasts = block;
    }

    public IEnumerator FadeTo(float alpha, float seconds)
    {
        float from = group.alpha;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, alpha, t / seconds);
            yield return null;
        }
        group.alpha = alpha;
    }
}
