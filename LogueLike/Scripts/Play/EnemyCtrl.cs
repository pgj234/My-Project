using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyCtrl : MonoBehaviour {
    private float speed = (GameManager.level -1)*0.05f + 0.5f;
    private int randomMove_num = -1;
    bool enemyRight = true;

    void OnCollisionEnter2D(Collision2D coll) {

        //벽 충돌시 방향 전환
        /*if (coll.gameObject.CompareTag("Wall")) {
            switch (randomMove_num) {
                case 0:
                    randomMove_num = 1;
                    break;
                case 1:
                    randomMove_num = 0;
                    break;
                case 2:
                    randomMove_num = 3;
                    break;
                case 3:
                    randomMove_num = 2;
                    break;
            }
        }*/

        if (coll.gameObject.CompareTag("Player") && PlayerCtrl.playerBullet_ing == false) {
            GameManager.hitpoint--;
            SceneManager.LoadScene("Play");
        }
    }

    void Start() {
        if (GameManager.level == 20) {
            speed += 1f;
        }
        StartCoroutine(ran());
    }

    void Update() {
        if (BossCtrl.EnemyKill == true) {
            Destroy(this.gameObject);
        }

        switch (randomMove_num) {
            case 0:
                this.transform.Translate(0, speed * Time.deltaTime, 0);
                break;
            case 1:
                this.transform.Translate(0, -speed * Time.deltaTime, 0);
                break;
            case 2:
                if (enemyRight == true) {
                    ReverseEnemy();
                }

                enemyRight = false;
                this.transform.Translate(-speed * Time.deltaTime, 0, 0);
                break;
            case 3:
                if (enemyRight == false) {
                    ReverseEnemy();
                }

                enemyRight = true;
                this.transform.Translate(speed * Time.deltaTime, 0, 0);
                break;
        }
    }

    //랜덤 방향 변화 타이밍
    IEnumerator ran() {
        while (true) {
            int secRan = Random.Range(15, 21);
            yield return new WaitForSeconds(secRan/10);

            randomMove_num = Random.Range(0, 4);
        }
    }

    //좌우 반전
    void ReverseEnemy() {
        Vector3 enemyScale = transform.localScale;
        enemyScale.x = enemyScale.x * (-1);
        transform.localScale = enemyScale;
    }
}
