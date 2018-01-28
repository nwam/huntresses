using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IFreezable {

    [SerializeField]
    private const float SPEED = 15f;

    private float currentSpeed = SPEED;

    [SerializeField]
    public int damage = 1;

	private bool hasHit = false;

	Animator animator;

	void Start(){
		// Hacky for the hackathon
		// Removes the need to rotate arrow
		animator = transform.GetChild(0).GetComponent<Animator> ();
	}

	private void FixedUpdate()
    {
		transform.position += currentSpeed * transform.up * Time.deltaTime;

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject other = collision.gameObject;

        if (other == null) {
            return;
        }

        // Ignore objects that are tagged to ignore bullets
        if (other.tag == "ignores-bullets") {
            return;
        }


		IShootable shootable = other.GetComponent<IShootable>();
		if (shootable != null) {
			// Hit an enemy
			shootable.GetShot (damage);
			animator.SetBool ("bleed", true);
		} else {
			// Hit something other than an enemy
			// Walls, doors, etc.
			animator.SetBool ("spark", true);
		}
        
		// Animator destroys bullet, but disable bullet behaviour until then
		enabled = false;
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
