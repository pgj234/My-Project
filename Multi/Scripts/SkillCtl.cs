using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SkillCtl : MonoBehaviourPunCallbacks {

    public GameObject[] bulletArray;
    public GameObject bulletObj;
    public GameObject gun01;
    public GameObject gun02;

    public float r_skillTime = 3.0f;
    public float r_skillCoolTime = 30.0f;
    public float r_skill_ShotDelay = 0.05f;

    public AudioClip r_Skill_ReloadReady;
    public AudioClip r_Skill_Shot01;
    public AudioClip r_Skill_Shot02;
    public AudioClip empty_shell01;
    public AudioClip empty_shell02;
    AudioClip audioClip;

    GameObject chat_Status;
    PlayerCtl playerCtl;
    MoveCtl moveCtl;
    Rigidbody2D rigidbody2d;
    Transform tr;
    AudioSource audioSource;
    Text r_Skill_Text;
    Text skill_Notice;

    float r_CurrentSkillTime;
    float r_CurrentSkillCool;
    public bool R_Skill_Ing { get { return r_Skill_ing; } }
    bool r_Skill_ing = false;
    public bool R_Skill_ReadyTime { get { return r_Skill_ReadyTime; } }
    bool r_Skill_ReadyTime = false;

    void Start() {
        r_Skill_Text = GameObject.Find("R_Skill_Text").GetComponent<Text>();
        skill_Notice = GameObject.Find("Skill_Notice_Text").GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        playerCtl = GetComponent<PlayerCtl>();
        moveCtl = GetComponent<MoveCtl>();
        chat_Status = GameObject.Find("Chat_Layout").transform.Find("Chat_Input").gameObject;

        rigidbody2d = GetComponent<Rigidbody2D>();
        tr = GetComponent<Transform>();
        
        r_CurrentSkillCool = r_skillCoolTime / 2;     //처음에만 쿨타임 반절

        if (photonView.IsMine) {
            StartCoroutine("R_Skill_Ready");
            StartCoroutine("R_SkillCoolTime");
        }
    }

    //R 스킬 준비
    IEnumerator R_Skill_Ready() {
        while (true) {
            if (chat_Status.activeSelf == false && r_CurrentSkillCool <= 0 && Input.GetKey(KeyCode.R)) {                 //R 스킬 쿨 0초 이하 && R키 누를 시
                r_CurrentSkillCool = r_skillCoolTime;
                moveCtl.MoveCtlStp = true;                                          //moveCtlStp true해서 움직일 수 없게 만듦
                moveCtl.ViewCtlStp = true;                                          //viewCtlStp true해서 방향 못틀게 만듦.
                r_Skill_ing = true;                                             //R 스킬 사용 중 true
                r_Skill_ReadyTime = true;                                       //R 스킬 준비타임 (선딜)
                StartCoroutine("DelayMotionReady");

                PlaySound(r_Skill_ReloadReady);
                yield return new WaitForSeconds(1.1f);
                StartCoroutine("R_Skill_StartFire");
                StartCoroutine("R_SkillCoolTime");
                r_Skill_ReadyTime = false;                                       //R 스킬 준비타임 끝 (선딜 끝)
            }
            else if (chat_Status.activeSelf == false && (r_CurrentSkillCool <= 0) == false && Input.GetKey(KeyCode.R)) {
                Skill_Notice("쿨타임 입니다!");
                yield return new WaitForFixedUpdate();
            }
            else {
                yield return new WaitForFixedUpdate();
            }
        }
    }

    //R 스킬 시작
    IEnumerator R_Skill_StartFire() {
        int ran;            //랜덤 저장
        float rot = 8.0f;            //각도
        int rotDirNum = 0;             //각도 방향 수치
        bool turn = true;              //rotDirNum가 특정한 양수일 경우 false를 하여 안쪽 방향으로 틀게 함, 특정한 음수일 경우 true를 하여 바깥 방향으로 틀게 함.
        audioSource.volume = 0.6f;          //연사이므로 시끄러울 수 있으므로 볼륨을 조금 줄임
        r_CurrentSkillTime = r_skillTime;   //R 스킬 지속시간 초기화

        while (r_CurrentSkillTime > 0.0f && moveCtl.Dash_Status == false) {                                        //R 스킬 현재 사용 남은시간이 0초 초과, 대쉬 중이 아닐 때 (대쉬 시 캔슬 기능)
            yield return new WaitForSeconds(r_skill_ShotDelay);

            r_CurrentSkillTime -= r_skill_ShotDelay * 1.5f;
                                                         //사운드 랜덤 재생을 위한 랜덤
            ran = (int)Random.Range(0, 2);

            switch (ran) {
                case 0:
                    audioClip = r_Skill_Shot01;
                    break;

                case 1:
                    audioClip = r_Skill_Shot02;
                    break;
            }

            if (turn == true) {                          //바깥 방향
                photonView.RPC("OUT_Rotation", RpcTarget.Others, rot);
                OUT_Rotation(rot);
                rotDirNum++;
            }
            else if (turn == false) {                    //안쪽 방향
                photonView.RPC("IN_Rotation", RpcTarget.Others, rot);
                IN_Rotation(rot);
                rotDirNum--;
            }
                                                        //각도 변경 방향 정하기
            if (rotDirNum >= 6) {
                turn = false;
            }
            else if (rotDirNum <= -6) {
                turn = true;
            }

            int actornumver = photonView.Owner.ActorNumber;
            photonView.RPC("R_Skill_Fire", RpcTarget.Others, actornumver);                  //상대 클라이언트에 RPC로 나와 똑같이 탄 생성
            R_Skill_Fire(actornumver);

            PlaySound(audioClip);
        }

        r_Skill_ing = false;                                              //R 스킬 사용 중 false
        moveCtl.MoveCtlStp = false;                                         //moveCtlStp false해서 움직일 수 있게 만듦
        moveCtl.ViewCtlStp = false;                                          //viewCtlStp false해서 방향 틀 수 있게 만듦


        //총구 원래대로
        photonView.RPC("Gun_Alignment", RpcTarget.Others);
        Gun_Alignment();

        audioSource.volume = 1.0f;                            //볼륨 정상으로
    }

    //R 스킬 안쪽 각도
    [PunRPC]
    void IN_Rotation(float rot) {
        gun01.transform.Rotate(0f, 0f, -rot);
        gun02.transform.Rotate(0f, 0f, +rot);
    }

    //R 스킬 바깥쪽 각도
    [PunRPC]
    void OUT_Rotation(float rot) {
        gun01.transform.Rotate(0f, 0f, +rot);
        gun02.transform.Rotate(0f, 0f, -rot);
    }

    //총구 원래대로
    [PunRPC]
    void Gun_Alignment() {
        gun01.transform.rotation = tr.transform.rotation;
        gun02.transform.rotation = tr.transform.rotation;
    }

    //R 스킬 발사
    [PunRPC]
    void R_Skill_Fire(int number) {
        for (int i = 0; i < bulletArray.Length; i++) {
            GameObject obj = Instantiate(bulletObj, bulletArray[i].transform.position, bulletArray[i].transform.rotation);          //탄 생성
            obj.GetComponent<BulletMove>().actorNumber = number;
        }
    }

    //R 스킬 쿨타임
    IEnumerator R_SkillCoolTime() {
        while (r_CurrentSkillCool > 0) {
            yield return new WaitForFixedUpdate();
            r_CurrentSkillCool -= Time.deltaTime;
            r_Skill_Text.text = "난사 : " + (int)r_CurrentSkillCool;
        }

        r_Skill_Text.text = "난사 : 준비완료";
    }

    //선딜 모션
    IEnumerator DelayMotionReady() {
        while (r_Skill_ReadyTime == true) {
            yield return new WaitForFixedUpdate();
            photonView.RPC("DelayMotion", RpcTarget.Others);
            DelayMotion();
        }

        //총구 원래대로
        photonView.RPC("Gun_Alignment", RpcTarget.Others);
        Gun_Alignment();
    }

    [PunRPC]
    void DelayMotion() {
        float turnAngle = 60.0f;
        gun01.transform.Rotate(new Vector3(0f, 0f, -turnAngle));
        gun02.transform.Rotate(new Vector3(0f, 0f, +turnAngle));
    }

    //스킬 알림 Text
    void Skill_Notice(string str) {
        skill_Notice.text = str;
        Invoke("Skill_Notice_Delay", 2.5f);
    }

    //스킬 알림 Text null
    void Skill_Notice_Delay() {
        skill_Notice.text = null;
    }

    //사운드 플레이
    void PlaySound(AudioClip clip) {
        audioSource.PlayOneShot(clip);
    }
}
