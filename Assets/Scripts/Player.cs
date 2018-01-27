using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IShootable
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float health = 1f;
    [SerializeField]
    private bool selected = false;
    [SerializeField]
    private KeyCode harvestKey;

    public GameObject bulletPrefab;
    public GameObject corpsePrefab;
    //public TimeBubble timeBubble;

    private BloodPool bloodPool;

    public string playerID = "0";

    private Corpse harvestTarget;

    private bool overwatching = false;
    private Quaternion overwatchRotation;

    // Use this for initialization
    void Start()
    {
        if (playerID == "1")
        {
            Select();
        }
        bloodPool = FindObjectOfType<BloodPool>();
    }

    private void FixedUpdate()
    {
        // Select player
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (Input.GetKeyDown(playerID))
            {
                Select();
            }
            else
            {
                Deselect();
            }
        }

        if (selected)
        {
            // Face cursor
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetVector = mousePosition - transform.position;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Normalize(targetVector));

            //transform.LookAt(Input.mousePosition);

            // Player Movement controls
            if (Input.GetKey(KeyCode.W))
            {
                Move(Vector3.up);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Move(Vector3.down);
            }
            if (Input.GetKey(KeyCode.A))
            {
                Move(Vector3.left);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Move(Vector3.right);
            }

            // Shooting controls
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
                EndOverwatch();
            }

            // Overwatch
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartOverwatch();
            }

            // Harvesting

        }
        else // Not selected
        {
            // Overwatching
            if (overwatching)
            {
                Overwatch();
            }
        }
    }

    public bool isSelected() {
        return selected;
    }
    
    void Select() {
        selected = true;
        //timeBubble.SetSelectedPlayer(this);
    }

    void Deselect() {
        selected = false;
    }

    void Move(Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;

        EndOverwatch();
    }

    void Shoot()
    {
        // Fire a bullet in the direction the player is facing
        GameObject newBulletGO = Instantiate(bulletPrefab, transform.position + transform.up * 0.8f, transform.rotation);
    }

    void StartOverwatch()
    {
        overwatching = true;
        overwatchRotation = transform.rotation;
        Debug.Log("Player " + playerID + " overwatching");
    }

    void EndOverwatch()
    {
        overwatching = false;
    }

    void Overwatch()
    {
        transform.rotation = overwatchRotation;

        // Find an enemy.
        GameObject visibleEnemy = LookForEnemy();
        if (visibleEnemy == null)
        {
            Debug.Log("No visible enemy");
            return;
        }

        // Found an enemy. Rotate to point right at enemy.
        Vector3 targetVector = visibleEnemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(transform.forward, targetVector);

        // FIRE ZE ARROWS!!!1!
        Shoot();

    }

    private GameObject LookForEnemy()
    {
        Vector2 frontOfSelf = (Vector2)(transform.position + transform.lossyScale.x * transform.up.normalized);

        //Debug.Log(frontOfSelf);

        RaycastHit2D castHit = Physics2D.Raycast(frontOfSelf, transform.up);

        if (castHit.transform != null)
        {
            GameObject hitObject = castHit.transform.gameObject;

            if (hitObject.CompareTag("Enemy"))
            {
                return hitObject;
            }
        }
        return null;
    }

    public void GetShot(int damage) {
        // Duped with Enemy but will diverge later (right?)
        // Re: above: probably won't? both should die and leave behind a corpse. Their Die() methods will differ once we get sprites in.
        health -= damage;
        Debug.Log(name + " got shot, now I have " + health + "hp");

        if (health <= 0) {
            Debug.Log(name + " is dead");
            Die();
        }
    }

    private void Die()
    {
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        // TODO: Make the corpse use the appropriate player corpse

        Destroy(gameObject);
    }

    public bool Harvest(Corpse corpse) {
        CircleCollider2D harvester = this.gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D harvestable = corpse.GetComponent<CircleCollider2D>();

        if (harvester.IsTouching(harvestable) && (!corpse.getBeingHarvested() || corpse == harvestTarget))
        {
            harvestTarget = corpse;
            float drained = corpse.beHarvested();
            
            return drained != 0;
        }

        return false;
    }
}