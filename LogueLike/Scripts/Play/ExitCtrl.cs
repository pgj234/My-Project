using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitCtrl : MonoBehaviour {

    public PlayerCtrl PlayerCtrl;
    public BoardManager BoardScrpit;

    private GameObject[] star;

    void Update() {
        star = GameObject.FindGameObjectsWithTag("Star");
    }

    void OnTriggerStay2D(Collider2D coll) {
        if (coll.CompareTag("Player") && PlayerCtrl.playerBullet_ing == false && star.Length == 0) {
            GameManager.level++;
            SceneManager.LoadScene("Play");
            PlayerCtrl.coolTime = 0;
        }
    }
}
