using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviour {
    GameObject esc_Panel;

    void Start() {
        esc_Panel = GameObject.Find("ESC_Panel");
        esc_Panel.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && esc_Panel.activeSelf == false) {
            esc_Panel.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && esc_Panel.activeSelf == true) {
            esc_Panel.SetActive(false);
        }
    }

    public void RespawnStart() {
        StartCoroutine(Respawn());
    }

    //리스폰
    IEnumerator Respawn() {
        Text text = GameObject.Find("Respawn_Text").GetComponent<Text>();

        for (int i = 5; i > 0; i--) {
            text.text = "리스폰 중 . . . . . . " + i;
            yield return new WaitForSeconds(1.0f);
        }

        text.text = null;

        CreatePlayer();
    }

    void CreatePlayer() {
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();

        int idx = Random.Range(1, points.Length);

        PhotonNetwork.Instantiate("PlayerCharacter", points[idx].position, Quaternion.identity);
    }
}
