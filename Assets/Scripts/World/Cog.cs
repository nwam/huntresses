using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cog : MonoBehaviour, IFreezable, IShootable, IPathLogic {
    
    private bool active = true;
    [SerializeField]
    private Gate controlled = null;
    [SerializeField]
    private float chargeRate = 1f;

    [SerializeField]
    private GameObject brokenCogPrefab;

	// Use this for initialization
	void Start () {
        OnSpawn();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (controlled != null) {
            PowerGate();
        }
	}

    public void GetShot(int d) {
        if (d == 3) { // Hacky check for LargeBullet
            Debug.Log(this.gameObject.name + " has been broken");
            GameObject newBrokenCog = Instantiate(brokenCogPrefab, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }

    private void PowerGate() {
        if (active) {
            controlled.SetEnergy(controlled.GetEnergy() + chargeRate * Time.deltaTime);
        }
    }

    public void Freeze() {
        Debug.Log(name + " frozen");
        active = false;
    }

    public void UnFreeze() {
        Debug.Log(name + " unfrozen");
        active = true;
    }

    public bool IsDestroyed() {
        return this == null;
    }
    
    public float Priority() {
        // Should actually return the priority of the thing...
        return 0f;
    }

    public string MapKey() {
        return "Cog";
    }

    public void OnSpawn() {
        WorldGrid.Instance.AddToMap(this.gameObject);
    }
}
