using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossCtrl : MonoBehaviour {
    public static int HP = 3;
    private int ranP = 0;
    private bool dir_Right = true;
    public float speed = 1f;

    private Vector3 PL;
    private Vector3 PlayerPosition;
    public GameObject Enemy;
    static public bool EnemyKill = false;

    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.CompareTag("Player")) {
            GameManager.hitpoint--;
            SceneManager.LoadScene("Play");
            HP = 3;
            speed = 1f;
            ranP = 0;
        }
    }

    void Start () {
        StartCoroutine(random());
        Time.timeScale = 1;
    }

    void Update () {
        //플레이어 위치 추적
        PL = GameObject.FindGameObjectWithTag("Player").transform.position;

        //별이 없을 때
        if (GameObject.FindGameObjectWithTag("Star") == null) {
            HP--;

            speed = speed + 0.15f;
        }

        if (HP <= 0) {
            BossDie();
        }

        if (ranP == 0) {
            Move();
        }

        if (ranP == 1) {
            P1_Move(PlayerPosition);
        }
    }

    //보스 기본이동
    void Move() {
        if (PL.y > this.transform.position.y) {
            this.transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
        if (PL.y < this.transform.position.y) {
            this.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (PL.x > this.transform.position.x) {
            if (dir_Right == false) {
                ReverseBoss();
            }

            this.transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
            dir_Right = true;
        }
        if (PL.x < this.transform.position.x) {
            if (dir_Right == true) {
                ReverseBoss();
            }

            this.transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
            dir_Right = false;
        }
    }

    //랜덤 패턴
    IEnumerator random() {
        while (true) {
            yield return new WaitForSeconds(3f);

            ranP = (int)Random.Range(1, 3);

            //패턴 시작
            switch (ranP) {
                case 1:
                    StartCoroutine(P1());
                    break;
                case 2:
                    P2();
                    break;
            }

            yield return new WaitForSeconds(7f);

            //패턴 종료 후 처리
            switch (ranP) {
                case 1:
                    speed = speed - 4.3f;
                    break;
                case 2:
                    while (GameObject.Find("Enemy(Clone)")) {
                        yield return new WaitForFixedUpdate();
                        EnemyKill = true;
                    }
                    EnemyKill = false;
                    break;
            }
        }
    }

    //돌진 (패턴1)
    IEnumerator P1() {
        speed = speed + 4.3f;

        while (ranP == 1) {
            PlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

            yield return new WaitForSeconds(2.5f);
        }
    }

    //돌진
    void P1_Move(Vector3 position) {
        if (position.y > this.transform.position.y) {
            this.transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
        if (position.y < this.transform.position.y) {
            this.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (position.x > this.transform.position.x) {
            if (dir_Right == false) {
                ReverseBoss();
            }

            this.transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
            dir_Right = true;
        }
        if (position.x < this.transform.position.x) {
            if (dir_Right == true) {
                ReverseBoss();
            }

            this.transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
            dir_Right = false;
        }
    }

    //쫄 소환 (패턴2)
    void P2() {
        for (int i=0; i<8; i++) {
            Instantiate(Enemy, this.transform.position, Quaternion.identity);
        }

        StartCoroutine(P2_Move());
    }

    //쫄 소환 후 보스 기본 이동
    IEnumerator P2_Move() {
        yield return new WaitForSeconds(2);

        while (ranP == 2) {
            yield return new WaitForFixedUpdate();

            Move();
        }
    }

    //보스가 죽음
    void BossDie() {
        Destroy(this.gameObject);
        SceneManager.LoadScene("Clear");
        HP = 3;
        speed = 1f;
        ranP = 0;
        enabled = false;
    }

    //보스 좌우 방향 전환
    void ReverseBoss() {
        Vector3 bossScale = transform.localScale;
        bossScale.x = bossScale.x * (-1);
        transform.localScale = bossScale;
    }
}
