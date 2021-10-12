using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks {

    private string gameVersion = "1.0";
    
    public byte maxPlayer = 4;

    void Awake() {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start() {
        PhotonNetwork.GameVersion = this.gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        Debug.Log("커넥트 투 마스터");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("방이 없어 입장 실패");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayer });
    }

    public override void OnJoinedRoom() {
        Debug.Log("방 조인");
        CreatePlayer();
    }

    void CreatePlayer() {
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();

        int idx = Random.Range(1, points.Length);
        
        PhotonNetwork.Instantiate("PlayerCharacter", points[idx].position, Quaternion.identity);
    }
}
