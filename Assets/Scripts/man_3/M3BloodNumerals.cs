using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Phase 2 – Đưa ảnh gia đình cho con mắt: mắt co giật, chảy máu,
// từng giọt rơi xuống sàn tạo thành 5 ký hiệu số La Mã.
public class M3BloodNumerals : MonoBehaviour
{
    public M3Eye eye;
    public TMP_Text[] numerals;           // 5 ký hiệu, theo thứ tự trái -> phải
    public SpriteRenderer dropTemplate;   // giọt máu mẫu (ẩn), được nhân bản cho mỗi ký hiệu

    public float twitchSeconds = 1.5f;
    public float dropFallTime = 0.5f;
    public float numeralFadeTime = 0.6f;
    public float delayBetween = 0.6f;

    [TextArea] public string startLine;
    [TextArea] public string endLine;
    public UnityEvent onRevealed = new UnityEvent();

    private bool revealed;

    private void Awake()
    {
        if (numerals == null) return;
        foreach (TMP_Text n in numerals)
        {
            if (n != null) n.alpha = 0f;
        }
        if (dropTemplate != null) dropTemplate.gameObject.SetActive(false);
    }

    public void Reveal()
    {
        if (revealed) return;
        revealed = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        M3Dialogue.Say(startLine);
        if (eye != null) eye.Twitch(twitchSeconds);
        yield return new WaitForSeconds(twitchSeconds);

        foreach (TMP_Text numeral in numerals)
        {
            if (numeral == null) continue;

            if (dropTemplate != null && eye != null)
            {
                yield return FallDrop(eye.transform.position, numeral.transform.position);
            }

            for (float t = 0f; t < numeralFadeTime; t += Time.deltaTime)
            {
                numeral.alpha = t / numeralFadeTime;
                yield return null;
            }
            numeral.alpha = 1f;
            yield return new WaitForSeconds(delayBetween);
        }

        if (eye != null) eye.CloseForever();
        yield return new WaitForSeconds(1f);

        M3Dialogue.Say(endLine);
        onRevealed?.Invoke();
    }

    private IEnumerator FallDrop(Vector3 from, Vector3 to)
    {
        SpriteRenderer drop = Instantiate(dropTemplate, dropTemplate.transform.parent);
        drop.gameObject.SetActive(true);
        Transform tr = drop.transform;

        for (float t = 0f; t < dropFallTime; t += Time.deltaTime)
        {
            float k = t / dropFallTime;
            // rơi theo đường cong nhẹ, nhanh dần như trọng lực
            Vector3 p = Vector3.Lerp(from, to, k * k);
            p.x = Mathf.Lerp(from.x, to.x, k);
            tr.position = p;
            yield return null;
        }
        Destroy(drop.gameObject);
    }
}
