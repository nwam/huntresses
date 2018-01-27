using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodPool : MonoBehaviour {

    /*
    // Singleton
    private static BloodPool instance;

    public static BloodPool Instance() {
        if(instance == null) {
            instance = new BloodPool();
        }
        return instance;
    }*/
    
    [SerializeField]
    private int maxDuration = 60;       // In seconds

    [SerializeField]
    private int drainPerSecond = 1;

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

    public void Fill(int amount) {
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
