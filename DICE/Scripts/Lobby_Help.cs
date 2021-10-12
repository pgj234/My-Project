using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby_Help : MonoBehaviour {

    public GameObject help_Img;

    public void Help() {
        if (help_Img.activeSelf == false) {          //도움말 ON
            help_Img.SetActive(true);
        }
        else {                                       //도움말 OFF
            help_Img.SetActive(false);
        }
    }
}
