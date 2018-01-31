using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Notifier : MonoBehaviour {

    public const string NAME = "Notifier";

    private Text notifArea;
    private CanvasGroup canvasGroup;

    private void Start() {

        canvasGroup = GetComponentInParent<CanvasGroup>();
        if(canvasGroup == null) {
            Debug.Log("Error: Notifier requires a canvasgroup parent");
        }

        canvasGroup.alpha = 0;
    }

    public void Notify(string notification, int duration=3) {
        // GameObject notifHolder = GetComponentInChildren<GameObject>();
        Text notifArea = GetComponentInChildren<Text>();
        canvasGroup.alpha = 1;
        StartCoroutine(ShowNotify(notifArea, notification, duration));
    }

    private IEnumerator ShowNotify(Text text, string notification, int duration) {
        if (text != null) {
            text.text = notification;
        }

        // wait forever if duration == -1
        if (duration != -1) {
            yield return new WaitForSeconds(duration);
            canvasGroup.alpha = 0;
        }
    }
}
