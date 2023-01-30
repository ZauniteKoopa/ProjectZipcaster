using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Cannon Properties")]
    [SerializeField]
    [Min(0.01f)]
    private float secondsPerBullet = 0.01f;

    [Header("Projectile Properties")]
    [SerializeField]
    private MovingProjectile cannonBall;
    [SerializeField]
    [Min(0.01f)]
    private float bulletSpeed = 0.01f;


    // On awake, set up cannon
    private void Awake() {
        if (cannonBall == null) {
            Debug.LogError("No cannon ball prefab to clone for this cannon!");
        }

        StartCoroutine(firingSequence());
    }


    // Main firing sequence 
    private IEnumerator firingSequence() {
        while (true) {
            yield return new WaitForSeconds(secondsPerBullet);

            MovingProjectile curProj = Object.Instantiate(cannonBall, transform.position + Vector3.forward, Quaternion.identity);
            curProj.startBullet(bulletSpeed, transform.right);
        }
    }

}
