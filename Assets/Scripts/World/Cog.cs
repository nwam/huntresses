using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cog : MonoBehaviour, IFreezable {

    private bool active;
    [SerializeField]
    private Gate controlled = null;
    [SerializeField]
    private float chargeRate = 30f;

    [SerializeField]
    private GameObject brokenCogPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (controlled != null) {
            PowerGate();
        }
	}

    private void PowerGate() {
        if (active) {
            controlled.setEnergy(controlled.getEnergy() + chargeRate * Time.deltaTime);
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

    public bool isDestroyed() {
        return this == null;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        LargeBullet largeBullet = collision.gameObject.GetComponent<LargeBullet>();
        
        if (largeBullet != null) {
            GameObject newBrokenCog = Instantiate(brokenCogPrefab, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }

}
