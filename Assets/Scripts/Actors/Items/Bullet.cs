using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowObject))]
public class Bullet : MonoBehaviour, IFreezable {

    [SerializeField]
    private const float SPEED = 15f;

    private float currentSpeed = SPEED;

    [SerializeField]
    public int damage = 1;

	private bool hasHit = false;

	private FollowObject followObj;

	// Hacky for the hackathon
	// Removes the need to rotate arrow
	[SerializeField]
	public Animator animator;


	void Start(){
		followObj = GetComponent<FollowObject> ();
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Z)) {
			Debug.Break ();
		}
	}

	private void FixedUpdate() {
		transform.position += currentSpeed * transform.up * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;

		if (other == null || other.tag == "ignores-bullets") {
            return;
        }

		IShootable shootable = other.GetComponent<IShootable>();
		if (shootable != null) {
			// Hit an enemy
			shootable.GetShot (damage);
			animator.SetBool ("bleed", true);
			Vector3 offset = transform.position - other.transform.position;

			if (followObj != null) {
				followObj.setOffset (offset);
				followObj.setTarget (other.transform);
				followObj.enable ();
			}
		}
		else {
			// Hit something other than an enemy
			// Walls, doors, etc.
			// In this case the animation is stationary
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
