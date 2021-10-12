using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MoveCtl : MonoBehaviourPunCallbacks, IPunObservable {

    public float moveSpeed = 15.0f;
    public float dashSpeed = 80.0f;
    public float dashTime = 0.2f;
    public float dashCoolTime = 5.0f;

    GameObject chat_Status;
    Transform tr;
    Rigidbody2D rigidbody2d;
    Text dash_Text;
    SkillCtl skillCtl;

    public bool Invincible { get { return invincible; } }
    bool invincible = false;
    public bool Dash_Status { get { return dash; } }
    bool dash = false;
    public bool ViewCtlStp { set { viewCtlStp = value; } }
    bool viewCtlStp = false;
    public bool MoveCtlStp { set { moveCtlStp = value; } }
    bool moveCtlStp = false;
    float currentDashTime;
    float currentDashCool;

    void Start() {

        chat_Status = GameObject.Find("Chat_Layout").transform.Find("Chat_Input").gameObject;
        rigidbody2d = GetComponent<Rigidbody2D>();
        tr = GetComponent<Transform>();
        skillCtl = GetComponent<SkillCtl>();
        dash_Text = GameObject.Find("Dash_Text").GetComponent<Text>();

        dash_Text.text = "대쉬 : 준비완료";
        currentDashCool = 0.0f;
        currentDashTime = dashTime;

        if (photonView.IsMine) {
            StartCoroutine(ViewCtl());
            StartCoroutine(Move());
        }
        else {
            rigidbody2d.isKinematic = true;
            StartCoroutine(SyncedMoveAndDir());
        }
    }

    //캐릭터 방향
    IEnumerator ViewCtl() {
        while (true) {
            if (chat_Status.activeSelf == false && viewCtlStp == false) {                             //채팅창이 꺼져있을 경우 && ViewCtlStp이 false일 경우
                yield return new WaitForFixedUpdate();

                Vector3 aimPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float x = aimPos.x - this.transform.position.x;
                float y = aimPos.y - this.transform.position.y;

                float rotateAngle = Mathf.Atan2(x, y) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0f, 0f, -rotateAngle);
            }
            else {
                yield return new WaitForFixedUpdate();
            }
        }
    }

    //캐릭터 이동 관련
    IEnumerator Move() {
        while (true) {
            
            //대쉬
            if (chat_Status.activeSelf == false && Input.GetKey(KeyCode.LeftShift) && dash == false && currentDashCool <= 0.0f && skillCtl.R_Skill_ReadyTime == false) {
                                                                   //대쉬 (채팅창이 꺼져있을 경우 && 좌쉬프트를 누를경우 && 대쉬중이 아닐경우 && 대쉬 쿨타임이 아닐 경우 && R 스킬 준비중이 아닐 떄)
                dash = true;
                currentDashCool = dashCoolTime;
                StartCoroutine(DashCool());

                invincible = true;                                                                 //무적 on

                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                StartCoroutine(Dash(horizontal, vertical));
            }

            //기본 이동
            if (chat_Status.activeSelf == false && dash == false && moveCtlStp == false) {                             //채팅창이 꺼져있을 경우 && 대쉬중이 아닐경우 && moveCtlStp이 false 일 때
                yield return new WaitForFixedUpdate();

                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                Vector2 move = new Vector2(horizontal, vertical);

                Vector2 position = rigidbody2d.position;

                position = position + move * moveSpeed * Time.deltaTime;

                rigidbody2d.MovePosition(position);
            }
            else {
                yield return new WaitForFixedUpdate();
            }            
        }
    }

    //대쉬 기능
    IEnumerator Dash(float h, float v) {
        viewCtlStp = true;

        while (currentDashTime > 0.0f) {
            yield return new WaitForFixedUpdate();

            Vector2 move = new Vector2(h, v);

            Vector2 position = rigidbody2d.position;

            position = position + move * dashSpeed * Time.deltaTime;
            tr.transform.Rotate(new Vector3(0f, 0f, 30f), Space.Self);                  //덤블링? 모션 ㅋㅋ

            rigidbody2d.MovePosition(position);

            currentDashTime -= Time.deltaTime;
        }

        invincible = false;                                                                 //무적 off

        currentDashTime = dashTime;
        dash = false;
        viewCtlStp = false;
    }

    //대쉬 쿨타임 돌리기
    IEnumerator DashCool() {
        while (currentDashCool > 0.0f) {
            yield return new WaitForFixedUpdate();

            currentDashCool -= Time.deltaTime;
            dash_Text.text = "대쉬 : " + (int)currentDashCool;
        }

        dash_Text.text = "대쉬 : 준비완료";
    }

    Vector3 currPos;
    Quaternion currDir;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
        else {
            currPos = (Vector3)stream.ReceiveNext();
            currDir = (Quaternion)stream.ReceiveNext();
        }
    }

    IEnumerator SyncedMoveAndDir() {
        while (true) {
            yield return new WaitForFixedUpdate();

            if ((tr.position - currPos).sqrMagnitude >= 10.0f * 10.0f) {
                tr.position = currPos;
                tr.rotation = currDir;
            }
            else {
                tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * moveSpeed);
                tr.rotation = Quaternion.Slerp(tr.rotation, currDir, Time.deltaTime * 50.0f);
            }
        }
    }
}
