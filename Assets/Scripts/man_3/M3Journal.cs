using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Sổ tay của Elias + dòng "Mục tiêu" ở góc trái màn hình.
// - AddEntry("..."): ghi manh mối vào sổ tay (manh mối nằm rải rác ở cả 3 phòng)
// - MarkDone("flag"): đánh dấu tiến độ; mục tiêu hiển thị = mục đầu tiên chưa xong (và đã đủ điều kiện hiện)
public class M3Journal : MonoBehaviour
{
    [System.Serializable]
    public class Objective
    {
        public string doneFlag;       // xong khi flag này được MarkDone
        public string requiresFlag;   // chỉ hiện khi flag này đã xong (để trống = luôn hiện)
        [TextArea] public string text;
    }

    public static M3Journal Instance { get; private set; }

    [Header("Giao diện")]
    public TMP_Text objectiveText;
    public GameObject journalPanel;
    public TMP_Text entriesText;
    public GameObject newBadge;          // chấm báo có ghi chép mới

    [Header("Mục tiêu theo thứ tự")]
    public List<Objective> objectives = new List<Objective>();
    [TextArea] public string allDoneText = "...";

    private readonly List<string> entries = new List<string>();
    private readonly HashSet<string> flags = new HashSet<string>();
    private Coroutine pulse;

    private void Awake()
    {
        Instance = this;
        if (journalPanel != null) journalPanel.SetActive(false);
        if (newBadge != null) newBadge.SetActive(false);
        RefreshEntries();
        RefreshObjective(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool IsDone(string flag) => flags.Contains(flag);

    public void MarkDone(string flag)
    {
        if (string.IsNullOrEmpty(flag) || !flags.Add(flag)) return;
        RefreshObjective(true);
    }

    public void AddEntry(string entry)
    {
        if (string.IsNullOrEmpty(entry) || entries.Contains(entry)) return;
        entries.Add(entry);
        RefreshEntries();
        if (newBadge != null && (journalPanel == null || !journalPanel.activeSelf)) newBadge.SetActive(true);
    }

    public void ToggleJournal()
    {
        if (journalPanel == null) return;
        journalPanel.SetActive(!journalPanel.activeSelf);
        if (journalPanel.activeSelf && newBadge != null) newBadge.SetActive(false);
    }

    public void CloseJournal()
    {
        if (journalPanel != null) journalPanel.SetActive(false);
    }

    private void RefreshEntries()
    {
        if (entriesText == null) return;
        entriesText.text = entries.Count == 0
            ? "<i>Chưa có ghi chép nào.</i>"
            : "• " + string.Join("\n\n• ", entries);
    }

    private void RefreshObjective(bool animate)
    {
        if (objectiveText == null) return;

        string text = allDoneText;
        foreach (Objective o in objectives)
        {
            if (flags.Contains(o.doneFlag)) continue;
            if (!string.IsNullOrEmpty(o.requiresFlag) && !flags.Contains(o.requiresFlag)) continue;
            text = o.text;
            break;
        }
        objectiveText.text = "<b>Mục tiêu:</b> " + text;

        if (animate && isActiveAndEnabled)
        {
            if (pulse != null) StopCoroutine(pulse);
            pulse = StartCoroutine(Pulse());
        }
    }

    private IEnumerator Pulse()
    {
        Transform t = objectiveText.transform;
        Color baseColor = objectiveText.color;
        for (float e = 0f; e < 1.2f; e += Time.deltaTime)
        {
            float k = Mathf.Sin(e / 1.2f * Mathf.PI);
            t.localScale = Vector3.one * (1f + 0.06f * k);
            objectiveText.color = Color.Lerp(baseColor, new Color(1f, 0.85f, 0.4f), k);
            yield return null;
        }
        t.localScale = Vector3.one;
        objectiveText.color = baseColor;
    }
}
