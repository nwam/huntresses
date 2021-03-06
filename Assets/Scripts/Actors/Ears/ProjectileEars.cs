﻿using UnityEngine;
using System.Collections;

// Listens for nearby Bullets
public class ProjectileEars : EnemyEars {

    protected override void Hear(GameObject obj) {
        // The enemy has a 'hearing radius'. If the player or a projectile passes through this circle, the enemy is alerted.

        Vector2? sourceOfNoise = null;
        Bullet bullet = obj.GetComponent<Bullet>();
        if (bullet != null && bullet.IsPlayerBullet()) {
            // The enemy is able to determine where the bullet was fired from from the bullet's rotation.
            Vector2 bulletSourceDirection = -bullet.transform.up;
            // Debug.Log("Shot from " + bulletSourceDirection);

            // We'll send the enemy to the location of the first thing the ray hits.
            RaycastHit2D hit = Physics2D.Raycast(bullet.transform.position, bulletSourceDirection);
            sourceOfNoise = hit.point;
            // Debug.Log("Hit point " + sourceOfNoise);
            Debug.DrawRay(bullet.transform.position, (Vector3)sourceOfNoise);
            // Debug.Break();
        }

        if (sourceOfNoise != null) {
            // Debug.Log("SourceOfNoise: " + sourceOfNoise);
            owner.HearNoise(new Enemy.PlayerLocation((Vector2)sourceOfNoise, 1));
        }
    }
}
