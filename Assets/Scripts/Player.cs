using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IShootable {
    [SerializeField]
    private TimeBubble timeBubble;

    [SerializeField]
    private int health = 10;

    // Update is called once per frame
    void Update() {
        
    }

    void FixedUpdate() {

    }

    public void GetShot(int damage) {
        // Duped with Enemy but will diverge later (right?)
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            // Die
            Debug.Log(name + " is dead");
        }
    }
}
