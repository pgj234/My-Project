using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletMove : MonoBehaviour {

    public float bulletForce = 1200.0f;
    public float DestroyDelay = 2.5f;
    public int actorNumber = -1;

    public int bulletDMG { get { return bulletDmg; } }
    int bulletDmg = 1;

    void Start() {
        GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * bulletForce);
        Destroy(gameObject, DestroyDelay);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Player") || col.CompareTag("Wall")) {
            Destroy(this.gameObject);
        }
    }
}
