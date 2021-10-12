using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class PlayerCtl : MonoBehaviourPunCallbacks {

    public GameObject[] bulletArray;
    public GameObject bulletObj;
    public GameObject gun01;
    public GameObject gun02;

    public float shotDelay = 0.15f;
    public int maxHP = 20;
    public int maxbulletNum = 20;
    public int currentHealth { get { return currentHP; } }
    public int currentBullet { get { return currentBulletNum; } }

    public AudioClip gunShot;
    public AudioClip gun_NoBullet;
    public AudioClip gunReload;

    SkillCtl skillCtl;
    Rigidbody2D rigidbody2d;
    AudioSource audioSource;
    Transform tr;
    GameObject chat_Status;
    GameObject killViewCtl;

    bool shotAvailable = true;
    float reloadTime = 1f;
    float currentreloadTime;
    bool reloading = false;
    int currentHP;
    int currentBulletNum;    

    void Start() {
        
        currentHP = maxHP;
        currentBulletNum = maxbulletNum;

        skillCtl = GetComponent<SkillCtl>();
        chat_Status = GameObject.Find("Chat_Layout").transform.Find("Chat_Input").gameObject;

        rigidbody2d = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        tr = GetComponent<Transform>();

        killViewCtl = GameObject.Find("KillViewContent");
        if (photonView.IsMine) {
            GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().Follow = rigidbody2d.transform;
            StartCoroutine("StartFire");
        }
    }

    void Update() {
        if (skillCtl.R_Skill_Ing == true) {
            currentBulletNum = 0;
        }
    }

    IEnumerator StartFire() {
        while (true) {
            yield return new WaitForFixedUpdate();

            if (chat_Status.activeSelf == false && skillCtl.R_Skill_Ing == false) {                                       //채팅이 비활성화 일 경우 && R스킬 시전 중이 아닐 경우
                if (Input.GetButton("Fire1") && reloading == false) {                           //마우스 클릭 중 && 재장전 중이 아닐 때
                    if (currentBulletNum > 0 && shotAvailable == true) {                           //남은 탄이 있을 시 && 발사 가능일 때
                        int actornumver = photonView.Owner.ActorNumber;
                        photonView.RPC("Fire", RpcTarget.Others, actornumver);                  //상대 클라이언트에 RPC로 나와 똑같이 탄 생성
                        Fire(actornumver);
                    }
                }

                if (Input.GetButtonDown("Fire1") && currentBulletNum <= 0 && reloading == false) {  //마우스 클릭 && 남은 탄이 없음 && 재장전 중이 아닐 시
                    SoundPlay(gun_NoBullet);
                }

                if (Input.GetButton("Fire2") && reloading == false) {                                //재장전 R키 && 재장전 중이 아닐 시
                    if (currentBulletNum != maxbulletNum) {                                     //남아있는 탄 != 최대 탄창
                        Reload();
                    }
                }
            }
        }
    }
    
    //총 발사
    [PunRPC]
    void Fire(int number) {
        for (int i = 0; i < bulletArray.Length; i++) {
            GameObject obj = Instantiate(bulletObj, bulletArray[i].transform.position, bulletArray[i].transform.rotation);          //탄 생성
            obj.GetComponent<BulletMove>().actorNumber = number;
        }

        SoundPlay(gunShot);
        currentBulletNum -= 2;                                      //남아있는 탄 -2

        shotAvailable = false;                                      //총 발사 가능 false

        Invoke("ShotCoolTime", shotDelay);                          //딜레이(0.2초) 후 ShotCoolTime() 함수 실행
    }

    void Reload() {
        reloading = true;                                   //재장전 중 true
        StartCoroutine("ReloadMotionReady");
        SoundPlay(gunReload);                               
        Invoke("ReloadComplete", reloadTime);               //재장전 중 시간이 지난 후 ReloadComplete() 함수 실행
    }

    void ReloadComplete() {
        currentBulletNum = maxbulletNum;                    //남은 총알 최대로 채우기
        reloading = false;                                  //재장전 중 false
    }

    //재장전 모션 레디
    IEnumerator ReloadMotionReady() {
        while (reloading == true) {
            yield return new WaitForFixedUpdate();
            photonView.RPC("ReloadMotion", RpcTarget.Others);
            ReloadMotion();
        }

        photonView.RPC("ReloadMotionStop", RpcTarget.Others);
        ReloadMotionStop();
    }

    //재장전 모션
    [PunRPC]
    void ReloadMotion() {
        float turnAngle = 60.0f;
        gun01.transform.Rotate(new Vector3(0f, 0f, -turnAngle));
        gun02.transform.Rotate(new Vector3(0f, 0f, +turnAngle));
    }

    //재장전 모션 Stop
    [PunRPC]
    void ReloadMotionStop() {
        gun01.transform.rotation = tr.transform.rotation;
        gun02.transform.rotation = tr.transform.rotation;
    }

    void ShotCoolTime() {
        shotAvailable = true;                               //총 발사 가능 true
    }

    //충돌(현재 총알만)
    void OnTriggerEnter2D(Collider2D col) {
        BulletMove bullet = col.gameObject.GetComponent<BulletMove>();

        //내 캐릭일 때 && '총알'태그에 맞았을 때 && 얻어야하는 스크립트가 null이 아닐 때 && 무적이 아닐 때
        if (photonView.IsMine && col.CompareTag("Bullet") && bullet != null && GetComponent<MoveCtl>().Invincible == false) {
            int actorNumber = bullet.actorNumber;
            string hitter = GetNickNameByActorNumber(actorNumber);

            photonView.RPC("HP_Sync", RpcTarget.Others, bullet.bulletDMG);
            currentHP -= bullet.bulletDMG;

            if (currentHP <= 0.0f) {
                killViewCtl.GetComponent<KillViewCtl>().Hitter = hitter;
                photonView.RPC("Death", RpcTarget.Others);

                GameObject.Find("GameManager").GetComponent<GameManager>().RespawnStart();                          //리스폰 스타트
                Destroy(tr.parent.gameObject);                                                                      //캐릭터 파괴
            }
        }
    }

    //캐릭터 데미지
    [PunRPC]
    void HP_Sync(int dmg) {
        currentHP -= dmg;
    }

    //캐릭터 파괴
    [PunRPC]
    void Death() {
        Destroy(tr.parent.gameObject);
    }

    //닉네임 가져오기
    string GetNickNameByActorNumber(int actorNumber) {
        foreach (Player player in PhotonNetwork.PlayerListOthers) {
            if (player.ActorNumber == actorNumber) {
                return player.NickName;
            }
        }
        return "Ghost";
    }

    //사운드 플레이
    void SoundPlay(AudioClip clip) {
        audioSource.PlayOneShot(clip);
    }
}
