using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class SceneCtl : MonoBehaviour {
    public void PlayScene() {
        if (string.IsNullOrEmpty(GameObject.Find("NickNameField").GetComponent<Text>().text) == false) {
            PhotonNetwork.NickName = GameObject.Find("NickNameField").GetComponent<Text>().text;
            GameObject.Find("InputField").SetActive(false);
            GameObject.Find("Play_Button").SetActive(false);
            GameObject.Find("Quit_Button").SetActive(false);

            GameObject.Find("NoticeField").GetComponent<Text>().text = "서버에 연결중";

            SceneManager.LoadScene("Play");
        }
        else {
            GameObject.Find("NoticeField").GetComponent<Text>().text = "닉네임을 입력하세요.";
            Invoke("NoticeOff", 3.0f);
        }
    }

    void NoticeOff() {
        GameObject.Find("NoticeField").GetComponent<Text>().text = null;
    }

    public void Quit() {
        Application.Quit();
    }

    public void Back() {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Start");
    }
}
