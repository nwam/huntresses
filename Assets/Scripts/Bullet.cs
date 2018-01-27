using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 15f;

    [SerializeField]
    public int damage = 1;

    // Use this for initialization
    void Start()
    {
       
    }

    private void FixedUpdate()
    {
        transform.position += speed * transform.up * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject other = collision.gameObject;

        if (other == null) {
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null) {
            enemy.DecreaseHealth(damage);
        }

        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null) {
            // Do not interact with other bullets
            return;
        }

        // Hit something other than an enemy
        // Walls, doors, etc. just destroy the bullet - Anything else a bullet can interact with?
        Debug.Log("Destroying: " + name);
        gameObject.SetActive(false);
        Destroy(this);
    }
}
