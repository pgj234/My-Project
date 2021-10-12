using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillViewCtl : MonoBehaviour {

    public string Hitter { get { return hitter; } set { hitter = value; } }
    string hitter = null;
    string killText;


    void Start() {
        killText = GetComponent<Text>().text;
    }

    void Update() {
        if (hitter != null) {
            CreateKillText(hitter);
            hitter = null;
        }
    }

    void CreateKillText(string h) {
        killText += h + "님에게 사망\n";
        //Invoke("TextTime", 3.0f);
    }

    void TextTime() {

    }
}
