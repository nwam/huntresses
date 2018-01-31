using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour {

    private bool isActive;
    private Renderer rend;

    private List<IFreezable> collisions = new List<IFreezable>();

    // [SerializeField]
    private Player attachedPlayer;

    // [SerializeField]
    private BloodPool bloodPool;

    [SerializeField]
    private KeyCode key;

    // Use this for initialization
    void Start() {
        isActive = false;
        rend = GetComponent<Renderer>();
        attachedPlayer = GetComponentInParent<Player>();
        bloodPool = FindObjectOfType<BloodPool>();
    }

    // Update is called once per frame
    void Update() {
        bool wasActive = isActive;

        if (Input.GetKeyDown(key) && attachedPlayer.IsSelected()) {
            isActive = !isActive;
            // Debug.Log(attachedPlayer.name + " toggled timebubble " + isActive);
        }

        if(isActive && !wasActive) {
            // ask the player if it is in a state where it can activate the bubble.
            if(!attachedPlayer.OnStartTimeBubble()) {
                Debug.Log("Not starting time bubble cause " + attachedPlayer.gameObject.name + " is not selected");
                isActive = false; 
            }
        }

        if (isActive) {
            // These checks must be done always when the bubble is active - either running out of blood
            // or entering Overwatch state cancels bubble.
            if (attachedPlayer.GetState() == PlayerState.OVERWATCH || !bloodPool.Withdraw()) {
                // Debug.Log("cancelling bubble due to overwatch");
                isActive = false;
            }
        }

        rend.enabled = isActive;
        attachedPlayer.getAnimator().SetBool("spell", isActive);

        if (isActive) {
            // Remove destroyed objects - should be a way to optimize this
            for (int i = 0; i < collisions.Count; i++) {
                if (collisions[i].isDestroyed()) {
                    collisions.RemoveAt(i);
                }
            }
        }

        transform.position = attachedPlayer.transform.position;
        if (isActive && !wasActive) {
            collisions.ForEach(obj => obj.Freeze());
        }
        else if (!isActive && wasActive) {
            collisions.ForEach(obj => obj.UnFreeze());
            attachedPlayer.OnStopTimeBubble();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        IFreezable freezable = collision.gameObject.GetComponent<IFreezable>();
        if (freezable != null) {
            Freeze(freezable);
            //Debug.Log("Freezing " + collision.gameObject.name);
        }
    }

    private void Freeze(IFreezable freezable) {
        if (freezable != null && !collisions.Contains(freezable)) {
            if (isActive) {
                freezable.Freeze();
            }
            collisions.Add(freezable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        IFreezable freezable = collision.gameObject.GetComponent<IFreezable>();

        if (freezable != null) {
            if (isActive) {
                freezable.UnFreeze();
            }
            collisions.Remove(freezable);
        }
    }
}
