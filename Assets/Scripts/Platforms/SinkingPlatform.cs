using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingPlatform : IDynamicPlatform
{
    // Static serialized variables
    [SerializeField]
    private Vector2 floatLocalPosition;
    [SerializeField]
    private Vector2 sinkLocalPosition;
    [SerializeField]
    [Min(0.01f)]
    private float sinkSpeed = 2f;
    [SerializeField]
    [Min(0.01f)]
    private float floatSpeed = 2f;
    [SerializeField]
    [Min(1)]
    private int sinkThreshold = 1;

    // Runtime instance variables
    private Vector2 floatPosition;
    private Vector2 sinkPosition;
    private float maxSinkDistance;
    private Vector2 sinkDirection;
    private int numEntitiesLanded = 0;
    private float curDistance = 0f;


    // On awake, calculate initial runtime variables
    private void Awake() {
        floatPosition = transform.parent.TransformPoint(floatLocalPosition);
        sinkPosition = transform.parent.TransformPoint(sinkLocalPosition);
        maxSinkDistance = Vector2.Distance(floatPosition, sinkPosition);
        sinkDirection = (sinkPosition - floatPosition).normalized;

        transform.position = floatPosition;
    }


    // On update, move the platform accordingly
    private void Update() {
        // Case where it's floating
        if (numEntitiesLanded < sinkThreshold && curDistance > 0f) {
            // Calculate and update distances
            float prevDistance = curDistance;
            float distDelta = Time.deltaTime * floatSpeed;
            curDistance = Mathf.Clamp(curDistance - distDelta, 0f, maxSinkDistance);

            // translate
            Vector2 distVector = (curDistance - prevDistance) * sinkDirection;
            transform.Translate(distVector);
        }

        // Case where it's sinking
        else if (numEntitiesLanded >= sinkThreshold && curDistance < maxSinkDistance) {
            // Calculate and update distances
            float prevDistance = curDistance;
            float distDelta = Time.deltaTime * sinkSpeed;
            curDistance = Mathf.Clamp(curDistance + distDelta, 0f, maxSinkDistance);

            // translate
            Vector2 distVector = (curDistance - prevDistance) * sinkDirection;
            transform.Translate(distVector);
        }
    }


    // Main function to handle the case in which an entity lands on this platform
    //  Pre: entity doesn't equal null
    //  Post: attach entity to this moving platform
    public override void onEntityLand(Transform entity) {
        entity.parent = transform;
        numEntitiesLanded++;
    }


    // Main function to handle the case in which an entity leaves the platform
    //  Pre: entity doesn't equal null
    //  Post: detach entity if entity was attached to this platform
    public override void onEntityLeave(Transform entity) {
        if (entity.parent == transform) {
            entity.parent = null;
        }

        numEntitiesLanded -= (numEntitiesLanded == 0) ? 0 : 1;
    }

}
