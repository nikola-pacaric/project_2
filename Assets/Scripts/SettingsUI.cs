using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        InitSlider(masterSlider, AudioChannel.Master);
        InitSlider(musicSlider, AudioChannel.Music);
        InitSlider(sfxSlider,   AudioChannel.Sfx);
    }

    private void InitSlider(Slider slider, AudioChannel channel)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(channel));
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(v => AudioManager.Instance.SetVolume(channel, v));
    }
}
