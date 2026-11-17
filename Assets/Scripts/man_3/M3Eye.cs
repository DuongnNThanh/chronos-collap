using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Con mắt thịt sau bức chân dung: chớp liên tục, con ngươi dõi theo chuột (dõi theo Elias),
// co giật khi bị kích động và nhắm lại vĩnh viễn sau khi tiết lộ mật mã.
public class M3Eye : MonoBehaviour
{
    public Transform eyeBody;    // phần sẽ bị "ép dẹt" khi chớp
    public Transform pupil;
    public float pupilRange = 0.25f;

    public Vector2 blinkInterval = new Vector2(1.2f, 3.5f);
    public float blinkTime = 0.12f;

    private Vector3 bodyScale;
    private Vector3 pupilOrigin;
    private bool closed;
    private bool twitching;

    private void Awake()
    {
        if (eyeBody == null) eyeBody = transform;
        bodyScale = eyeBody.localScale;
        if (pupil != null) pupilOrigin = pupil.localPosition;
    }

    private void OnEnable()
    {
        if (!closed) StartCoroutine(BlinkLoop());
    }

    private void Update()
    {
        if (closed || pupil == null || Mouse.current == null || Camera.main == null) return;

        Vector3 mouse = Mouse.current.position.ReadValue();
        mouse.z = -Camera.main.transform.position.z;
        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        Vector3 local = transform.InverseTransformPoint(world) - pupilOrigin;
        local.z = 0f;
        Vector3 jitter = twitching ? (Vector3)(Random.insideUnitCircle * 0.08f) : Vector3.zero;
        pupil.localPosition = pupilOrigin + Vector3.ClampMagnitude(local, pupilRange) + jitter;
    }

    private IEnumerator BlinkLoop()
    {
        while (!closed)
        {
            yield return new WaitForSeconds(Random.Range(blinkInterval.x, blinkInterval.y));
            if (closed) yield break;
            yield return Squash(0.08f, blinkTime);
            yield return Squash(1f, blinkTime);
        }
    }

    private IEnumerator Squash(float targetY, float time)
    {
        float startY = eyeBody.localScale.y / bodyScale.y;
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            SetOpen(Mathf.Lerp(startY, targetY, t / time));
            yield return null;
        }
        SetOpen(targetY);
    }

    private void SetOpen(float k)
    {
        eyeBody.localScale = new Vector3(bodyScale.x, bodyScale.y * k, bodyScale.z);
    }

    // Co giật dữ dội trong vài giây (khi đưa ảnh gia đình lại gần)
    public void Twitch(float seconds)
    {
        if (isActiveAndEnabled) StartCoroutine(TwitchRoutine(seconds));
    }

    private IEnumerator TwitchRoutine(float seconds)
    {
        twitching = true;
        Vector3 origin = transform.localPosition;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            transform.localPosition = origin + (Vector3)(Random.insideUnitCircle * 0.06f);
            yield return null;
        }
        transform.localPosition = origin;
        twitching = false;
    }

    public void CloseForever()
    {
        closed = true;
        StopAllCoroutines();
        if (isActiveAndEnabled) StartCoroutine(Squash(0.05f, 0.8f));
        else SetOpen(0.05f);
    }
}
