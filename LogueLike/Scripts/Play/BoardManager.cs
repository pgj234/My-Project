using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour {

    public Transform tr;
    public GameObject White_Tile;
    public GameObject Black_Tile;
    public GameObject Star;
    public GameObject Enemy;
    public GameObject Boss;
    public GameObject Wall;
    public GameObject Wall_in;
    public GameObject Exit;
    public GameObject Player;
    private static int row = 3 + (int)Mathf.Log(GameManager.level, 1.7f);
    private int col = row;
    private float gap = 0.6f;
    private List<Vector3> gridPos = new List<Vector3>();

    private GameObject[] star;
    
    public GameManager GM;

    void Start () {
        //캐릭터 죽음
        if (GameManager.hitpoint <= 0) {
            Destroy(Player);
            SceneManager.LoadScene("End");
            GM.GameOver();
        }

        // 땅 크기 단계별 재조정
        row = 3 + (int)Mathf.Log(GameManager.level, 1.7f);
        col = row;

        //땅 생성
        if (GameManager.level < 20) {
            for (int i = 0; i < col; i++) {
                for (int j = 0; j < row; j++) {
                    if ((i + j) % 2 == 0) {
                        Instantiate(White_Tile, tr.position + new Vector3(i * gap, j * gap, 0), Quaternion.identity);
                    }
                    else {
                        Instantiate(Black_Tile, tr.position + new Vector3(i * gap, j * gap, 0), Quaternion.identity);
                    }
                }
            }
        }
        else if (GameManager.level == 20) {
            for (int i = 0; i < col+2; i++) {
                for (int j = 0; j < row+2; j++) {
                    if ((i + j) % 2 == 0) {
                        Instantiate(White_Tile, tr.position + new Vector3(i * gap, j * gap, 0), Quaternion.identity);
                    }
                    else {
                        Instantiate(Black_Tile, tr.position + new Vector3(i * gap, j * gap, 0), Quaternion.identity);
                    }
                }
            }
        }
        

        //테두리 생성
        if (GameManager.level < 20) {
            for (int i = 0; i < col + 2; i++) {
                Instantiate(Wall, tr.position + new Vector3(-gap, i * gap - gap, 0), Quaternion.identity);
                Instantiate(Wall, tr.position + new Vector3(col * gap, i * gap - gap, 0), Quaternion.identity);
            }
            for (int i = 0; i < row + 2; i++) {
                Instantiate(Wall, tr.position + new Vector3(i * gap - gap, -gap, 0), Quaternion.identity);
                Instantiate(Wall, tr.position + new Vector3(i * gap - gap, row * gap, 0), Quaternion.identity);
            }
        }
        else if (GameManager.level == 20) {
            for (int i = 0; i < col + 4; i++) {
                Instantiate(Wall, tr.position + new Vector3(-gap, i * gap - gap, 0), Quaternion.identity);
                Instantiate(Wall, tr.position + new Vector3(col * gap + gap * 2, i * gap - gap, 0), Quaternion.identity);
            }
            for (int i = 0; i < row + 4; i++) {
                Instantiate(Wall, tr.position + new Vector3(i * gap - gap, -gap, 0), Quaternion.identity);
                Instantiate(Wall, tr.position + new Vector3(i * gap - gap, row * gap + gap * 2, 0), Quaternion.identity);
            }
        }

        //Exit 생성
        if (GameManager.level < 20) {
            Instantiate(Exit, tr.position + new Vector3(gap * row - gap, gap * col - gap, 0), Quaternion.identity);
        }

        // 좌표 초기화
        gridPos.Clear();

        coor();

        //오브젝트 생성
        if (GameManager.level == 1) {

            // 1스테이지는 오브젝트 고정 생성
            Instantiate(Star, tr.position + new Vector3(1.2f, 0, 0), Quaternion.identity);
            Instantiate(Star, tr.position + new Vector3(0, 1.2f, 0), Quaternion.identity);
            Instantiate(Wall_in, tr.position + new Vector3(0.6f, 0.6f, 0), Quaternion.identity);
        }
        // 2~19스테이지 오브젝트 생성
        else if (GameManager.level > 1 && GameManager.level < 20) {
            int wallMake = (int)Mathf.Log(GameManager.level, 1.7f) + 1;
            int starMake = (int)Mathf.Log(GameManager.level, 1.42f);
            int enemyMake = (int)Mathf.Log(GameManager.level, 1.75f);
            int sideOneEx = starMake;

            while (enemyMake > 0) {
                if (wallMake > 0) {
                    Instantiate(Wall_in, RandomPosition(), Quaternion.identity);
                    wallMake--;
                }
                else if (wallMake <= 0 && starMake > 0) {
                    if (sideOneEx == starMake) {
                        sideCoor();
                    }

                    Instantiate(Star, RandomPosition(), Quaternion.identity);
                    starMake--;
                }
                else if (starMake <= 0 && enemyMake > 0) {
                    Instantiate(Enemy, RandomPosition(), Quaternion.identity);
                    enemyMake--;
                }
            }
        }
        // 20스테이지 보스 오브젝트 생성
        else if (GameManager.level == 20) {
            StartCoroutine(BossStar());

            Instantiate(Boss, new Vector3(gap * 6, gap * 6, 0), Quaternion.identity);
        }

        //1초 후 플레이어 생성
        Invoke("PlayerCreate", 1f);
    }

    void Update() {
        star = GameObject.FindGameObjectsWithTag("Star");
    }

    // 새 좌표 생성
    void coor() {
        for (int i=1; i<row-1; i++) {
            for (int j=1; j<col-1; j++) {
                gridPos.Add(new Vector3(i*gap, j*gap, 0));
            }
        }
    }

    void sideCoor() {
        if (row >= 5) {
            for (int i = 2; i < col - 2; i++) {
                gridPos.Add(new Vector3(i * gap, 0, 0));
                gridPos.Add(new Vector3(i * gap, (row - 1) * gap, 0));
            }
            for (int i = 2; i < row - 2; i++) {
                gridPos.Add(new Vector3(0, i * gap, 0));
                gridPos.Add(new Vector3((col - 1) * gap, i * gap, 0));
            }
        }
    }

    // 중복없는 랜덤좌표 지정
    Vector3 RandomPosition() {
        int ran = (int)Random.Range(0, gridPos.Count);
        Vector3 ranPos = gridPos[ran];
        gridPos.RemoveAt(ran);
        return ranPos;
    }

    // 플레이어 생성
    void PlayerCreate () {
        Instantiate(Player, new Vector3(0, -0.12f, 0), Quaternion.identity);
    }

    // 보스전 별 생성
    IEnumerator BossStar() {
        while (true) {
            yield return new WaitForEndOfFrame();

            if (star.Length == 0) {
                gridPos.Clear();

                coor();
                int starMake = (int)Random.Range(13, 15);

                while (starMake > 0) {
                    Instantiate(Star, RandomPosition(), Quaternion.identity);
                    starMake--;
                }
            }
        }
    }

    public void SetupScene() {
        Start();
    }
}
