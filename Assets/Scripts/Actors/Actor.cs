using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public abstract class Actor : MonoBehaviour, IShootable, IHarvester, IPathLogic {
    
    protected int health;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float fov = 60;     // in degrees

    protected enum EnemyType { Normal, Large };

    [SerializeField]
    protected EnemyType enemyType;
    [SerializeField]
    protected Bullet bulletPrefab;
    [SerializeField]
    protected Bullet largeBulletPrefab;

    [SerializeField]
    protected GameObject corpsePrefab;

    private float timeUntilShot = 0f;

    private List<Corpse> harvestableCorpses = new List<Corpse>();
    protected bool isHarvesting = false;

    protected Animator animator;
    protected Renderer rendererr;

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        rendererr = GetComponent<Renderer>();
        OnSpawn();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate() {
        if (timeUntilShot > 0f) {
            timeUntilShot -= Time.deltaTime;
        }
    }

    public Animator getAnimator() {
        return animator;
    }


    /**
     * TargetTag - "Player" or "Enemy"
     * ForwardDirection - UP for player, RIGHT for enemy.
     */
    protected GameObject LookFor(string targetTag, Vector3 forwardDirection, float fov) {
        // Enemies array could be cached to improve performance
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject target in targets) {
            float angle = Vector2.Angle(target.transform.position - transform.position, forwardDirection);

            if (Mathf.Abs(angle) <= fov / 2) {
                Vector2 rayDirection = target.transform.position - transform.position;
                RaycastHit2D castHit = Physics2D.Raycast(transform.position, rayDirection);
                Debug.DrawRay(transform.position, rayDirection);

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
    protected abstract float GetFireRate();
    protected abstract void Shoot();

    protected virtual void Shoot(bool isEnemy) {
        if (timeUntilShot <= 0f) {
            animator.SetBool("shoot", true);

            // Hacky McHacker for isEnemy
            float zRotation = isEnemy ? -90 : 0;
            Vector3 shootAngle = isEnemy ? transform.right : transform.up;

            // Fire a bullet in the direction the player is facing
            Vector3 target = transform.position + shootAngle * 0.8f;

            Vector3 ea = transform.rotation.eulerAngles;
            Quaternion rotation = Quaternion.Euler(ea.x, ea.y, ea.z + zRotation);

            Bullet fab = bulletPrefab;
            if (enemyType == EnemyType.Large) {
                fab = largeBulletPrefab;
            }
            if (fab != null) {
                Bullet newBulletGO = Instantiate(fab, target, rotation);
                newBulletGO.SetCreator(this);
            }
            else {
                Debug.LogError("Missing bullet prefab for " + name);
            }

            timeUntilShot = GetFireRate();
        }
    }

    public void GetShot(int damage) {
        health -= damage;
        // Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            Debug.Log(name + " is dead");
            Die();
        }
    }

    // Subclasses are responsible for destroying themselves when they die. Call this, perform any post-death actions, then destroy self.
    protected virtual void Die() {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        Destroy(gameObject);
        rendererr.enabled = false;
    }

    #region Harvesting

    protected virtual float Harvest() {
        if (harvestableCorpses.Count == 0) {
            Debug.Log("Nothing to harvest");
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

    protected virtual void StopHarvest() {
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

    public virtual float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public virtual string MapKey() {
        return "Actor";
    }

    public void OnSpawn() {
        WorldGrid.Instance.AddToMap(this.gameObject);
    }
}
