using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_menu : MonoBehaviour {
    public GameObject pause_menu;

	void Update () {
        if (pause_menu.activeSelf == false && Input.GetKeyUp(KeyCode.Escape) == true) {
            pause_menu.SetActive(true);
            Time.timeScale = 0;
        }
        else if (pause_menu.activeSelf == true && Input.GetKeyUp(KeyCode.Escape) == true) {
            pause_menu.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
