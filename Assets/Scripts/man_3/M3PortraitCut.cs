using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Phase 1 – Bức chân dung sống: dùng dao mổ rạch tranh -> lộ con mắt và két sắt phía sau.
public class M3PortraitCut : MonoBehaviour
{
    public GameObject portraitIntact;
    public GameObject portraitTorn;
    public Transform slashLine;          // vệt rạch, kéo dài dần theo trục X
    public float slashTime = 0.35f;

    [Header("Hiện ra sau khi rạch (con mắt, két sắt...)")]
    public GameObject[] revealAfterCut;
    public M3Grow[] startOozing;         // máu đen bắt đầu rỉ từ khe sàn

    [TextArea] public string afterCutLine;
    public AudioSource sfx;              // tùy chọn: tiếng xé vải
    public UnityEvent onCut = new UnityEvent();

    private bool done;

    public void Play()
    {
        if (done) return;
        done = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (sfx != null) sfx.Play();

        if (slashLine != null)
        {
            Vector3 full = slashLine.localScale;
            slashLine.gameObject.SetActive(true);
            for (float t = 0f; t < slashTime; t += Time.deltaTime)
            {
                slashLine.localScale = new Vector3(full.x * (t / slashTime), full.y, full.z);
                yield return null;
            }
            slashLine.localScale = full;
            yield return new WaitForSeconds(0.15f);
            slashLine.gameObject.SetActive(false);
        }

        if (portraitIntact != null) portraitIntact.SetActive(false);
        if (portraitTorn != null) portraitTorn.SetActive(true);
        if (revealAfterCut != null)
        {
            foreach (GameObject go in revealAfterCut)
            {
                if (go != null) go.SetActive(true);
            }
        }

        yield return M3Fx.Shake(M3Fx.MainCameraTransform(), 0.3f, 0.12f);

        if (startOozing != null)
        {
            foreach (M3Grow g in startOozing)
            {
                if (g != null) g.Begin();
            }
        }

        M3Dialogue.Say(afterCutLine);
        onCut?.Invoke();
    }
}
