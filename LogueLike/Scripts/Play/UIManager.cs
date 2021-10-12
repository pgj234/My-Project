using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Text Life;
    public Text Stage;
    public Text Skill;
    public Text Boss;

    void Awake() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.SetResolution(1600, 900, true);
    }

    void Update() {
        Life.text = "Life : " + GameManager.hitpoint;
        Stage.text = "스테이지 : " + GameManager.level;

        if (GameManager.level == 20) {
            Boss.text = "보스 HP : " + BossCtrl.HP;
        }

        if (PlayerCtrl.coolTime != 0) {
            Skill.text = "스킬 : " + PlayerCtrl.coolTime;
        }
        else {
            Skill.text = "스킬 : ON";
        }

    }

    public void StartClick() {
        GameManager.Level_Setting(1);
        GameManager.Player_HP(3);
        SceneManager.LoadScene("Play");
    }

    public void goMain() {
        SceneManager.LoadScene("Main");
    }

    public void QuitClick() {
        Application.Quit();
    }

    public void stage20() {
        GameManager.Level_Setting(20);
        GameManager.Player_HP(99);
        SceneManager.LoadScene("Play");
    }
}
