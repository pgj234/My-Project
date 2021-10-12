using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

    public GameObject loadGameManager;

    void Awake () {
        if (GameManager.instance == null) {
            Instantiate(loadGameManager);
        }
    }
}
