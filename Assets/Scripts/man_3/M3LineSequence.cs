using System.Collections;
using UnityEngine;

// Chuỗi lời thoại có độ trễ (VD: lính gác gõ cửa sau khi con mắt chảy máu).
public class M3LineSequence : MonoBehaviour
{
    [System.Serializable]
    public class Line
    {
        public float delay = 1f;     // chờ bao lâu trước câu này
        public string speaker = "Elias";
        [TextArea] public string text;
        public bool shakeCamera;     // rung nhẹ (tiếng đập cửa...)
    }

    public Line[] lines;
    public AudioSource sfx;          // tùy chọn: tiếng gõ cửa

    private bool played;

    public void Play()
    {
        if (played) return;
        played = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (lines == null) yield break;
        foreach (Line l in lines)
        {
            yield return new WaitForSeconds(l.delay);
            if (l.shakeCamera)
            {
                if (sfx != null) sfx.Play();
                StartCoroutine(M3Fx.Shake(M3Fx.MainCameraTransform(), 0.35f, 0.08f));
            }
            M3Dialogue.Say(l.speaker, l.text);
        }
    }
}
