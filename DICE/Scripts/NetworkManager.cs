using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks {

    public GameManager gameManager;

    [Header("DisconnectPanel")]
    public GameObject startPanel;
    public InputField nickNameInput;

    [Header("LobbyPanel")]
    public GameObject lobbyPanel;
    public InputField roomInput;
    public Text welcomeText;
    public Text lobbyInfoText;
    public Button[] roomBtn;
    public Button previousBtn;
    public Button nextBtn;

    [Header("RoomPanel")]
    public GameObject playPanel;
    public Text joinUser;
    public Text[] chatText;
    public InputField chatInput;
    public Text roomName;
    public GameObject help_Img;

    [Header("ETC")]
    public Text statusText;
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    GameObject connecting;

    void Awake() {
        Screen.SetResolution(1600, 900, false);

        connecting = GameObject.Find("Connecting");
    }

    void Start() {
        connecting.SetActive(false);

        chatInput = chatInput.GetComponent<InputField>();
    }

    void Update() {
        if (playPanel.activeSelf == true) {         //게임방일 때
            if (chatInput.interactable == true) {       //채팅 ON 일때
                if (chatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) {       //엔터로 채팅
                    Send();
                    chatInput.ActivateInputField();
                }
                else if (chatInput.text == "" && Input.GetKeyDown(KeyCode.Return)) {         //채팅 OFF
                    chatInput.interactable = false;
                }
            }
            else {          //채팅 OFF 일때
                if (Input.GetKeyDown(KeyCode.Return)) {         //채팅 ON
                    chatInput.interactable = true;
                    chatInput.ActivateInputField();
                }
            }

            if (help_Img.activeSelf == false && Input.GetKeyDown(KeyCode.T)) {          //도움말 ON
                help_Img.SetActive(true);
            }
            else if (help_Img.activeSelf == true && Input.GetKeyDown(KeyCode.T)) {          //도움말 OFF
                help_Img.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {         //방 나가기
                gameManager.Leave_Process();
            }
        }
        
        statusText.text = PhotonNetwork.NetworkClientState.ToString();

        if (lobbyPanel.activeSelf == true) {                                //로비일 때
            lobbyInfoText.text = "현재 접속자 : " + PhotonNetwork.CountOfPlayers;
        }
    }

    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num) {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal() {
        // 최대페이지
        maxPage = (myList.Count % roomBtn.Length == 0) ? myList.Count / roomBtn.Length : myList.Count / roomBtn.Length + 1;

        // 이전, 다음버튼
        previousBtn.interactable = (currentPage <= 1) ? false : true;
        nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * roomBtn.Length;
        for (int i = 0; i < roomBtn.Length; i++) {
            roomBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            roomBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            roomBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++) {
            if (!roomList[i].RemovedFromList) {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region 서버연결

    public void Connect() {
        connecting.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() {
        startPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
        welcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        myList.Clear();
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause) {
        gameManager.Leave_Process();
        connecting.SetActive(false);
        lobbyPanel.SetActive(false);
        playPanel.SetActive(false);
        startPanel.SetActive(true);
    }
    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text, new RoomOptions { MaxPlayers = 2 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public override void OnJoinedRoom() {
        lobbyPanel.SetActive(false);
        playPanel.SetActive(true);
        RoomRenewal();
        gameManager.Ready_Img_Init(false);
        chatInput.text = "";
        for (int i = 0; i < chatText.Length; i++) chatText[i].text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, string message) { roomInput.text = ""; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        RoomRenewal();
        gameManager.notMaster_Nickname = newPlayer.NickName;
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal() {
        joinUser.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            joinUser.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : " VS ");
        roomName.text = PhotonNetwork.CurrentRoom.Name;
    }
    #endregion


    #region 채팅
    public void Send() {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    [PunRPC]
    void ChatRPC(string msg) {
        bool isInput = false;
        for (int i = 0; i < chatText.Length; i++)
            if (chatText[i].text == "") {
                isInput = true;
                chatText[i].text = msg;
                break;
            }
        if (!isInput) {             // 꽉차면 한칸씩 위로 올림
            for (int i = 1; i < chatText.Length; i++) chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }
    #endregion
}