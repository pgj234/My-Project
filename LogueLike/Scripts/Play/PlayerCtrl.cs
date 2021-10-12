using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCtrl : MonoBehaviour {
    private Animator ani;

    private bool playerRight = true;
    public static bool playerBullet_ing = false;
    private bool wallCrash = false;
    private bool status = false;
    private bool useCanDash = true;

    public static int coolTime = 0;
    private bool coolTime_ing = false;
    private bool coolTime_switch = false;

    private bool dir_Up = false;
    private bool dir_Down = false;
    private bool dir_Left = false;
    private bool dir_Right = true;

    public float speed = 0.1f;

    void Start () {
        ani = GetComponent<Animator>();

        StartCoroutine(bulletMove());
    }

    // 여러가지 충돌 IN
    void OnCollisionStay2D(Collision2D coll) {
        if (coll.gameObject.CompareTag("Wall")) {
            wallCrash = true;
        }
    }
     // 여러가지 충돌 OUT
    void OnCollisionExit2D(Collision2D coll) {
        if (coll.gameObject.CompareTag("Wall")) {
            wallCrash = false;
        }
    }

    void Update () {
        if (playerBullet_ing == true) {
            this.gameObject.layer = 10;
        }
        else {
            this.gameObject.layer = 0;
        }

        //불릿대쉬 쿨타임 조건
        if ((Input.GetKeyUp("space") && (
            ani.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Bullet") ||
            ani.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Bullet_UP") ||
            ani.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Bullet_DOWN"))) ||
            (coolTime == 0 && wallCrash == true && playerBullet_ing == true && useCanDash == true)) {
                coolTime = 3;
                coolTime_ing = true;
                wallCrash = false;
                playerBullet_ing = false;
                status = false;
        }

        //불릿대쉬 쿨타임 실행
        if (coolTime_ing == true && coolTime_switch == false) {
            coolTime_switch = true;
            useCanDash = false;
            StartCoroutine(coolTimer());
        }

        //캐릭터 기본이동
        if (playerBullet_ing == false) {
            status = false;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                if (!(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))) {
                    status = true;

                    dir_Up = true;
                    dir_Down = false;
                    dir_Left = false;
                    dir_Right = false;
                    
                    PlayerWalk();
                    this.transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
                }
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))) {
                    status = true;

                    dir_Up = false;
                    dir_Down = true;
                    dir_Left = false;
                    dir_Right = false;

                    PlayerWalk();
                    this.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
                }
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                if (!(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))) {
                    status = true;

                    //캐릭터 방향 전환
                    if (playerRight == true) {
                        PlayerReverse();
                    }
                    playerRight = false;

                    dir_Up = false;
                    dir_Down = false;
                    dir_Left = true;
                    dir_Right = false;

                    PlayerWalk();
                    this.transform.Translate(new Vector3(- speed * Time.deltaTime, 0, 0));
                }
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                if (!(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))) {
                    status = true;

                    //캐릭터 방향 전환
                    if (playerRight == false) {
                        PlayerReverse();
                    }
                    playerRight = true;

                    dir_Up = false;
                    dir_Down = false;
                    dir_Left = false;
                    dir_Right = true;

                    PlayerWalk();
                    this.transform.Translate(new Vector3(speed* Time.deltaTime, 0, 0));
                }
            }
        }
    }

    // 캐릭터 방향 반전
    void PlayerReverse() {
        if (playerBullet_ing == false) {
            Vector3 playerScale = transform.localScale;
            playerScale.x = playerScale.x * (-1);
            transform.localScale = playerScale;
        }
    }

    // 플레이어 걷고있다
    void PlayerWalk () {
        ani.SetTrigger("Walk");
    }

    // 플레이어 멈춰있다
    void PlayerStop() {
        ani.SetTrigger("Stop");
    }

    // 플레이어 곧 잠잔다
    void PlayerStop_SP() {
        ani.SetTrigger("Stop_SP");
    }

    // 플레이어 불릿대쉬한다 (좌우)
    void PlayerDash() {
        ani.SetTrigger("Bullet");
    }

    // 플레이어 불릿대쉬한다 (위)
    void PlayerDash_UP() {
        ani.SetTrigger("Bullet_up");
    }

    // 플레이어 불릿대쉬한다 (아래)
    void PlayerDash_DOWN() {
        ani.SetTrigger("Bullet_down");
    }

    // 캐릭터 죽음
    void PlayerDie() {
        ani.SetTrigger("Die");
    }

    // 캐릭터 클리어
    void PlayerClear() {
        ani.SetTrigger("Clear");
    }

    IEnumerator coolTimer() {
        while (coolTime != 0) {
            useCanDash = false;
            yield return new WaitForSeconds(1);

            coolTime--;

            if (coolTime == 0) {
                coolTime_ing = false;
                coolTime_switch = false;
                useCanDash = true;
                break;
            }
        }
    }

    //캐릭터 불릿대쉬 이동
    IEnumerator bulletMove() {
        while (true) {
            yield return new WaitForFixedUpdate();

            if (useCanDash == true && Input.GetKey(KeyCode.Space) && dir_Up == true && wallCrash == false) {
                playerBullet_ing = true;
                PlayerDash_UP();
                this.transform.Translate(new Vector3(0, speed * Time.deltaTime * 3, 0));
            }
            else if (useCanDash == true && Input.GetKey(KeyCode.Space) && dir_Down == true && wallCrash == false) {
                playerBullet_ing = true;
                PlayerDash_DOWN();
                this.transform.Translate(new Vector3(0, -speed * Time.deltaTime * 3, 0));
            }
            else if (useCanDash == true && Input.GetKey(KeyCode.Space) && dir_Left == true && wallCrash == false) {
                playerBullet_ing = true;
                PlayerDash();
                this.transform.Translate(new Vector3(-speed * Time.deltaTime * 3, 0, 0));
            }
            else if (useCanDash == true && Input.GetKey(KeyCode.Space) && dir_Right == true && wallCrash == false) {
                playerBullet_ing = true;
                PlayerDash();
                this.transform.Translate(new Vector3(speed * Time.deltaTime * 3, 0, 0));
            }

            // 캐릭터 멈춤
            if (status == false && playerBullet_ing == false) {
                PlayerStop();
                PlayerStop_SP();
            }
        }
    }
}
