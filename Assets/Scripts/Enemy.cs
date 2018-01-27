using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IShootable, IFreezable {

    public const float SPEED = 5f;

    [SerializeField]
    private float turnSpeed = 7.5f;
    [SerializeField]
    private int health = 3;

    private float currentSpeed = SPEED;

    private void FixedUpdate()
    {
        bool turning = false;
        if (transform.position.y >= 5)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x, 5, 0), transform.rotation);
            transform.Rotate(transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (transform.rotation.z > 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 180));
            }
        }
        else if (transform.position.y <= -5)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x, -5, 0), transform.rotation);
            transform.Rotate(transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (transform.rotation.z < 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            }
        }

        if (!turning)
        {
            transform.position += currentSpeed * transform.up * Time.deltaTime;
        }
    }

    public void GetShot(int damage) {
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            // Die
            Debug.Log(name + " is dead");
        }
    }

    public void Freeze() {
        Debug.Log(name + " frozen");
        currentSpeed = 0;
        // Also cannot rotate or shoot
    }

    public void UnFreeze() {
        Debug.Log(name + " unfrozen");
        currentSpeed = SPEED;
        // Restore ability to rotate and shoot
    }
}
