using UnityEngine;
using System.Collections;

// If the player is within a small radius, the enemy hears them
public class NearbyPlayerEars : EnemyEars {

    protected override float getRadius() {
        return 3f;
    }

    protected override void Hear(GameObject obj) {
        // The enemy has a 'hearing radius'. If the player passes through this circle, the enemy is alerted.

        Vector2? sourceOfNoise = null;
        Player player = obj.GetComponent<Player>();
        if (player != null) {

            // Can't hear through walls
            LayerMask layerMask = -1;
            int mask = 1 << 2;
            layerMask.value &= ~mask;
            mask = 1 << 8;
            layerMask.value &= ~mask;
            RaycastHit2D castHit = Physics2D.Raycast(owner.transform.position, obj.transform.position);// Mathf.Infinity, layerMask);

            if (castHit.transform != null && castHit.collider.gameObject.CompareTag("Player")) {
                // Send the enemy after the player!
                sourceOfNoise = player.transform.position;
            }

        }

        if (sourceOfNoise != null) {
            Debug.Log("SourceOfNoise: " + sourceOfNoise);
            owner.HearNoise(new Enemy.PlayerLocation((Vector2)sourceOfNoise, 1));
        }
    }
}
