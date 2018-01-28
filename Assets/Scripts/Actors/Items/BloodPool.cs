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
    private GameObject needBloodIndicator;

    // amount of blood in the pool - starts full, cannot go below 0
    private float available;

    private void Start() {
        available = maxDuration;
        needBloodIndicator.SetActive(false);
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

    public void Fill(float amount) {
        available += amount;
        if(available > maxDuration) {
            available = maxDuration;
        }
    }

    IEnumerator displayNotEnoughBlood() {
        needBloodIndicator.SetActive(true);
        yield return new WaitForSeconds(1);
        needBloodIndicator.SetActive(false);
    }
}
