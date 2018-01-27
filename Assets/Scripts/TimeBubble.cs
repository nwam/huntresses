using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour {

    private BloodPool bloodPool = BloodPool.Instance();

    private bool isActive;
    private Renderer rend;

    private List<IFreezable> collisions = new List<IFreezable>();

    private Transform activePlayerPos;

    [SerializeField]
    private KeyCode key;

    // Use this for initialization
    void Start () {
        isActive = false;
        rend = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        bool wasActive = isActive;

        if(Input.GetKeyDown(key)) {
            isActive = !isActive;
        }

        rend.enabled = isActive;

        transform.position = activePlayerPos.transform.position;
        if (isActive && !wasActive) {
            collisions.ForEach(obj => obj.Freeze());
        }
        else if(!isActive && wasActive) {
            collisions.ForEach(obj => obj.UnFreeze());
        }
    }

    public void SetSelectedPlayer(Player player) {
        activePlayerPos = player.transform;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("timebubble collides with " + collision.gameObject.name);
        IFreezable freezable = collision.gameObject.GetComponent<IFreezable>();

        if(freezable != null) {
            if(isActive) {
                freezable.Freeze();
            }
            collisions.Add(freezable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Debug.Log("timebubble STOPS collide with " + collision.gameObject.name);
        IFreezable freezable = collision.gameObject.GetComponent<IFreezable>();

        if (freezable != null) {
            if(isActive) {
                freezable.UnFreeze();
            }
            collisions.Remove(freezable);
        }
    }
}
