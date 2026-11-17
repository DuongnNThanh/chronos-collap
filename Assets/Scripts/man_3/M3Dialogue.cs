using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Hộp thoại phụ đề ở cuối màn hình: lời thoại của Elias / nhân vật khác, kèm chân dung người nói.
// Gọi từ bất kỳ đâu: M3Dialogue.Say("...") hoặc M3Dialogue.Say("Em trai", "...")
// Bấm vào hộp thoại (hoặc Space/Enter): lần 1 hiện hết chữ ngay, lần 2 đóng hộp thoại.
public class M3Dialogue : MonoBehaviour
{
    public static M3Dialogue Instance { get; private set; }

    [Header("Giao diện (panel phải là object CON, không phải chính object này)")]
    public GameObject panel;
    public TMP_Text speakerText;
    public TMP_Text lineText;
    public GameObject continueHint;   // "▼ Bấm để đóng" – hiện khi chữ đã chạy xong

    [System.Serializable]
    public class SpeakerPortrait
    {
        public string speaker;   // trùng tên truyền vào Say(speaker, ...)
        public Sprite portrait;
    }

    [Header("Chân dung người đang nói")]
    public Image portraitImage;
    public SpeakerPortrait[] portraits;
    public Sprite unknownPortrait;   // dùng khi không tìm thấy tên

    [Header("Tốc độ hiện chữ")]
    public float secondsPerChar = 0.025f;
    public float minHoldSeconds = 3f;
    public float holdSecondsPerChar = 0.05f;

    private Coroutine routine;
    private bool typing;
    private bool skipTyping;

    public bool IsShowing => panel != null && panel.activeSelf;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Say(string line)
    {
        Say("Elias", line);
    }

    public static void Say(string speaker, string line)
    {
        if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim())) return;

        if (Instance != null) Instance.Show(speaker, line);
        else Debug.Log(speaker + ": " + line);
    }

    public void Show(string speaker, string line)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(speaker, line));
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        typing = false;
        if (panel != null) panel.SetActive(false);
    }

    // Bấm vào hộp thoại: đang chạy chữ -> hiện hết; đã hiện hết -> đóng
    public void Advance()
    {
        if (!IsShowing) return;
        if (typing) skipTyping = true;
        else Hide();
    }

    private void Update()
    {
        if (!IsShowing || Keyboard.current == null) return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Advance();
        }
    }

    private Sprite FindPortrait(string speaker)
    {
        if (portraits != null)
        {
            foreach (SpeakerPortrait p in portraits)
            {
                if (p != null && p.speaker == speaker) return p.portrait;
            }
        }
        return unknownPortrait;
    }

    private IEnumerator ShowRoutine(string speaker, string line)
    {
        if (panel != null) panel.SetActive(true);
        if (continueHint != null) continueHint.SetActive(false);

        if (speakerText != null)
        {
            speakerText.text = speaker;
            speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }
        if (portraitImage != null)
        {
            Sprite s = FindPortrait(speaker);
            portraitImage.sprite = s;
            portraitImage.enabled = s != null;
        }

        lineText.text = line;
        lineText.ForceMeshUpdate();
        int total = lineText.textInfo.characterCount;
        lineText.maxVisibleCharacters = 0;

        typing = true;
        skipTyping = false;
        for (int i = 1; i <= total && !skipTyping; i++)
        {
            lineText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(secondsPerChar);
        }
        lineText.maxVisibleCharacters = int.MaxValue;
        typing = false;
        if (continueHint != null) continueHint.SetActive(true);

        yield return new WaitForSeconds(Mathf.Max(minHoldSeconds, line.Length * holdSecondsPerChar));

        if (panel != null) panel.SetActive(false);
        routine = null;
    }
}
