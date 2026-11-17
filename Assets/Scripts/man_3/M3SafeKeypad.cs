using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Phase 3 – Két sắt mật mã 5 chữ số.
// Mã 28614 (28/6/1914 – ngày vụ ám sát ở dòng thời gian gốc), giải qua 3 bước:
//   1. Máu viết V · I · IV · II · III  = số hiệu các con mắt trên bản đồ (không phải chữ số)
//   2. Tấm biển "viết ngược từ cái kết" -> đọc ngược: III, II, IV, I, V
//   3. Đếm vạch chữ thập dưới từng con mắt trên bản đồ: III=2, II=8, IV=6, I=1, V=4 -> 28614
// Bẫy: đọc xuôi cho ra 41682.
public class M3SafeKeypad : MonoBehaviour
{
    [Header("Mật mã")]
    public string correctCode = "28614";

    [Header("Giao diện")]
    public GameObject panelRoot;      // toàn bộ panel két (nền tối + cửa sổ)
    public GameObject keypadView;     // màn nhập mã
    public GameObject contentsView;   // bên trong két (hồ sơ) sau khi mở
    public TMP_Text display;
    public Button[] digitButtons = new Button[10]; // phần tử i = phím số i
    public Button clearButton;
    public Button enterButton;

    [Header("Phản hồi")]
    public Color normalColor = new Color(0.85f, 0.8f, 0.65f);
    public Color wrongColor = new Color(0.9f, 0.15f, 0.15f);
    public Color rightColor = new Color(0.3f, 0.9f, 0.4f);
    [TextArea] public string wrongLine = "Sai rồi. Ổ khóa không nhúc nhích.";
    [Tooltip("Gợi ý tăng dần: cứ mỗi wrongsBeforeHint lần sai lại mở thêm 1 gợi ý")]
    [TextArea] public string[] hintLines =
    {
        "Năm ký hiệu máu không phải chữ số... Trong phòng này còn thứ gì được đánh số La Mã nữa?",
        "Những con mắt trên bản đồ... soi bằng kính lúp xem mỗi con có bao nhiêu dấu chữ thập?",
        "Tấm biển đồng: \"viết ngược từ cái kết\". Có lẽ mình đã đọc máu sai chiều.",
        "Đọc ký hiệu máu từ phải sang trái, rồi đếm dấu chữ thập dưới con mắt mang số đó trên bản đồ."
    };
    public int wrongsBeforeHint = 2;
    [TextArea] public string openLine = "2-8-6-1-4... 28 tháng 6 năm 1914. Ngày Thái tử lẽ ra đã chết. Cửa két mở ra...";

    public UnityEvent onUnlocked = new UnityEvent();

    private string input = "";
    private bool unlocked;
    private bool busy;
    private int wrongCount;

    public bool IsUnlocked => unlocked;

    private void Awake()
    {
        for (int i = 0; i < digitButtons.Length; i++)
        {
            int digit = i;
            if (digitButtons[i] != null) digitButtons[i].onClick.AddListener(() => PressDigit(digit));
        }
        if (clearButton != null) clearButton.onClick.AddListener(Clear);
        if (enterButton != null) enterButton.onClick.AddListener(Submit);
        RefreshDisplay();
    }

    public void Open()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        if (keypadView != null) keypadView.SetActive(!unlocked);
        if (contentsView != null) contentsView.SetActive(unlocked);
        if (!unlocked) Clear();
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Update()
    {
        // Hỗ trợ gõ phím số trên bàn phím khi đang mở màn nhập mã
        if (unlocked || busy || Keyboard.current == null) return;
        if (keypadView != null && !keypadView.activeInHierarchy) return;

        for (int d = 0; d <= 9; d++)
        {
            // Trong enum Key: Digit1..Digit9 liền nhau, Digit0 đứng sau Digit9
            Key rowKey = d == 0 ? Key.Digit0 : Key.Digit1 + (d - 1);
            if (Keyboard.current[rowKey].wasPressedThisFrame || Keyboard.current[Key.Numpad0 + d].wasPressedThisFrame)
            {
                PressDigit(d);
            }
        }
        if (Keyboard.current.backspaceKey.wasPressedThisFrame) Clear();
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) Submit();
        if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
    }

    public void PressDigit(int digit)
    {
        if (unlocked || busy || input.Length >= correctCode.Length) return;
        input += digit.ToString();
        RefreshDisplay();
    }

    public void Clear()
    {
        if (busy) return;
        input = "";
        RefreshDisplay();
    }

    public void Submit()
    {
        if (unlocked || busy) return;
        StartCoroutine(input == correctCode ? Success() : Wrong());
    }

    private void RefreshDisplay()
    {
        if (display == null) return;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < correctCode.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(i < input.Length ? input[i] : '_');
        }
        display.text = sb.ToString();
        display.color = normalColor;
    }

    private IEnumerator Wrong()
    {
        busy = true;
        wrongCount++;
        display.color = wrongColor;
        string line = wrongLine;
        if (hintLines != null && hintLines.Length > 0 && wrongsBeforeHint > 0 && wrongCount % wrongsBeforeHint == 0)
        {
            int hintIndex = Mathf.Min(wrongCount / wrongsBeforeHint - 1, hintLines.Length - 1);
            line = hintLines[hintIndex];
        }
        M3Dialogue.Say(line);

        RectTransform rt = display.rectTransform;
        Vector2 origin = rt.anchoredPosition;
        for (float t = 0f; t < 0.4f; t += Time.deltaTime)
        {
            rt.anchoredPosition = origin + new Vector2(Mathf.Sin(t * 60f) * 12f, 0f);
            yield return null;
        }
        rt.anchoredPosition = origin;

        busy = false;
        Clear();
    }

    private IEnumerator Success()
    {
        busy = true;
        unlocked = true;
        display.color = rightColor;
        yield return new WaitForSeconds(0.9f);

        if (keypadView != null) keypadView.SetActive(false);
        if (contentsView != null) contentsView.SetActive(true);
        M3Dialogue.Say(openLine);
        busy = false;
        onUnlocked?.Invoke();
    }
}
