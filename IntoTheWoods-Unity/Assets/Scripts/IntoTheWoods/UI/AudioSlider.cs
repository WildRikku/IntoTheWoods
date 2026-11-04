using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioSlider : MonoBehaviour {
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private TextMeshProUGUI valueText;

    public void OnChangeSlider(float Value) {
        valueText.SetText($"{Value.ToString("N2")}");
        mixer.SetFloat("Volume", Mathf.Log10(Value) * 20); // TODO : change range between -80 and +20 dB
    }
}
