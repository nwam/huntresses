using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Notifier : MonoBehaviour {

    public void Notify(string notification) {
        GameObject[] notifAreas = GameObject.FindGameObjectsWithTag("notifArea");
            
        foreach(GameObject go in notifAreas) {
            Text text = go.GetComponent<Text>();
            StartCoroutine(ShowNotify(text, notification));
        }
    }

    private static IEnumerator ShowNotify(Text text, string notification, int duration=3) {
        if(text != null) {
            text.text = notification;
            yield return new WaitForSeconds(3);
        } 
    }
}
