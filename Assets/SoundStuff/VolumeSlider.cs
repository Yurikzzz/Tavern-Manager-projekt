using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    void Start()
    {
        Slider mySlider = GetComponent<Slider>();

        if (MusicPlayer.instance != null)
        {
            mySlider.onValueChanged.AddListener(MusicPlayer.instance.SetVolume);
        }
    }
}