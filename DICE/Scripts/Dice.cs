using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Dice : MonoBehaviourPunCallbacks {

    GameManager gameManager;
    Button btn;

    [HideInInspector]
    public bool select;

    [HideInInspector]
    public int dice_Num = 0;

    public Sprite[] dark = new Sprite[6];
    public Sprite[] white = new Sprite[6];

    Image dice_Img;
    RawImage diceSelect_Img;

    GameObject table;
    int dice_Spawn_Num;

    public PhotonView PV;

    void Awake() {
        dice_Img = GetComponent<Image>();
        diceSelect_Img = transform.GetChild(0).gameObject.GetComponent<RawImage>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        btn = GetComponent<Button>();

        select = false;         //선택 초기화
        int ran = Random.Range(1, 7);       // 1부터 6까지

        dice_Num = ran;

        if (gameManager.myTurn == true) {
            if (gameManager.player_1 == true) {
                Dice_black_Imging(ran);
            }
            else {
                Dice_white_Imging(ran);
            }
        }

        if (gameManager.myTurn == true && (dice_Num == 1 || dice_Num == 5)) {
            Button_Activate(true);
        }
    }

    void Start() {
        table = transform.parent.parent.gameObject;
        dice_Spawn_Num = 0;

        for (int i = 0; i < 21; i++) {
            if (Equals(table.transform.GetChild(i).gameObject, transform.parent.gameObject)) {
                dice_Spawn_Num = i;
            }
        }
    }

    public void Button_Activate(bool value) {                 //버튼 활성화
        btn.GetComponent<Button>().enabled = value;
    }

    public void Dice_black_Imging(int ran) {        //검은 주사위
        switch (ran) {
            case 1:
                dice_Img.sprite = dark[0];
                break;
            case 2:
                dice_Img.sprite = dark[1];
                break;
            case 3:
                dice_Img.sprite = dark[2];
                break;
            case 4:
                dice_Img.sprite = dark[3];
                break;
            case 5:
                dice_Img.sprite = dark[4];
                break;
            case 6:
                dice_Img.sprite = dark[5];
                break;
        }
    }

    public void Dice_white_Imging(int ran) {        //흰색 주사위
        switch (ran) {
            case 1:
                dice_Img.sprite = white[0];
                break;
            case 2:
                dice_Img.sprite = white[1];
                break;
            case 3:
                dice_Img.sprite = white[2];
                break;
            case 4:
                dice_Img.sprite = white[3];
                break;
            case 5:
                dice_Img.sprite = white[4];
                break;
            case 6:
                dice_Img.sprite = white[5];
                break;
        }
    }

    public void Select() {                          //주사위 클릭
        if (select == false) {
            select = true;
            diceSelect_Img.enabled = true;
            gameManager.DiceNum_Receive(dice_Num, this.gameObject);

            PV.RPC("Click_Chk_Img", RpcTarget.Others, true, dice_Spawn_Num);
        }
        else {
            select = false;
            diceSelect_Img.enabled = false;
            gameManager.DiceNum_Subtract(dice_Num, this.gameObject);

            PV.RPC("Click_Chk_Img", RpcTarget.Others, false, dice_Spawn_Num);
        }
    }

    [PunRPC]
    void Click_Chk_Img(bool chk, int parent_num) {                                                                  //상대방에게도 내 주사위 클릭 상황 보여주기
        table.transform.GetChild(parent_num).GetChild(0).GetChild(0).GetComponent<RawImage>().enabled = chk;
    }
}
