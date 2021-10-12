using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerNameSetting : MonoBehaviour {

    const string playerNamePrefKey = "닉네임 입력";

    public InputField inputField;

    void Start() {

        inputField.ActivateInputField();

        string defaultName = string.Empty;
        InputField _inputField = this.GetComponent<InputField>();

        if (_inputField != null) {
            if (PlayerPrefs.HasKey(playerNamePrefKey)) {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName(string value) {

        if (string.IsNullOrEmpty(value)) {
            return;
        }

        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
