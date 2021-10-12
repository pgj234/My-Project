using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCtl : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.CompareTag("Player") && PlayerCtrl.playerBullet_ing == false) {
            Destroy(this.gameObject);
        }
    }
}
