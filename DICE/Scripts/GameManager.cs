using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {

    [HideInInspector]
    public bool myTurn = false;
    [HideInInspector]
    public bool player_1 = false;
    bool game_Ing = false;
    bool me_Ready = false;
    bool[] ready_Array = new bool[2];

    [HideInInspector]
    int target_Score = 4000;

    int turn_Num = 0;

    public GameObject playPanel;
    [HideInInspector]
    public string notMaster_Nickname;
    public Text game_Timer_Text;
    int game_Timer = 0;
    public GameObject p1_Table;
    public GameObject p2_Table;
    public GameObject p1_Score_Panel;
    public GameObject p2_Score_Panel;
    public GameObject first_Order;
    public GameObject win_TextObj;
    public GameObject myTurn_Text_Obj;
    public Text giveUp_Text;

    [Header("")]
    public GameObject Dice_Prefab;

    List<GameObject> tableChild_List = new List<GameObject>();

    [Header("P1 Score")]
    public Text p1_TotalScore_Text;
    public Text p1_RoundScore_Text;
    public Text p1_SelectScore_Text;

    [Header("P2 Score")]
    public Text p2_TotalScore_Text;
    public Text p2_RoundScore_Text;
    public Text p2_SelectScore_Text;

    //현재 선택한 주사위들 리스트
    [HideInInspector]
    public List<GameObject> selectDice_List = new List<GameObject>();

    List<int> num_List = new List<int>();

    public GameObject ggwang;

    //나의 테이블, 점수 패널, 굴릴 수 있는 주사위 배열, 세이브한 주사위 배열
    GameObject my_Table;
    GameObject my_ScorePanel;
    Text myTotal_Score_Text;
    Text myRound_Score_Text;
    Text mySelect_Score_Text;
    int myTotal_Score;
    int myRound_Score;
    int mySelect_Score;
    List<GameObject> my_Dice = new List<GameObject>();
    List<GameObject> my_SaveDice = new List<GameObject>();

    GameObject you_Table;
    Text you_TotalScore_Text;
    Text you_RoundScore_Text;
    Text you_SelectScore_Text;

    bool ggwang_Delay = false;

    [Header("")]
    public GameObject Ready_Btn;
    public GameObject me_Ready_ChkImg;
    public GameObject you_Ready_ChkImg;

    public GameObject SFX_Obj;
    AudioSource _audio;
    public AudioClip roll_Audio;
    public AudioClip win_Audio;
    public AudioClip defeat_Audio;
    public PhotonView PV;

    Vector3 p1_Table_Pos;
    Vector3 p2_Table_Pos;
    Vector3 p1_ScorePanel_Pos;
    Vector3 p2_ScorePanel_Pos;

    void Start() {
        _audio = SFX_Obj.GetComponent<AudioSource>();

        //테이블, 점수패널 위치, 회전값 저장
        p1_Table_Pos = p1_Table.transform.localPosition;
        p2_Table_Pos = p2_Table.transform.localPosition;
        p1_ScorePanel_Pos = p1_Score_Panel.transform.localPosition;
        p2_ScorePanel_Pos = p2_Score_Panel.transform.localPosition;

        PV.RPC("Ready_Init", RpcTarget.All);
    }

    public void Ready() {
        if (me_Ready == false) {        //내가 레디를 안 했던 상태였다면
            me_Ready_ChkImg.SetActive(true);
            me_Ready = true;
            PV.RPC("Ready_Img", RpcTarget.Others, true);

            if (PhotonNetwork.IsMasterClient) {
                Master_Ready_Chk(me_Ready);
            }
            else {
                PV.RPC("Ready_Chk", RpcTarget.MasterClient, me_Ready);
            }
        }
        else {                      //내가 레디를 한 상태였다면
            me_Ready = false;
            me_Ready_ChkImg.SetActive(false);
            PV.RPC("Ready_Img", RpcTarget.Others, false);

            if (PhotonNetwork.IsMasterClient) {
                Master_Ready_Chk(me_Ready);
            }
            else {
                PV.RPC("Ready_Chk", RpcTarget.MasterClient, me_Ready);
            }
        }
    }

    //유저가 나갈 때 처리
    public void Leave_Process() {
        if (game_Ing == true) {
            PV.RPC("Win", RpcTarget.Others, "상대가 포기했습니다");
        }
        PV.RPC("Init", RpcTarget.All);
        PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
        PV.RPC("Dice_Destroy", RpcTarget.All, "Save_Dice");
        PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");

        playPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void Init() {                                         //전체 초기화
        game_Ing = false;

        turn_Num = 0;
        myTotal_Score = 0;
        myRound_Score = 0;
        mySelect_Score = 0;

        p1_TotalScore_Text.text = "0";
        p1_RoundScore_Text.text = "0";
        p1_SelectScore_Text.text = "0";
        p2_TotalScore_Text.text = "0";
        p2_RoundScore_Text.text = "0";
        p2_SelectScore_Text.text = "0";

        notMaster_Nickname = "";
        myTurn = false;
        player_1 = false;
        game_Timer = 0;
        game_Timer_Text.text = "";
        ggwang.SetActive(false);
        myTurn_Text_Obj.SetActive(false);
        Ready_Btn.SetActive(true);
        me_Ready_ChkImg.SetActive(false);
        you_Ready_ChkImg.SetActive(false);
        my_Table = null;
        you_Table = null;
        my_ScorePanel = null;

        Table_Init();

        Ready_Init();
        Ready_Img(false);
        Text_Off();
    }

    void Table_Init() {                                     //테이블 위치, 회전값 초기화
        p1_Table.transform.localPosition = p1_Table_Pos;
        p2_Table.transform.localPosition = p2_Table_Pos;
        p1_Table.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        p2_Table.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
        p1_Score_Panel.transform.localPosition = p1_ScorePanel_Pos;
        p2_Score_Panel.transform.localPosition = p2_ScorePanel_Pos;

    }

    [PunRPC]
    void Master_Ready_Chk(bool value) {
        ready_Array[0] = value;

        if (ready_Array[0] == true && ready_Array[1] == true) {        //2명 모두 레디상태라면
            PV.RPC("Game_Start", RpcTarget.All);                       //게임시작
        }
    }

    [PunRPC]
    void Ready_Chk(bool value) {
        ready_Array[1] = value;

        if (ready_Array[0] == true && ready_Array[1] == true) {        //2명 모두 레디상태라면
            PV.RPC("Game_Start", RpcTarget.All);                       //게임시작
        }
    }

    [PunRPC]
    public void Ready_Img(bool value) {
        you_Ready_ChkImg.SetActive(value);
    }

    [PunRPC]
    void Ready_Init() {
        giveUp_Text.text = "방 나가기";
        Ready_Btn.SetActive(true);
        for (int i = 0; i < ready_Array.Length; i++) {
            ready_Array[i] = false;
        }
    }

    public void Ready_Img_Init(bool value) {
        PV.RPC("Ready_Img_Init_Process", RpcTarget.All, value);
    }

    [PunRPC]
    void Ready_Img_Init_Process(bool value) {
        me_Ready_ChkImg.SetActive(value);
    }

    [PunRPC]
    void Game_Start() {
        game_Ing = true;
        me_Ready_ChkImg.SetActive(false);
        you_Ready_ChkImg.SetActive(false);
        Ready_Btn.SetActive(false);

        Game_Init();
    }

    void Game_Init() {
        giveUp_Text.text = "포기하고 방 나가기";

        p1_Table.transform.localPosition = p1_Table_Pos;
        p2_Table.transform.localPosition = p2_Table_Pos;
        p1_Score_Panel.transform.localPosition = p1_ScorePanel_Pos;
        p2_Score_Panel.transform.localPosition = p2_ScorePanel_Pos;

        if (PhotonNetwork.IsMasterClient) {
            int firstRan = Random.Range(0, 2);

            switch (firstRan) {
                case 0:                 //방장 선
                    PV.RPC("FirstOrder_Text", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
                    FirstOrder_Set();
                    PV.RPC("P2_Table_Set", RpcTarget.Others);
                    break;
                case 1:                 //방원 선
                    PV.RPC("FirstOrder_Text", RpcTarget.All, notMaster_Nickname);
                    PV.RPC("FirstOrder_Set", RpcTarget.Others);
                    P2_Table_Set();
                    break;
            }
        }
    }

    [PunRPC]
    void FirstOrder_Text(string text) {
        first_Order.SetActive(true);
        first_Order.GetComponent<Text>().text = text;
        Invoke("Text_Off", 2);
    }

    void Text_Off() {                                               //메시지 끄기
        first_Order.SetActive(false);
        win_TextObj.SetActive(false);
        ggwang.SetActive(false);
    }

    [PunRPC]
    void FirstOrder_Set() {                                                 //선공 1P
        player_1 = true;
        my_Table = p1_Table;
        you_Table = p2_Table;
        my_ScorePanel = p1_Score_Panel;

        myTotal_Score_Text = p1_TotalScore_Text;
        myRound_Score_Text = p1_RoundScore_Text;
        mySelect_Score_Text = p1_SelectScore_Text;

        you_TotalScore_Text = p2_TotalScore_Text;
        you_RoundScore_Text = p2_RoundScore_Text;
        you_SelectScore_Text = p2_SelectScore_Text;

        Score_Init();
        Turn_Start();
    }

    [PunRPC]
    void P2_Table_Set() {                                                       //후공 2P
        player_1 = false;
        p1_Table.transform.localPosition = p2_Table_Pos;
        p2_Table.transform.localPosition = p1_Table_Pos;
        p1_Table.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
        p2_Table.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        p1_Score_Panel.transform.localPosition = p2_ScorePanel_Pos;
        p2_Score_Panel.transform.localPosition = p1_ScorePanel_Pos;

        my_Table = p2_Table;
        you_Table = p1_Table;
        my_ScorePanel = p2_Score_Panel;

        myTotal_Score_Text = p2_TotalScore_Text;
        myRound_Score_Text = p2_RoundScore_Text;
        mySelect_Score_Text = p2_SelectScore_Text;

        you_TotalScore_Text = p1_TotalScore_Text;
        you_RoundScore_Text = p1_RoundScore_Text;
        you_SelectScore_Text = p1_SelectScore_Text;

        Score_Init();
    }

    void Score_Init() {                                     //점수 완전 초기화
        myTotal_Score = 0;
        myTotal_Score_Text.text = myTotal_Score.ToString();
        Round_Scoring(0);
        Select_Scoring(0);
    }

    [PunRPC]
    void Turn_Start() {                                                         //턴 시작
        myTurn = true;
        myTurn_Text_Obj.SetActive(true);        //마이턴 단축키 텍스트 ON
        selectDice_List.Clear();            //선택한 주사위 초기화
        my_Dice.Clear();

        if (turn_Num == 0) {                //처음 턴 이라면
            my_SaveDice.Clear();
            PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
            PV.RPC("Dice_Destroy", RpcTarget.All, "Save_Dice");
            PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");
        }

        mySelect_Score = 0;

        PV.RPC("Roll_Audio_Play", RpcTarget.All);    //주사위 돌리기 효과음

        Invoke("Dice_Roll", 1.4f);
    }

    void Dice_Roll() {                                                                       //주사위 굴리기
        tableChild_List.Clear();
        ggwang_Delay = false;

        for (int i = 0; i < 21; i++) {
            tableChild_List.Add(my_Table.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < (6 - my_SaveDice.Count); i++) {              //남아있는 주사위만큼 돌리기
            int ran = Random.Range(0, tableChild_List.Count);

            GameObject prefab = Instantiate(Dice_Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            int dice_Num = prefab.GetComponent<Dice>().dice_Num;

            prefab.transform.SetParent(tableChild_List[ran].transform);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
            my_Dice.Add(prefab);

            PV.RPC("Dice_Create", RpcTarget.Others, player_1, dice_Num, ran, i);
            tableChild_List.RemoveAt(ran);
        }

        if (Roll_Chk() == false) {                  //노 족보 일시
            GGWANG_Turn_Off();
            return;
        }
        Btn_On();
        Btn_ReChk_Off();
        StartCoroutine(Turn_Timer());
        StartCoroutine(Play_Hotkey());
    }

    [PunRPC]
    void Dice_Create(bool player, int num, int index, int onlyone_num) {                                //상대에게도 내 주사위 생성되는 것 보여주기

        if (onlyone_num == 0) {         //턴당 한번만

            tableChild_List.Clear();

            for (int i = 0; i < 21; i++) {
                tableChild_List.Add(you_Table.transform.GetChild(i).gameObject);
            }
        }

        GameObject prefab = Instantiate(Dice_Prefab, new Vector3(0, 0, 0), Quaternion.identity);

        if (player == true) {                                                   //상대가 1P
            prefab.GetComponent<Dice>().Dice_black_Imging(num);
        }
        else {                                                                  //상대가 2P
            prefab.GetComponent<Dice>().Dice_white_Imging(num);
        }

        prefab.transform.SetParent(tableChild_List[index].transform);
        prefab.transform.localScale = Vector3.one;
        prefab.transform.localPosition = Vector3.zero;
        tableChild_List.RemoveAt(index);
    }

    void GGWANG_Turn_Off() {                            //꽝 일 때
        ggwang_Delay = true;
        myTurn = false;
        myTurn_Text_Obj.SetActive(false);

        turn_Num = 0;
        myRound_Score = 0;
        Round_Scoring(myRound_Score);
        mySelect_Score = 0;
        Select_Scoring(mySelect_Score);
        ggwang.SetActive(true);

        PV.RPC("GGWANG_Other_Text", RpcTarget.Others);
        Invoke("Text_Off", 2f);
        Invoke("GGWANG_Delay", 2f);
    }
    
    void GGWANG_Delay() {
        PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
        PV.RPC("Dice_Destroy", RpcTarget.All, "Save_Dice");
        PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");
        ggwang_Delay = false;

        PV.RPC("Turn_Start", RpcTarget.Others);
    }

    [PunRPC]
    void GGWANG_Other_Text() {                  //상대방에게도 꽝 텍스트 보여주기
        ggwang.SetActive(true);
        Invoke("Text_Off", 2f);
    }

    bool Roll_Chk() {                               //족보 체크 (True : 족보 나옴, False : 족보 안나옴)
        num_List.Clear();

        if (my_Dice.Count > 2) {                           //굴린 주사위가 3개 이상일 경우 (2개 초과)
            DiceClickable_Chk();
        }

        for (int i = 0; i < my_Dice.Count; i++) {
            if (my_Dice[i].GetComponent<Button>().enabled == true) {
                return true;
            }
        }

        return false;                            //족보가 하나도 안나왔네 ㅈㅈ
    }

    void DiceClickable_Chk() {                                  //주사위 클릭 가능 여부
        for (int i=0; i<my_Dice.Count -2; i++) {
            for (int j=i+1; j<my_Dice.Count -1; j++) {
                for (int k=i+2; k<my_Dice.Count; k++) {
                    if (my_Dice[i].GetComponent<Dice>().dice_Num == my_Dice[j].GetComponent<Dice>().dice_Num &&
                        my_Dice[j].GetComponent<Dice>().dice_Num == my_Dice[k].GetComponent<Dice>().dice_Num) {                         //같은 숫자 3개 이상일 때

                        my_Dice[i].GetComponent<Dice>().Button_Activate(true);
                        my_Dice[j].GetComponent<Dice>().Button_Activate(true);
                        my_Dice[k].GetComponent<Dice>().Button_Activate(true);
                    }
                }
            }
        }

        if (my_Dice.Count == 5) {                               //굴린 주사위가 5개 일 때
            for (int i = 0; i < my_Dice.Count; i++) {
                num_List.Add(my_Dice[i].GetComponent<Dice>().dice_Num);
            }

            for (int i = 0; i < my_Dice.Count; i++) {
                if (num_List.Contains(1) && num_List.Contains(2) && num_List.Contains(3) && num_List.Contains(4) && num_List.Contains(5)) {                 //1, 2, 3, 4, 5 스트레이트가 나왔을 때
                    for (int j=0; j< my_Dice.Count; j++) {
                        my_Dice[j].GetComponent<Dice>().Button_Activate(true);
                    }
                }
                else if (num_List.Contains(2) && num_List.Contains(3) && num_List.Contains(4) && num_List.Contains(5) && num_List.Contains(6)) {                //2, 3, 4, 5, 6 스트레이트가 나왔을 때
                    for (int j = 0; j < my_Dice.Count; j++) {
                        my_Dice[j].GetComponent<Dice>().Button_Activate(true);
                    }
                }
            }
        }
        else if (my_Dice.Count == 6) {                               //굴린 주사위가 6개 일 때
            for (int i = 0; i < my_Dice.Count; i++) {
                num_List.Add(my_Dice[i].GetComponent<Dice>().dice_Num);
            }

            if (num_List.Contains(1) && num_List.Contains(2) && num_List.Contains(3) && num_List.Contains(4) && num_List.Contains(5) && num_List.Contains(6)) {                 //1, 2, 3, 4, 5, 6 스트레이트가 나왔을 때
                for (int j = 0; j < my_Dice.Count; j++) {
                    my_Dice[j].GetComponent<Dice>().Button_Activate(true);
                }
            }
        }

        num_List.Clear();
    }

    IEnumerator Turn_Timer() {                                          //시간제한
        game_Timer = 25;

        if (turn_Num > 0) {
            yield break;
        }

        PV.RPC("Watch_Time", RpcTarget.All, game_Timer);

        WaitForSeconds wait = new WaitForSeconds(1f);

        while (game_Timer > 0) {
            yield return wait;

            if (ggwang_Delay == false) {
                game_Timer -= 1;
            }

            if (myTurn == false) {
                yield break;
            }

            PV.RPC("Watch_Time", RpcTarget.All, game_Timer);
        }

        if (game_Timer <= 0) {                                          //시간 초과시 턴 종료하고 / 이것저것 초기화 후 / 상대에게 턴 넘김
            myTurn = false;
            myTurn_Text_Obj.SetActive(false);

            turn_Num = 0;
            myRound_Score = 0;
            mySelect_Score = 0;
            Total_Scoring();
            Round_Scoring(myRound_Score);
            Select_Scoring(mySelect_Score);
            PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
            PV.RPC("Dice_Destroy", RpcTarget.All, "Save_Dice");
            PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");

            PV.RPC("Turn_Start", RpcTarget.Others);
        }
    }

    IEnumerator Play_Hotkey() {                                         //단축키로 플레이
        bool key_Pressable = true;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (myTurn == true && key_Pressable == true) {
            yield return wait;

            if (mySelect_Score > 0) {                            //선택된 점수가 있을 때
                myTurn_Text_Obj.transform.GetChild(1).gameObject.SetActive(true);

                if (Input.GetKey(KeyCode.F)) {                     //플레이 중 F (점수를 따고 다시 굴리기)
                    key_Pressable = false;
                    ggwang_Delay = true;

                    for (int i = 0; i < selectDice_List.Count; i++) {
                        selectDice_List[i].GetComponent<Button>().enabled = false;
                    }

                    for (int i=0; i < selectDice_List.Count; i++) {
                        selectDice_List[i].tag = "Save_Dice";
                        my_SaveDice.Add(selectDice_List[i]);
                        selectDice_List[i].transform.SetParent(my_Table.transform.GetChild(my_SaveDice.Count + 20).gameObject.transform);
                        my_SaveDice[my_SaveDice.Count - 1].transform.localScale = Vector3.one;
                        my_SaveDice[my_SaveDice.Count - 1].transform.localPosition = Vector3.zero;

                        PV.RPC("SaveDice_Create", RpcTarget.Others, player_1, my_SaveDice[my_SaveDice.Count-1].GetComponent<Dice>().dice_Num, my_SaveDice.Count);
                    }
                    selectDice_List.Clear();

                    PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
                    PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");
                    Round_Scoring(mySelect_Score);
                    mySelect_Score = 0;
                    Select_Scoring(mySelect_Score);
                    turn_Num += 1;

                    if (my_SaveDice.Count == 6) {
                        turn_Num = 0;
                    }

                    Turn_Start();
                }
            }
            else {
                myTurn_Text_Obj.transform.GetChild(1).gameObject.SetActive(false);
            }

            if (mySelect_Score > 0) {
                myTurn_Text_Obj.transform.GetChild(2).gameObject.SetActive(true);

                if (Input.GetKey(KeyCode.Q)) {                     //플레이 중 Q (점수를 따고 넘기기)
                    key_Pressable = false;
                    ggwang_Delay = true;

                    for (int i = 0; i < selectDice_List.Count; i++) {
                        selectDice_List[i].GetComponent<Button>().enabled = false;
                    }

                    myTurn = false;
                    myTurn_Text_Obj.SetActive(false);

                    turn_Num = 0;
                    Total_Scoring();
                    myRound_Score = 0;
                    Round_Scoring(myRound_Score);
                    mySelect_Score = 0;
                    Select_Scoring(mySelect_Score);

                    PV.RPC("Dice_Destroy", RpcTarget.All, "Dice");
                    PV.RPC("Dice_Destroy", RpcTarget.All, "Save_Dice");
                    PV.RPC("Dice_Destroy", RpcTarget.All, "Dice_Select_Img");

                    Win_Chk();      //승리 체크
                }
            }
            else {
                myTurn_Text_Obj.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    [PunRPC]
    void SaveDice_Create(bool player, int num, int index) {                                //상대에게도 내 주사위 생성되는 것 보여주기

        GameObject prefab = Instantiate(Dice_Prefab, new Vector3(0, 0, 0), Quaternion.identity);
        prefab.tag = "Save_Dice";

        if (player == true) {                                                   //상대가 1P
            prefab.GetComponent<Dice>().Dice_black_Imging(num);
        }
        else {                                                                  //상대가 2P
            prefab.GetComponent<Dice>().Dice_white_Imging(num);
        }
        
        prefab.transform.SetParent(you_Table.transform.GetChild(index + 20).gameObject.transform);
        prefab.transform.localScale = Vector3.one;
        prefab.transform.localPosition = Vector3.zero;
    }

    [PunRPC]
    void Dice_Destroy(string tag) {                                                     //주사위 없애기
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < objects.Length; i++) {
            Destroy(objects[i]);
        }
    }

    void Win_Chk() {                                                    //승리 체크 (토탈 점수 얻을 때 마다)
        if (myTotal_Score >= target_Score) {                                        //목표 점수에 도달했을 시
            PV.RPC("Win", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        }
        else {                                                               //목표 점수에 도달 못했을 시
            PV.RPC("Turn_Start", RpcTarget.Others);                             //상대방 턴 시작
        }
    }

    [PunRPC]
    void Win(string nickname) {
        Win_Audio_Play();
        PV.RPC("Defeat_Audio_Play", RpcTarget.Others);     //승리 효과음
        Dice_Destroy("Dice");
        Dice_Destroy("Save_Dice");
        Dice_Destroy("Dice_Select_Img");
        win_TextObj.SetActive(true);
        win_TextObj.GetComponent<Text>().text = nickname;
        Invoke("Init", 2f);
    }

    void Total_Scoring() {                     //토탈 점수 갱신
        myTotal_Score += (myRound_Score + mySelect_Score);
        myTotal_Score_Text.text = myTotal_Score.ToString();
        PV.RPC("TotalScore_Texting", RpcTarget.Others, myTotal_Score);
    }

    void Round_Scoring(int score) {              //라운드 점수 갱신
        myRound_Score += score;
        myRound_Score_Text.text = myRound_Score.ToString();
        PV.RPC("RoundScore_Texting", RpcTarget.Others, myRound_Score);
    }

    void Select_Scoring(int score) {             //현재 선택된 점수 갱신
        mySelect_Score = score;
        mySelect_Score_Text.text = mySelect_Score.ToString();
        PV.RPC("SelectScore_Texting", RpcTarget.Others, mySelect_Score);
    }

    [PunRPC]
    void TotalScore_Texting(int score) {              //토탈 스코어링 텍스팅
        you_TotalScore_Text.text = score.ToString();
    }

    [PunRPC]
    void RoundScore_Texting(int score) {              //토탈 스코어링 텍스팅
        you_RoundScore_Text.text = score.ToString();
    }

    [PunRPC]
    void SelectScore_Texting(int score) {              //토탈 스코어링 텍스팅
        you_SelectScore_Text.text = score.ToString();
    }

    [PunRPC]
    void Watch_Time(int time) {                                                 //시간제한 보여주기
        game_Timer_Text.text = "Time : " + time;
    }

    [PunRPC]
    void Roll_Audio_Play() {                                                 //주사위 굴리기 효과음
        _audio.PlayOneShot(roll_Audio);
    }

    [PunRPC]
    void Win_Audio_Play() {                                                 //승리 효과음
        _audio.PlayOneShot(win_Audio);
    }

    [PunRPC]
    void Defeat_Audio_Play() {                                                 //패배 효과음
        _audio.PlayOneShot(defeat_Audio);
    }

    public void DiceNum_Receive(int dice_num, GameObject obj) {                             //굴린 주사위 숫자 받아오고 / 점수체크 / 조합 후 남은 주사위 비활성화 여부
        selectDice_List.Add(obj);
        num_List.Add(dice_num);

        Btn_Off(dice_num, obj);
        Rule_Score(dice_num);
    }

    public void DiceNum_Subtract(int dice_num, GameObject obj) {                            //굴린 주사위 숫자 빼고 / 점수체크 / 조합 후 남은 주사위 비활성화 여부
        selectDice_List.Remove(obj);
        num_List.Remove(dice_num);

        Btn_On();
        Rule_Score(dice_num);
    }

    void Btn_On() {
        List<int> num_cnt = new List<int>();
        num_cnt.Clear();

        for (int i = 0; i < my_Dice.Count; i++) {
            num_cnt.Add(my_Dice[i].GetComponent<Dice>().dice_Num);
        }

        if (my_Dice.Count > 2) {
            for (int i = 0; i < my_Dice.Count - 2; i++) {
                for (int j = i + 1; j < my_Dice.Count - 1; j++) {
                    for (int k = i + 2; k < my_Dice.Count; k++) {
                        if (my_Dice[i].GetComponent<Dice>().dice_Num == my_Dice[j].GetComponent<Dice>().dice_Num &&
                            my_Dice[j].GetComponent<Dice>().dice_Num == my_Dice[k].GetComponent<Dice>().dice_Num) {             //같은 숫자 3개 이상이면
                            my_Dice[i].GetComponent<Dice>().Button_Activate(true);                          //조건에 맞으면 다른 주사위 버튼 ON
                            my_Dice[j].GetComponent<Dice>().Button_Activate(true);
                            my_Dice[k].GetComponent<Dice>().Button_Activate(true);
                        }
                    }
                }
            }
        }

        if (num_cnt.Contains(1) && num_cnt.Contains(2) && num_cnt.Contains(3) && num_cnt.Contains(4) && num_cnt.Contains(5) && num_cnt.Contains(6)) {
            for (int i=0; i<my_Dice.Count; i++) {
                my_Dice[i].GetComponent<Dice>().Button_Activate(true);
            }
            return;
        }

        if (num_cnt.Contains(1) && num_cnt.Contains(2) && num_cnt.Contains(3) && num_cnt.Contains(4) && num_cnt.Contains(5)) {
            for (int i=0; i<my_Dice.Count; i++) {
                if (my_Dice[i].GetComponent<Dice>().dice_Num != 6) {
                    my_Dice[i].GetComponent<Dice>().Button_Activate(true);
                }
            }
        }

        if (num_cnt.Contains(2) && num_cnt.Contains(3) && num_cnt.Contains(4) && num_cnt.Contains(5) && num_cnt.Contains(6)) {
            for (int i = 0; i < my_Dice.Count; i++) {
                if (my_Dice[i].GetComponent<Dice>().dice_Num != 1) {
                    my_Dice[i].GetComponent<Dice>().Button_Activate(true);
                }
            }
        }
    }

    void Btn_Off(int dice_num, GameObject obj) {
        List<int> num_cnt = new List<int>();
        num_cnt.Clear();

        for (int i = 0; i < my_Dice.Count; i++) {
            num_cnt.Add(my_Dice[i].GetComponent<Dice>().dice_Num);
        }

        if (my_Dice.Count > 3) {
            if (dice_num == 2 || dice_num == 3 || dice_num == 4 || dice_num == 6) {
                for (int i = 0; i < my_Dice.Count - 2; i++) {
                    for (int j = i + 1; j < my_Dice.Count - 1; j++) {
                        for (int k = i + 2; k < my_Dice.Count; k++) {
                            if (my_Dice[i].GetComponent<Dice>().dice_Num == my_Dice[j].GetComponent<Dice>().dice_Num &&
                                my_Dice[j].GetComponent<Dice>().dice_Num == my_Dice[k].GetComponent<Dice>().dice_Num &&
                                my_Dice[k].GetComponent<Dice>().dice_Num == dice_num) {
                                return;
                            }
                        }
                    }
                }

                int cnt = 0;
                int i_cnt = 0;
                for (int i = 0; i < my_Dice.Count; i++) {
                    if (!Equals(my_Dice[i], obj) && my_Dice[i].GetComponent<Dice>().dice_Num == dice_num) {
                        cnt += 1;
                        i_cnt = i;
                    }
                }

                if (cnt == 1) {
                    my_Dice[i_cnt].GetComponent<Dice>().Button_Activate(false);
                }
            }
        }
    }

    void Btn_ReChk_Off() {
        int num1_Cnt = 0;               //카운트 초기화
        int num2_Cnt = 0;
        int num3_Cnt = 0;
        int num4_Cnt = 0;
        int num5_Cnt = 0;
        int num6_Cnt = 0;

        for (int i = 0; i < my_Dice.Count; i++) {
            if (my_Dice[i].GetComponent<Dice>().dice_Num == 1) {                     //1주사위 카운트
                num1_Cnt += 1;
            }
            else if (my_Dice[i].GetComponent<Dice>().dice_Num == 2) {                //2주사위 카운트
                num2_Cnt += 1;
            }
            else if (my_Dice[i].GetComponent<Dice>().dice_Num == 3) {                //3주사위 카운트
                num3_Cnt += 1;
            }
            else if (my_Dice[i].GetComponent<Dice>().dice_Num == 4) {                //4주사위 카운트
                num4_Cnt += 1;
            }
            else if (my_Dice[i].GetComponent<Dice>().dice_Num == 5) {                //5주사위 카운트
                num5_Cnt += 1;
            }
            else if (my_Dice[i].GetComponent<Dice>().dice_Num == 6) {                //6주사위 카운트
                num6_Cnt += 1;
            }
        }

        if (!(num1_Cnt == 1 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1) ||
            !(num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1 && num6_Cnt == 1)) {                  //스트레이트가 아닐 때 제외

            if (num2_Cnt == 2) {
                for (int i = 0; i < my_Dice.Count; i++) {
                    if (my_Dice[i].GetComponent<Dice>().dice_Num == 2) {
                        my_Dice[i].GetComponent<Dice>().Button_Activate(false);
                    }
                }
            }

            if (num3_Cnt == 2) {
                for (int i = 0; i < my_Dice.Count; i++) {
                    if (my_Dice[i].GetComponent<Dice>().dice_Num == 3) {
                        my_Dice[i].GetComponent<Dice>().Button_Activate(false);
                    }
                }
            }

            if (num4_Cnt == 2) {
                for (int i = 0; i < my_Dice.Count; i++) {
                    if (my_Dice[i].GetComponent<Dice>().dice_Num == 4) {
                        my_Dice[i].GetComponent<Dice>().Button_Activate(false);
                    }
                }
            }

            if (num6_Cnt == 2) {
                for (int i = 0; i < my_Dice.Count; i++) {
                    if (my_Dice[i].GetComponent<Dice>().dice_Num == 6) {
                        my_Dice[i].GetComponent<Dice>().Button_Activate(false);
                    }
                }
            }
        }
    }

    #region 룰에 따른 점수

    void Rule_Score(int dice_num) {                                                 //룰에 따른 점수
        int[] cnt_array = new int[6];

        int num1_Cnt = 0;               //카운트 초기화
        int num2_Cnt = 0;
        int num3_Cnt = 0;
        int num4_Cnt = 0;
        int num5_Cnt = 0;
        int num6_Cnt = 0;

        for (int i=0; i < num_List.Count; i++) {
            if (num_List[i] == 1) {                     //1주사위 카운트
                num1_Cnt += 1;
            }
            else if (num_List[i] == 2) {                //2주사위 카운트
                num2_Cnt += 1;
            }
            else if (num_List[i] == 3) {                //3주사위 카운트
                num3_Cnt += 1;
            }
            else if (num_List[i] == 4) {                //4주사위 카운트
                num4_Cnt += 1;
            }
            else if (num_List[i] == 5) {                //5주사위 카운트
                num5_Cnt += 1;
            }
            else if (num_List[i] == 6) {                //6주사위 카운트
                num6_Cnt += 1;
            }
        }

        cnt_array[0] = num1_Cnt;
        cnt_array[1] = num2_Cnt;
        cnt_array[2] = num3_Cnt;
        cnt_array[3] = num4_Cnt;
        cnt_array[4] = num5_Cnt;
        cnt_array[5] = num6_Cnt;

        if (selectDice_List.Count == 1) {                       //선택한 주사위 1개일 때
            if (num1_Cnt == 1) {
                Select_Scoring(100);
                return;
            }

            if (num5_Cnt == 1) {
                Select_Scoring(50);
                return;
            }
        }
        else if (selectDice_List.Count == 2) {                      //선택한 주사위 2개일 때
            if (num1_Cnt == 1 && num5_Cnt == 1) {
                Select_Scoring(100 + 50);
                return;
            }

            if (num1_Cnt == 2) {
                Select_Scoring(100 * num1_Cnt);
                return;
            }

            if (num5_Cnt == 2) {
                Select_Scoring(50 * num5_Cnt);
                return;
            }
        }
        else if (selectDice_List.Count == 3) {                      //선택한 주사위 3개일 때
            if (num1_Cnt == 1 && num5_Cnt == 2) {
                Select_Scoring(100 + (50 * num5_Cnt));
                return;
            }

            if (num1_Cnt == 2 && num5_Cnt == 1) {
                Select_Scoring((100 * num1_Cnt) + 50);
                return;
            }

            if (num1_Cnt == 3) {
                Select_Scoring(1000);
                return;
            }

            for (int i = 0; i< cnt_array.Length; i++) {
                if (cnt_array[i] == 3) {                                       //같은 숫자 3개 조합
                    Select_Scoring(100 * (i+1));
                    return;
                }
            }
        }
        else if (selectDice_List.Count == 4) {                      //선택한 주사위 4개일 때
            if (num1_Cnt == 2 && num5_Cnt == 2) {
                Select_Scoring((100 * num1_Cnt) + (50 * num5_Cnt));
                return;
            }

            if (num1_Cnt == 3 && num5_Cnt == 1) {
                Select_Scoring(1000 + 50);
                return;
            }

            if (num1_Cnt == 4) {
                Select_Scoring(2000);
                return;
            }

            for (int i = 0; i < cnt_array.Length; i++) {
                if (cnt_array[i] == 3 && num1_Cnt == 1) {                               //같은 숫자 3개 조합 +
                    Select_Scoring((100 * (i + 1)) + 100);
                    return;
                }

                if (cnt_array[i] == 3 && num5_Cnt == 1) {
                    Select_Scoring((100 * (i + 1)) + 50);
                    return;
                }

                if (cnt_array[i] == 4) {                                       //같은 숫자 4개 조합
                    Select_Scoring(200 * (i + 1));
                    return;
                }
            }
        }
        else if (selectDice_List.Count == 5) {                      //선택한 주사위 5개일 때
            if (num1_Cnt == 3 && num5_Cnt == 2) {
                Select_Scoring(1000 + (50 * num5_Cnt));
                return;
            }

            if (num1_Cnt == 4 && num5_Cnt == 1) {
                Select_Scoring(2000 + 50);
                return;
            }

            if (num1_Cnt == 5) {
                Select_Scoring(4000);
                return;
            }

            for (int i = 0; i < cnt_array.Length; i++) {
                if (cnt_array[i] == 3 && num1_Cnt == 2) {                               //같은 숫자 3개 조합 +
                    Select_Scoring((100 * (i + 1)) + (100 * num1_Cnt));
                    return;
                }

                if (cnt_array[i] == 3 && num5_Cnt == 2) {
                    Select_Scoring((100 * (i + 1)) + (50 * num5_Cnt));
                    return;
                }

                if (cnt_array[i] == 3 && num1_Cnt == 1 && num5_Cnt == 1) {
                    Select_Scoring((100 * (i + 1)) + 100 + 50);
                    return;
                }

                if (cnt_array[i] == 4 && num1_Cnt == 1) {                               //같은 숫자 4개 조합 +
                    Select_Scoring(200 * (i + 1) + 100);
                    return;
                }

                if (cnt_array[i] == 4 && num5_Cnt == 1) {
                    Select_Scoring(200 * (i + 1) + 50);
                    return;
                }

                if (cnt_array[i] == 5) {                                       //같은 숫자 5개 조합
                    Select_Scoring(400 * (i + 1));
                    return;
                }
            }

            if (num1_Cnt == 1 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1 && num6_Cnt == 0) {        //1, 2, 3, 4, 5 스트레이트
                Select_Scoring(500);
                return;
            }

            if (num1_Cnt == 0 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1 && num6_Cnt == 1) {        //2, 3, 4, 5, 6 스트레이트
                Select_Scoring(750);
                return;
            }
        }
        else if (selectDice_List.Count == 6) {                      //선택한 주사위 6개일 때
            if (num1_Cnt == 4 && num5_Cnt == 2) {
                Select_Scoring(2000 + (50 * num5_Cnt));
                return;
            }

            if (num1_Cnt == 5 && num5_Cnt == 1) {
                Select_Scoring(4000 + 50);
                return;
            }

            if (num1_Cnt == 6) {
                Select_Scoring(8000);
                return;
            }

            for (int i = 0; i < cnt_array.Length; i++) {
                if (cnt_array[i] == 3 && num1_Cnt == 3) {                               //같은 숫자 3개 조합 + 1주사위 3개
                    Select_Scoring((100 * (i + 1)) + 1000);
                    return;
                }

                if (cnt_array[i] == 3 && num1_Cnt == 2 && num5_Cnt == 1) {
                    Select_Scoring((100 * (i + 1)) + (100 * num1_Cnt) + 50);
                    return;
                }

                if (cnt_array[i] == 3 && num1_Cnt == 1 && num5_Cnt == 2) {
                    Select_Scoring((100 * (i + 1)) + 100 + (50 * num5_Cnt));
                    return;
                }

                if (cnt_array[i] == 4 && num1_Cnt == 2) {                               //같은 숫자 4개 조합 +
                    Select_Scoring((200 * (i + 1)) + (100 * num1_Cnt));
                    return;
                }

                if (cnt_array[i] == 4 && num5_Cnt == 2) {
                    Select_Scoring((200 * (i + 1)) + (50 * num5_Cnt));
                    return;
                }

                if (cnt_array[i] == 4 && num1_Cnt == 1 && num5_Cnt == 1) {                   //같은 숫자 4개 조합 + 1주사위 1개, 5주사위 1개
                    Select_Scoring((200 * (i + 1)) + 100 + 50);
                    return;
                }

                if (cnt_array[i] == 5 && num1_Cnt == 1) {                                       //같은 숫자 5개 조합
                    Select_Scoring((400 * (i + 1)) + 100);
                    return;
                }

                if (cnt_array[i] == 5 && num5_Cnt == 1) {
                    Select_Scoring((400 * (i + 1)) + 50);
                    return;
                }

                if (cnt_array[i] == 6) {                                        //같은 숫자 6개 조합
                    Select_Scoring(800 * (i + 1));
                    return;
                }
            }

            for (int i = 0; i < cnt_array.Length -1; i++) {                                       //같은 숫자 3개 조합 + 같은 숫자 3개 조합
                for (int j = i + 1; j < cnt_array.Length; j++) {
                    if (cnt_array[i] == 3 && cnt_array[j] == 3) {
                        Select_Scoring((100 * (i + 1)) + (100 * (j + 1)));
                        return;
                    }
                }
            }

            if (num1_Cnt == 2 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1 && num6_Cnt == 0) {       //1, 2, 3, 4, 5 스트레이트 + 1주사위
                Select_Scoring(500 + 100);
                return;
            }

            if (num1_Cnt == 1 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 2 && num6_Cnt == 0) {       //1, 2, 3, 4, 5 스트레이트 + 5주사위
                Select_Scoring(500 + 50);
                return;
            }

            if (num1_Cnt == 0 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 2 && num6_Cnt == 1) {        //2, 3, 4, 5, 6 스트레이트 + 5주사위
                Select_Scoring(750 + 50);
                return;
            }

            if (num1_Cnt == 1 && num2_Cnt == 1 && num3_Cnt == 1 && num4_Cnt == 1 && num5_Cnt == 1 && num6_Cnt == 1) {        //1, 2, 3, 4, 5, 6 스트레이트
                Select_Scoring(1500);
                return;
            }
        }

        Select_Scoring(0);          //선택된 족보 없음 0점
    }

    #endregion
}