using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCtl : MonoBehaviour {

    public int SetWidth = 16;       //가로 스크린 비율
    public int SetHeight = 9;       //세로 스크린 비율

    void Awake() {
        Screen.SetResolution(Screen.width, Screen.width * SetWidth / SetHeight, true);
    }
}
