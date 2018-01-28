using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowObject))]
[RequireComponent(typeof(Collider2D))]
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

    private Actor creator;

    //private Collider2D edgeCollider;

    public void SetCreator(Actor actor) {
        creator = actor;
    }


	void Start(){
		followObj = GetComponent<FollowObject> ();
	}


	void Update() {
		if (Input.GetKeyDown (KeyCode.Z)) {
			Debug.Break ();
		}
	}

	private void FixedUpdate() {
        // Debug.Log("edgeCollider null? " + edgeCollider == null);
		transform.position += currentSpeed * transform.up * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;
        //SpriteRenderer renderer = other.GetComponent<SpriteRenderer>();
        Debug.Log("Shot " + other.name);
        //Debug.Break();

        if (other == null || other.tag == "ignores-bullets") {
            return;
        }

        // Remove the collider so that we don't hit anything else UNLESS we're hitting a timebubble
        if(other.GetComponent<TimeBubble>() == null) {
            Collider2D edgeCollider = GetComponent<Collider2D>();
            if(edgeCollider == null) {
                Debug.LogError("null edgecollider on bullet");
            }
            edgeCollider.enabled = false;
        }

        /*
        Debug.Log("The creator is at " + creator.transform.position);
        Debug.Log("The collision is at " + other.transform.position);
        Debug.Log("The offset is " + offset);
        float dist = Vector2.Distance(creator.transform.position, other.transform.position);
        Debug.Log("The distance is " + dist);
        if (dist < 2) {
            // The target is very close by; cast a ray to figure out where we hit it so that the animation doesn't spawn inside of it
            RaycastHit2D hit = Physics2D.Raycast(creator.transform.position, other.transform.position);
            gameObject.transform.position = hit.transform.position;
            Debug.Log("Object is too close, and the ray hits at " + hit.transform.position);
            Debug.Log("The NEW offset is " + offset);
        }
        */

		IShootable shootable = other.GetComponent<IShootable>();
		if (shootable != null) {
			// Hit an enemy
			shootable.GetShot (damage);
			animator.SetBool ("bleed", true);

            if(followObj != null) {
                Vector3 offset = transform.position - other.transform.position;
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
