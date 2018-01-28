using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class EnemyEars : MonoBehaviour {

    protected Enemy owner;

    // Use this for initialization
    void Start() {
        owner = GetComponentInParent<Enemy>();
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.radius = getRadius();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Hear(collision.gameObject);
    }

    protected abstract void Hear(GameObject obj);
    protected abstract float getRadius();   
}
