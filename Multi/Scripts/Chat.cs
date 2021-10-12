using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Chat;

public class Chat : MonoBehaviourPunCallbacks {

    public Text msgList;
    public InputField ifSendMsg;

    GameObject Chat_Input;
    GameObject Chat_Output;
    GameObject Chat_Scrollbar_vertical;
    GameObject Chat_Handle;

    Image Chat_Scrollbar_img;
    Image Chat_Handle_img;
    Image Chat_Scroll_View_img;

    void Awake() {
        Chat_Input = GameObject.Find("Chat_Input");
        Chat_Output = GameObject.Find("Chat_Output");
        Chat_Scrollbar_vertical = GameObject.Find("Chat_Scrollbar_Vertical");
        Chat_Handle = GameObject.Find("Chat_Handle");
    }

    void Start() {
        PhotonNetwork.IsMessageQueueRunning = true;

        Chat_Scrollbar_img = Chat_Scrollbar_vertical.GetComponent<Image>();
        Chat_Handle_img = Chat_Handle.GetComponent<Image>();
        Chat_Scroll_View_img = GameObject.Find("Chat_Scroll_View").GetComponent<Image>();
        ifSendMsg.ActivateInputField();
        ChatOff();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (Chat_Input.activeSelf == false) {
                ChatOn();
                ifSendMsg.ActivateInputField();
            }
            else if (Chat_Input.activeSelf == true && string.IsNullOrEmpty(ifSendMsg.text) != true) {
                ifSendMsg.text = null;
                ifSendMsg.ActivateInputField();
            }
            else if (Chat_Input.activeSelf == true && string.IsNullOrEmpty(ifSendMsg.text) == true) {
                ChatOff();
            }
        }
    }

    void ChatOff() {
        Chat_Input.SetActive(false);
        Chat_Scrollbar_img.enabled = false;
        Chat_Handle_img.enabled = false;
        Chat_Scroll_View_img.enabled = false;
    }

    void ChatOn() {
        Chat_Input.SetActive(true);
        Chat_Scrollbar_img.enabled = true;
        Chat_Handle_img.enabled = true;
        Chat_Scroll_View_img.enabled = true;
    }

    public void OnSendChatMsg() {
        if (string.IsNullOrEmpty(ifSendMsg.text) != true) {
            string msg = string.Format("{0} : {1}", PhotonNetwork.LocalPlayer.NickName, ifSendMsg.text);
            photonView.RPC("ReceiveMsg", RpcTarget.OthersBuffered, msg);
            ReceiveMsg(msg);
        }
    }

    [PunRPC]
    void ReceiveMsg(string msg) {
        msgList.text += msg + "\n";
        Chat_Scrollbar_vertical.GetComponent<Scrollbar>().value = 0;
    }
}