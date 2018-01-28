using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Actor : MonoBehaviour, IShootable, IHarvester {

    [SerializeField]
    protected int health;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float fov = 60;     // in degrees

    [SerializeField]
    protected GameObject projectilePrefab;

    [SerializeField]
    protected GameObject corpsePrefab;

    [SerializeField]
    protected float timePerShot = 1f; // in seconds
    private float timeUntilShot = 0f;

    private List<Corpse> harvestableCorpses = new List<Corpse>();
    protected bool isHarvesting = false;

	public Animator animator;

	void Start(){
		animator = GetComponent<Animator> ();
		AfterStart ();
	}

	protected virtual void AfterStart(){
	}

    // Update is called once per frame
    protected virtual void FixedUpdate() {
        if (timeUntilShot > 0f) {
            timeUntilShot -= Time.deltaTime;
        }
    }

    /**
     * TargetTag - "Player" or "Enemy"
     * ForwardDirection - UP for player, RIGHT for enemy.
     */
    protected GameObject LookFor(string targetTag, Vector3 forwardDirection) {
        // Enemies array could be cached to improve performance
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject target in targets) {
            float angle = Vector2.Angle(target.transform.position - transform.position, forwardDirection);

            if (Mathf.Abs(angle) <= fov / 2) {
                Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * forwardDirection.normalized);
                Vector2 rayDirection = target.transform.position - transform.position;
                RaycastHit2D castHit = Physics2D.Raycast(frontOfSelf, rayDirection);
                Debug.DrawRay(frontOfSelf, rayDirection);

                if (castHit.transform != null) {
                    GameObject hitObject = castHit.transform.gameObject;

                    if (hitObject.CompareTag(targetTag)) {
                        return hitObject;
                    }
                }
            }
        }

        return null;
    }

    protected abstract GameObject LookForOpponent();
    protected abstract void Shoot();

    protected virtual void Shoot(bool isEnemy) {
        if (timeUntilShot <= 0f) {
			animator.SetBool ("shoot", true);
            // Hacky McHacker for isEnemy
            float zRotation = isEnemy ? -90 : 0;
            Vector3 shootAngle = isEnemy ? transform.right : transform.up;

            // Fire a bullet in the direction the player is facing
            Vector3 ea = transform.rotation.eulerAngles;
            GameObject newBulletGO = Instantiate(projectilePrefab, transform.position + shootAngle * 1.2f, 
                Quaternion.Euler(ea.x, ea.y, ea.z + zRotation));

            timeUntilShot = timePerShot;
        }
    }

    public void GetShot(int damage) {
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            Debug.Log(name + " is dead");
            Die();
        }
    }

    protected void Die() {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        // TODO: Make the corpse use the appropriate player corpse

        Destroy(gameObject);
    }

#region Harvesting

    protected virtual float Harvest() {
        if (harvestableCorpses.Count == 0) {
            return 0f;
        }

        // TODO Play harvesting animation

        float drained = harvestableCorpses[0].Harvest(this);
        if (drained > 0) {
            isHarvesting = true;
        }
        else {
            StopHarvest();
        }
        return drained;
    }

    protected void StopHarvest() {
        if (harvestableCorpses.Count == 0) {
            return;
        }
        harvestableCorpses[0].StopHarvest(this);
        isHarvesting = false;
    }

    public void AddHarvestTarget(Corpse corpse) {
        harvestableCorpses.Add(corpse);
    }

    public void RemoveHarvestTarget(Corpse corpse) {
        harvestableCorpses.Remove(corpse);
    }

#endregion Harvesting

}
