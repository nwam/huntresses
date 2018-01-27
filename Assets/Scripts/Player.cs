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

    // Use this for initialization
    void Start()
    {
        selected = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
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
                } else
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

    void StopTime()
    {
        // Does nothing atm
    }

    void StartTime()
    {
        // Does nothing atm
    }

    void Shoot()
    {
        // Fire a bullet in the direction the player is facing
        GameObject newBulletGO = (GameObject)Instantiate(bulletPrefab, transform.position + transform.up * 0.5f, transform.rotation);
    }
}
