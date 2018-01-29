using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowObject))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour, IFreezable {

    // The speed of the bullet when it is not frozen.
    private float speed;

    private float currentSpeed;

    [SerializeField]
    public int damage = 1;

    private FollowObject followObj;

    // Hacky for the hackathon
    // Removes the need to rotate arrow
    [SerializeField]
    public Animator animator;

    private Actor creator;

    private const int playerBulletSpeed = 18;
    private const int enemyBulletSpeed = 10;

    private bool isFrozen = false;

    //private Collider2D edgeCollider;

    public void SetCreator(Actor actor) {
        creator = actor;
    }

    public bool IsPlayerBullet() {
        return creator.gameObject.GetComponent<Player>() != null;
    }

    /*
    public int GetBulletSpeed() {
        return IsPlayerBullet() ? playerBulletSpeed : enemyBulletSpeed;
    }
    */

    void Start() {
        followObj = GetComponent<FollowObject>();

        speed = IsPlayerBullet() ? playerBulletSpeed : enemyBulletSpeed;
        currentSpeed = isFrozen ? 0 : speed;
    }


    void Update() {

    }

    private void FixedUpdate() {
        // Debug.Log("edgeCollider null? " + edgeCollider == null);
        transform.position += currentSpeed * transform.up * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;
        //SpriteRenderer renderer = other.GetComponent<SpriteRenderer>();
        Debug.Log("Shot " + other.name);

        if (other == null || other.tag == "ignores-bullets") {
            return;
        }
        else if(isFrozen) {
        }
        else {
            // Remove the collider so that we don't hit anything else UNLESS we're hitting a timebubble
            if (other.GetComponent<TimeBubble>() == null) {
                Collider2D edgeCollider = GetComponent<Collider2D>();
                if (edgeCollider == null) {
                    Debug.LogError("null edgecollider on bullet");
                }
                edgeCollider.enabled = false;
            }

            OnHit(other);
            // Animator destroys bullet, but disable bullet behaviour until then
            enabled = false;
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
    }

    private void OnHit(GameObject other) {
        IShootable shootable = other.GetComponent<IShootable>();
        if (shootable != null) {
            // Hit an enemy
            shootable.GetShot(damage);
            animator.SetBool("bleed", true);

            if (followObj != null) {
                Vector3 offset = transform.position - other.transform.position;
                followObj.setOffset(offset);
                followObj.setTarget(other.transform);
                followObj.enable();
            }
        }
        else {
            // Hit something other than an enemy
            // Walls, doors, etc.
            // In this case the animation is stationary
            animator.SetBool("spark", true);
        }
    }

    public void Freeze() {
        currentSpeed = 0;
        isFrozen = true;
    }

    public void UnFreeze() {
        currentSpeed = speed;
        isFrozen = false;
    }

    public bool isDestroyed() {
        return this == null;
    }
}
