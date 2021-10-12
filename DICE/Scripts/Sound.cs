using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sound : MonoBehaviour {
    public GameObject obj;
    AudioSource audio_Source;

    void Start() {
        audio_Source = obj.GetComponent<AudioSource>();
    }

    public void Sound_Control(Toggle toggle) {
        if (toggle.isOn) {
            audio_Source.mute = false;
        }
        else if (!toggle.isOn) {
            audio_Source.mute = true;
        }
    }
}
