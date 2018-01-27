using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float health = 1f;

    private bool selected = false;
    private bool stoppingTime = false;

    public GameObject bulletPrefab;

    public string playerID = "0";

    // Use this for initialization
    void Start()
    {
        if (playerID == "1")
        {
            Select();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // Select player
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (Input.GetKeyDown(playerID))
            {
                selected = true;
            }
            else
            {
                selected = false;
            }
        }

        if (selected)
        {
            // Face cursor
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetVector = mousePosition - transform.position;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

            //transform.LookAt(Input.mousePosition);

            // Player Movement controls
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
            }

            // Time Stop controls
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (stoppingTime)
                {
                    StartTime();
                }
                else
                {
                    StopTime();
                }
            }

            // Shooting controls
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }
        }
    }
    
    void Select() {
        selected = true;
        TimeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
    }

    void Shoot()
    {
        // Fire a bullet in the direction the player is facing
        GameObject newBulletGO = (GameObject)Instantiate(bulletPrefab, transform.position + transform.up * 0.5f, transform.rotation);
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
