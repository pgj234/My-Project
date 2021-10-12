using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private UIManager UI;
    private BoardManager BoardScrpit;
    
    public static int level = 0;
    public static int hitpoint = 3;

    public static GameManager instance = null;

    public static void Level_Setting (int lv) {
        level = lv;
    }

    public static void Player_HP (int hp) {
        hitpoint = hp;
    }

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        BoardScrpit = GetComponent<BoardManager>();
        initGame();
    }

    void initGame() {
        BoardScrpit.SetupScene();
    }

    public void GameOver() {
        enabled = false;
    }
}
