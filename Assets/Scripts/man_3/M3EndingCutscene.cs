using System.Collections;
using UnityEngine;

// Phase 4 – Đốt Hiệp ước Thanh trừng và cảnh kết màn 3:
// lửa xanh lục -> phòng rung, kim đồng hồ quay ngược, vết nứt lan -> cửa bật tung, em trai bước vào rút súng
// -> bóng đen ngoài cửa sổ nổ súng -> em trai gục xuống -> máu thành con bọ cạp chui vào khe nứt
// -> căn phòng sụp đổ, màn hình tối sầm -> sang Màn 4.
public class M3EndingCutscene : MonoBehaviour
{
    [Header("Điều khiển chung")]
    public RoomManager roomManager;     // để chuyển về góc nhìn chính giữa
    public M3ScreenFader fader;
    public Transform cameraToShake;     // để trống = Camera.main

    [Header("Ngọn lửa xanh (trên tẩu thuốc)")]
    public GameObject greenFire;

    [Header("Căn phòng biến dạng")]
    public M3Clock clock;
    public M3Grow[] cracksToSpread;

    [Header("Em trai")]
    public GameObject doorClosed;
    public GameObject doorOpen;
    public SpriteRenderer brother;
    public GameObject brotherGun;
    public Vector3 brotherFallOffset = new Vector3(1.8f, -3.3f, 0f); // ngã đổ về phía trước, nằm trên sàn
    public float brotherFallAngle = -90f;

    [Header("Bóng đen ngoài cửa sổ")]
    public GameObject windowShadow;
    public GameObject muzzleFlash;

    [Header("Bọ cạp máu đen")]
    public SpriteRenderer bloodPool;
    public Transform scorpion;
    public Transform crackTarget;

    [Header("Âm thanh (tùy chọn)")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip rumbleClip;
    public AudioClip doorClip;
    public AudioClip gunshotClip;

    [Header("Lời thoại")]
    [TextArea] public string fireLine = "Lửa... màu xanh lục? Không phải lửa bình thường!";
    [TextArea] public string brotherLine = "Anh trai. Anh nghĩ đốt một tờ giấy là xóa được tất cả những gì ta đã gây dựng sao?";
    [TextArea] public string eliasPleaLine = "Em... dừng lại! Đây không phải con người thật của em!";
    [TextArea] public string afterShotLine = "Ai... ai vừa nổ súng? Thứ gì vừa chui vào khe nứt kia?!";
    public string brotherSpeaker = "Em trai";

    [Header("Kết thúc")]
    public bool loadNextLevel = true;

    private bool played;

    public void Play()
    {
        if (played) return;
        played = true;
        StartCoroutine(Run());
    }

    private void Sfx(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private IEnumerator Run()
    {
        Transform cam = cameraToShake != null ? cameraToShake : M3Fx.MainCameraTransform();
        if (fader != null) fader.BlockInput(true);
        if (InventoryManager.Instance != null) InventoryManager.Instance.DeselectAll();

        // 1. Hiệp ước bén lửa xanh lục
        if (greenFire != null) greenFire.SetActive(true);
        Sfx(fireClip);
        M3Dialogue.Say(fireLine);
        yield return new WaitForSeconds(2.5f);

        // 2. Chuyển về góc nhìn chính giữa, căn phòng rung chuyển dữ dội
        if (roomManager != null) roomManager.ExitDeskZoom();
        if (clock != null)
        {
            clock.reverse = true;
            clock.speedMultiplier = 30f;
        }
        if (cracksToSpread != null)
        {
            foreach (M3Grow g in cracksToSpread)
            {
                if (g != null) g.Rush(2.5f);
            }
        }
        Sfx(rumbleClip);
        yield return M3Fx.Shake(cam, 2.8f, 0.25f);

        // 3. Cửa bật tung, em trai bước vào
        if (doorClosed != null) doorClosed.SetActive(false);
        if (doorOpen != null) doorOpen.SetActive(true);
        Sfx(doorClip);
        StartCoroutine(M3Fx.Shake(cam, 0.3f, 0.3f));
        if (brother != null)
        {
            brother.gameObject.SetActive(true);
            yield return M3Fx.FadeSprite(brother, 0f, 1f, 0.6f);
        }
        M3Dialogue.Say(brotherSpeaker, brotherLine);
        yield return new WaitForSeconds(4.5f);

        // 4. Rút súng
        if (brotherGun != null) brotherGun.SetActive(true);
        M3Dialogue.Say(eliasPleaLine);
        yield return new WaitForSeconds(3f);

        // 5. Bóng đen ngoài cửa sổ nổ súng
        if (windowShadow != null) windowShadow.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        if (muzzleFlash != null) muzzleFlash.SetActive(true);
        Sfx(gunshotClip);
        StartCoroutine(M3Fx.Shake(cam, 0.15f, 0.2f));
        yield return new WaitForSeconds(0.1f);
        if (muzzleFlash != null) muzzleFlash.SetActive(false);

        // 6. Em trai gục xuống
        if (brother != null) yield return Fall(brother.transform, 0.5f);
        if (windowShadow != null) windowShadow.SetActive(false);
        yield return new WaitForSeconds(0.6f);

        // 7. Vũng máu tụ lại thành con bọ cạp, bò vào khe nứt
        if (bloodPool != null)
        {
            bloodPool.gameObject.SetActive(true);
            yield return Grow(bloodPool.transform, 1.2f);
        }
        if (scorpion != null && crackTarget != null)
        {
            scorpion.gameObject.SetActive(true);
            yield return Crawl(scorpion, crackTarget.position, 2.5f);
            scorpion.gameObject.SetActive(false);
        }
        M3Dialogue.Say(afterShotLine);
        yield return new WaitForSeconds(3f);

        // 8. Căn phòng sụp đổ, tối sầm
        Sfx(rumbleClip);
        StartCoroutine(M3Fx.Shake(cam, 3f, 0.45f));
        if (fader != null) yield return fader.FadeTo(1f, 2.5f);
        yield return new WaitForSeconds(1f);

        if (loadNextLevel && GameManager.Instance != null)
        {
            GameManager.Instance.CompleteCurrentLevel();
        }
        else
        {
            Debug.Log("[Màn 3] Hoàn thành – không có GameManager hoặc loadNextLevel = false.");
        }
    }

    private IEnumerator Fall(Transform t, float seconds)
    {
        Quaternion from = t.localRotation;
        Quaternion to = Quaternion.Euler(0f, 0f, brotherFallAngle);
        Vector3 p0 = t.localPosition;
        Vector3 p1 = p0 + brotherFallOffset;
        for (float e = 0f; e < seconds; e += Time.deltaTime)
        {
            float k = (e / seconds) * (e / seconds);
            t.localRotation = Quaternion.Slerp(from, to, k);
            t.localPosition = Vector3.Lerp(p0, p1, k);
            yield return null;
        }
        t.localRotation = to;
        t.localPosition = p1;
    }

    private IEnumerator Grow(Transform t, float seconds)
    {
        Vector3 target = t.localScale;
        for (float e = 0f; e < seconds; e += Time.deltaTime)
        {
            t.localScale = target * (e / seconds);
            yield return null;
        }
        t.localScale = target;
    }

    private IEnumerator Crawl(Transform t, Vector3 target, float seconds)
    {
        Vector3 start = t.position;
        Vector3 dir = (target - start).normalized;
        Vector3 side = new Vector3(-dir.y, dir.x, 0f);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0f, 0f, angle);

        for (float e = 0f; e < seconds; e += Time.deltaTime)
        {
            float k = e / seconds;
            // bò ngoằn ngoèo, giật cục
            t.position = Vector3.Lerp(start, target, k) + side * Mathf.Sin(e * 18f) * 0.12f;
            yield return null;
        }
        t.position = target;
    }
}
