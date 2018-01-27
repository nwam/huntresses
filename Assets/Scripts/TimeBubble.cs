using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour {

    private BloodPool bloodPool = BloodPool.Instance();

    private bool isActive;
    private Renderer rend;

    private List<IFreezable> collisions = new List<IFreezable>();

    [SerializeField]
    private Player attachedPlayer;

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

        if(Input.GetKeyDown(key) && attachedPlayer.isSelected()) {
            isActive = !isActive;
        }

        rend.enabled = isActive;

        // Remove destroyed objects
        for(int i = 0; i < collisions.Count; i++) {
            if(collisions[i].isDestroyed()) {
                collisions.RemoveAt(i);
            }
        }

        transform.position = attachedPlayer.transform.position;
        if (isActive && !wasActive) {
            collisions.ForEach(obj => obj.Freeze());
        }
        else if(!isActive && wasActive) {
            collisions.ForEach(obj => obj.UnFreeze());
        }
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
