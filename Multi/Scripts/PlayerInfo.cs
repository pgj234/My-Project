using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInfo : MonoBehaviourPunCallbacks, IPunObservable {

    public GameObject player;
    public Text nickName_Text;
    public Text hp_Text;
    public Text bullet_Text;

    int currentHP;
    int currentBulletNum;

    Transform tr;

    void Start() {
        tr = GetComponent<Transform>();

        nickName_Text.text = photonView.Owner.NickName;

        StartCoroutine(Info());
    }

    IEnumerator Info() {
        while (true) {
            yield return new WaitForFixedUpdate();

            transform.position = player.GetComponent<Rigidbody2D>().transform.position;

            currentHP = player.GetComponent<PlayerCtl>().currentHealth;
            hp_Text.text = "HP : " + currentHP;

            if (photonView.IsMine) {
                currentBulletNum = player.GetComponent<PlayerCtl>().currentBullet;
                bullet_Text.text = "탄 : " + currentBulletNum;
            }

            if (!photonView.IsMine) {
                SyncedMove();
            }
        }
    }

    Vector3 currPos;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(tr.position);
        }
        else {
            currPos = (Vector3)stream.ReceiveNext();
        }
    }

    void SyncedMove() {
        if ((tr.position - currPos).sqrMagnitude >= 10.0f * 10.0f) {
            tr.position = currPos;
        }
        else {
            tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 15.0f);
        }
    }
}
