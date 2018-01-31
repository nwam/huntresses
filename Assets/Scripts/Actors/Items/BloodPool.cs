using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodPool : MonoBehaviour {
    [SerializeField]
    private float maxDuration = 60;       // In seconds

    [SerializeField]
    private float drainPerSecond = 0;

    [SerializeField]
    private Text text;

    [SerializeField]
    private Text notifArea;

    // amount of blood in the pool - starts full, cannot go below 0
    private float available;

    private void Start() {
        available = maxDuration;
    }

    private void LateUpdate() {
        text.text = Mathf.Round(available) + " / " + maxDuration;
    }

    public bool Withdraw() {
        float toWithdraw = drainPerSecond * Time.deltaTime;
        if(toWithdraw <= available) {
            available -= toWithdraw;
            return true;
        }
        else {
            // Not enough available
            StartCoroutine(displayNotEnoughBlood());
            return false;
        }        
    }

    // Returns the amount of blood added to the pool. Ie returns 0 if the pool is full.
    public float Fill(float amount) {
        float originalAvailable = available;
        available += amount;
        if(available > maxDuration) {
            available = maxDuration;
        }
        return available - originalAvailable;
    }
    /*
    public bool IsFull() {
        return available >= maxDuration;
    }
    */

    IEnumerator displayNotEnoughBlood() {
        notifArea.text = "No more blood!";
        yield return new WaitForSeconds(1);
        notifArea.text = "";
    }
}
