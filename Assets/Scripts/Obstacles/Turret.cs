using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class Turret : MonoBehaviour
{
    [Header("Turret Properties")]
    [SerializeField]
    private EnemySensor2D enemySensor;
    [SerializeField]
    [Min(0.01f)]
    private float initialAggroStart = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float anticipation = 0.01f;
    [SerializeField]
    [Min(0.01f)]
    private float secondsBetweenShots = 0.01f;
    [SerializeField]
    private Color anticipationColor = Color.black;
    [SerializeField]
    private Color firedColor = Color.black;
    private SpriteRenderer render;

    [Header("Projectile Properties")]
    [SerializeField]
    private MovingProjectile cannonBall;
    [SerializeField]
    [Min(0.01f)]
    private float bulletSpeed = 0.01f;


    // Start is called before the first frame update
    void Awake()
    {
        if (enemySensor == null) {
            Debug.LogError("No enemy sensor connected to turret! Can't sense player!");
        }
        enemySensor.sensePlayerEvent.AddListener(onPlayerSense);
        enemySensor.losePlayerEvent.AddListener(onPlayerLost);

        if (cannonBall == null) {
            Debug.LogError("No cannonball prefab connected to turret! Cant fire anything!");
        }

        render = GetComponent<SpriteRenderer>();
        if (render == null) {
            Debug.LogError("No sprite renderer component on turret!");
        }
    }


    // Main aggressive sequence
    private IEnumerator aggroSequence(Transform tgt) {
        Debug.Assert(tgt != null);

        // Initial start period
        transform.right = (tgt.position - transform.position).normalized;
        yield return new WaitForSeconds(initialAggroStart);

        while (true) {
            // Anticipation
            transform.right = (tgt.position - transform.position).normalized;
            render.color = anticipationColor;
            yield return new WaitForSeconds(anticipation);

            // Fire and wait
            MovingProjectile curProj = Object.Instantiate(cannonBall, transform.position + Vector3.forward, Quaternion.identity);
            curProj.startBullet(bulletSpeed, transform.right);
            render.color = firedColor;
            yield return new WaitForSeconds(secondsBetweenShots);
        }
    }

    
    // On player sense, start aggressive sequence
    private void onPlayerSense() {
        Transform target = enemySensor.getTarget().transform;
        Debug.Assert(target != null);

        StartCoroutine(aggroSequence(target));
    }


    // On player loss, end aggressive sequence
    private void onPlayerLost() {
        render.color = firedColor;
        StopAllCoroutines();
    }
}
