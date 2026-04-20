using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool showMilliseconds = false;

    private void Update()
    {
        if (timerText == null) return;
        if (RunTimer.Instance == null) return;
        timerText.text = RunTimer.Instance.GetFormatted(showMilliseconds);
    }
}
