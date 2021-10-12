using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial : MonoBehaviour {

    public GameObject Tutorial;

	void Update () {
        if (GameManager.level == 1) {
            Time.timeScale = 0;

            if (Input.GetKeyUp(KeyCode.Return)) {
                Time.timeScale = 1;
                Tutorial.SetActive(false);
            }
        }
        else {
            Tutorial.SetActive(false);
        }
	}
}
