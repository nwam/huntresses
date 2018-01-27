using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IFreezable {

    [SerializeField]
    private const float SPEED = 15f;

    private float currentSpeed = SPEED;

    [SerializeField]
    public int damage = 1;

    private void FixedUpdate()
    {
        transform.position += currentSpeed * transform.up * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject other = collision.gameObject;

        if (other == null) {
            return;
        }

        IShootable shootable = other.GetComponent<IShootable>();
        if (shootable != null) {
            shootable.GetShot(damage);
        }

        // Ignore objects that are tagged to ignore bullets
        if (other.tag == "ignores-bullets") {
            return;
        }
        

        // Hit something other than an enemy
        // Walls, doors, etc. just destroy the bullet - Anything else a bullet can interact with?
        Debug.Log("Destroying: " + name);
        Destroy(gameObject);
    }

    public void Freeze() {
        Debug.Log(name + " frozen");
        currentSpeed = 0;
    }

    public void UnFreeze() {
        Debug.Log(name + " unfrozen");
        currentSpeed = SPEED;
    }

    public bool isDestroyed() {
        return this == null;
    }
}
