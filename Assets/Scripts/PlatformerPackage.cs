using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerPackage : MonoBehaviour
{
    [Header("Player Package Components")]
    [SerializeField]
    private PlayerFeet feet;
    [SerializeField]
    private IBlockerSensor leftSensor;
    [SerializeField]
    private IBlockerSensor rightSensor;
    [SerializeField]
    private IBlockerSensor ceilingSensor;

    [Header("Jump properties")]
    [SerializeField]
    private float longJumpHeight = 5f;
    [SerializeField]
    private float stoppedJumpHeight = 1f;
    [SerializeField]
    private float gravityAcceleration = 3f;
    [SerializeField]
    private float apexGravityReduction = 0.5f;
    [SerializeField]
    private float apexVelocityDefinition = 2f;
    [SerializeField]
    private float maxFallSpeed = 5f;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private float coyoteTime = 0.25f;
    [SerializeField]
    private float jumpBufferTime = 0.4f;
    [SerializeField]
    private float ceilingKnockback = -1f;

    private bool falling = true;
    private bool holdJumpButton = true;
    private float curFallVelocity = 0f;
    private Coroutine currentJumpBufferSequence = null;

    [Header("Move properties")]
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float airReduction = 0.75f;
    [SerializeField]
    private float maxHorizontalSpeed = 8.5f;
    private bool isMoving = false;
    private float walkingDirection = 0f;
    private bool facingRight = true;

    // The direction that the unit wants to move
    private float inertiaRatio = 0f;
    private Coroutine runningInertiaSequence = null;

    // Wall jump properties
    [Header("Wall Jump Properties")]
    [SerializeField]
    private float wallSlideReduction = 0.4f;
    [SerializeField]
    private float maxWallSlideSpeed = 3f;
    [SerializeField]
    private float wallJumpHorizontalInertia = 1.2f;
    [SerializeField]
    private float wallInertiaEffectDuration = 0.5f;
    [SerializeField]
    private float wallJumpHeight = 6f;
    private IBlockerSensor curGrabbedWall = null;

    // Dash properties
    [Header("Dash Properties")]
    [SerializeField]
    private float dashDistance = 3f;
    [SerializeField]
    private float dashDuration = 0.5f;
    [SerializeField]
    private float timeBetweenDashes = 0.5f;
    [SerializeField]
    private float dashOffset = 0.05f;
    private bool canDash = true;
    private Coroutine runningDashCoroutine;



    // Start is called before the first frame update
    void Awake()
    {
        // Connect events and variables to feet
        if (feet == null) {
            Debug.LogError("NO FEET CONNECTED TO PLAYER PACKAGE");
        }
        feet.landingEvent.AddListener(onFeetLand);
        feet.fallingEvent.AddListener(onFeetFall);
        feet.setCoyoteTime(coyoteTime);

        // Check horizontal sensors and set them up
        if (leftSensor == null || rightSensor == null) {
            Debug.LogError("ONE OF THE HORIZONTAL SENSORS IS NULL");
        }

        // Set current fall velocity to zero
        if (gravityAcceleration <= 0f) {
            Debug.LogError("GRAVITY IS NOT GRATER THAN 0");
        }
        if (longJumpHeight <= 0f || stoppedJumpHeight <= 0f) {
            Debug.LogError("JUMP HEIGHT IS NOT GREATER THAN 0");
        }

        curFallVelocity = 0f;
    }


    // Update is called once per frame
    void Update()
    {
        updateWallGrabState();

        if (runningDashCoroutine == null) {
            handleJump();
            handleMovement();
        }
    }


    // Main private helper function to handle the jumping action per frame
    //  Post: moves the player unit down based on jump
    private void handleJump() {
        // If falling, do falling sequence
        if (falling) {
            // Check celing condition
            if (ceilingSensor.isBlocked()) {
                curFallVelocity = ceilingKnockback;
            }

            // Calculate the gravity being used
            float curGravityReduction = 1f;
            bool atApex = (curFallVelocity > -apexVelocityDefinition) && (curFallVelocity < apexVelocityDefinition);

            if (isGrabbingWall()) {
                curGravityReduction = wallSlideReduction;
            } else if (atApex) {
                curGravityReduction = apexGravityReduction;
            }

            float curGravity = gravityAcceleration * curGravityReduction;

            // Calculate the current velocity within this frame
            curFallVelocity -= (curGravity * Time.deltaTime);
            float curMaxFallSpeed = (isGrabbingWall()) ? maxWallSlideSpeed : maxFallSpeed;
            curFallVelocity = Mathf.Max(-curMaxFallSpeed, curFallVelocity);

            float curDistDelta = curFallVelocity * Time.deltaTime;

            // Cast ray in that direction to check if you hit a floor so you don't glitch through (move to another function)
            Vector3 mainRaycastPos = (curDistDelta > 0f) ? ceilingSensor.transform.position : feet.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(mainRaycastPos, Vector2.up, curDistDelta, collisionMask);
            if (hit.collider != null) {
                float dir = Mathf.Sign(curDistDelta);
                curDistDelta = dir * hit.distance;
            }

            // Actually translate the function
            transform.Translate(curDistDelta * Vector2.up);
        }
    }


    // Main private helper function to handle the act of horizontal movewment per frame
    //  Post: move the player if the player is pressing a button
    private void handleMovement() {
        // Initially apply inertia to the current speed
        float currentSpeed = inertiaRatio * walkSpeed;

        // If you're moving, apply walk speed in inputted direction
        if (isMoving) {
            float inputPlayerSpeed = walkingDirection * walkSpeed;
            inputPlayerSpeed *= (falling) ? airReduction : 1f;
            currentSpeed += inputPlayerSpeed;
        }

        // Clamp the speed to make sure its between maximum horizontal speeds
        currentSpeed = Mathf.Clamp(currentSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);

        // Check Collisions - if you're being blocked by something to stop movement and establish wallGrabState
        IBlockerSensor opposingBlocker = (currentSpeed < 0f) ? leftSensor : rightSensor;
        if (opposingBlocker.isBlocked()) {
            currentSpeed = 0f;
        }

        // Cast ray in that direction to check if you hit a floor so you don't glitch through (move to another function)
        Vector2 rayDir = (currentSpeed < 0f) ? Vector2.left : Vector2.right;
        float curDistDelta = currentSpeed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(opposingBlocker.transform.position, rayDir, curDistDelta, collisionMask);

        if (hit.collider != null) {
            float dir = Mathf.Sign(curDistDelta);
            curDistDelta = dir * hit.distance;
        }

        // Actually apply speed to the player in the game
        transform.Translate(curDistDelta * Vector2.right);
    }


    // Main function to update grab state
    //  Pre: none
    //  Post: update the grab state so that it will be accurate
    private void updateWallGrabState() {
        // If you're not in the air OR curFallVelocity > 0f, set all wall grab states to false
        if (!falling || curFallVelocity > 0f) {
            curGrabbedWall = null;
        }

        // Else if you're currently grabbing a wall
        else if (isGrabbingWall())
        {
            // Check if you're even next to the wall anymore
            if (!curGrabbedWall.isBlocked()) {
                curGrabbedWall = null;
            }

            // Check if you're moving in the opposing direction if you're still grabbing a wall
            if (isGrabbingWall() && isMoving) {
                bool movingOpposingDirection = (curGrabbedWall == leftSensor && walkingDirection > 0f) ||
                                           (curGrabbedWall == rightSensor && walkingDirection < 0f);

                if (movingOpposingDirection) {
                    curGrabbedWall = null;
                }
            }
        }

        // If you're not grabbing a wall, falling in the air, and moving, check if move towards wall
        else if (!isGrabbingWall() && isMoving) {
            IBlockerSensor opposingBlocker = (walkingDirection < 0f) ? leftSensor : rightSensor;
            if (opposingBlocker.isBlocked()) {
                curGrabbedWall = opposingBlocker;
            }
        }

    }


    // Main boolean to check if you're grabbing a wall
    //  Post: returns a bool to check if you're sliding down a wall
    private bool isGrabbingWall() {
        return curGrabbedWall != null;
    }


    // Main event handler function to check when feet has landed
    //  Post: when feet land, stop falling
    private void onFeetLand() {
        curFallVelocity = 0f;
        falling = false;
    }


    // Main event handler function to keep track when feet stopped sensing ground
    //  Post: when feet stopped sensing ground, fall
    private void onFeetFall() {
        falling = true;
    }

    
    // Main event handler function for when jump button has been pressed
    //  Post: when jump button pressed, set velocity to jump velocity
    public void onJumpPress(InputAction.CallbackContext context) {

        // If you started pressing the jump button
        if (context.started) {
            // If you're allowed to jump in the current state, then jump
            if (!falling) {
                falling = true;
                curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, longJumpHeight);

            // If you're grabbing a wall, wall jump with inertia
            } else if (isGrabbingWall()){
                // Apply inertia
                float inertiaMagnitude = wallJumpHorizontalInertia;
                inertiaMagnitude *= (curGrabbedWall == leftSensor) ? 1f : -1f;
                runInertiaSequence(inertiaMagnitude, wallInertiaEffectDuration);

                // Apply the jump
                curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, wallJumpHeight);

            // If not, buffer the jump
            } else {
                if (currentJumpBufferSequence != null) {
                    StopCoroutine(currentJumpBufferSequence);
                }

                currentJumpBufferSequence = StartCoroutine(jumpBufferSequence());
            }

            holdJumpButton = true;
        }

        // If you released the jump button early, set the velocity to jump stop (VARIABLE JUMP HEIGHT)
        else if (context.canceled)
        {
            float jumpStopVelocity = calculateStartingJumpVelocity(-gravityAcceleration, stoppedJumpHeight);
            if (curFallVelocity > jumpStopVelocity) {
                curFallVelocity = jumpStopVelocity;
            }
            
            holdJumpButton = false;
        }
    }


    // Main private sequence function for buffering jump
    //  Pre: a player wants to jump but cannot do so because they're too early
    //  Post: the jump will come if the player lands within the jump buffer time
    private IEnumerator jumpBufferSequence() {
        // Set up
        float jumpBufferTimer = 0.0f;

        // Timer loop
        while (jumpBufferTimer < jumpBufferTime && falling) {
            yield return 0;
            jumpBufferTimer += Time.deltaTime;
        }

        // Allow jump if you landed before the timer ended
        if (jumpBufferTimer < jumpBufferTime) {
            float usedJumpHeight = (holdJumpButton) ? longJumpHeight : stoppedJumpHeight;

            falling = true;
            curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, usedJumpHeight);
        }

        currentJumpBufferSequence = null;   
    }


    // Main event hanler function for when you are walking
    public void onWalkingMoveChange(InputAction.CallbackContext context) {
        // Set flag for when player is pressing a button
        isMoving = !context.canceled;

        // Set inputVector value
        float currentDirection = context.ReadValue<float>();
        walkingDirection = currentDirection;

        if (walkingDirection > 0.01f) {
            facingRight = true;
        } else if (walkingDirection < -0.01f) {
            facingRight = false;
        }
    }


    // Main event handler for when pressing dash
    //  Post: will run dash when button is pressed
    public void onDashPress(InputAction.CallbackContext context) {
        if (context.started) {
            Vector2 dashDir = (facingRight) ? Vector2.right : Vector2.left;
            runDashSequence(dashDir, dashDistance, dashDuration);
        }
    }



    // Wrapper for running dash sequence
    //  Pre: dir is the direction of the dash, dashDist is the position of the dash, dashDuration is how long it will last
    //  Post: run dash sequence if you aren't dashing already
    public void runDashSequence(Vector2 dir, float dashDist, float dashDuration) {
        if (canDash) {
            curFallVelocity = 0f;
            runningDashCoroutine = StartCoroutine(dashSequence(dir, dashDist, dashDuration));
        }
    }


    // Main IEnumerator for dashing
    //  Pre: dir is the direction of the dash, dashDist is the position of the dash, dashDuration is how long it will last
    //  Post: do a running dash sequence
    private IEnumerator dashSequence(Vector2 dir, float dashDist, float dashDuration) {
        Debug.Assert(dashDuration > 0f && dashDist > 0f);

        // Calculate the speed of the actual dash itself
        canDash = false;
        float dashSpeed = dashDist / dashDuration;

        // Cast a boxcast ray to get the actual distance to travel
        dir = dir.normalized;
        float actualDistance = dashDist;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.lossyScale, 0f, dir, dashDist, collisionMask);
        if (hit.collider) {
            actualDistance = hit.distance - dashOffset;
        }

        // Move in that assumed direction
        float curDist = 0f;
        float timer = 0f;

        while (timer < dashDuration) {
            yield return 0;

            // Update time variables
            timer += Time.deltaTime;

            // Update distance variables
            if (curDist < actualDistance) {
                float distDelta = dashSpeed * Time.deltaTime;
                curDist += distDelta;

                // Adjust distance so it hits dest point exactly if it goes over
                distDelta -= (curDist > actualDistance) ? (curDist - actualDistance) : 0f;
                transform.Translate(distDelta * dir);
            }
        }

        // Set coroutine to null
        runningDashCoroutine = null;

        // Wait for a delay before you can dash again
        yield return new WaitForSeconds(timeBetweenDashes);
        canDash = true;
    }



    // Main helper function to run inertia sequence
    //  Pre: intertiaMagnitude is the magnitude the intertia is effected, duration is how long the inertia is interpolated to 0
    //  Post: starts with magnitude intertia and linearly interpolates the inertia 
    private void runInertiaSequence(float inertiaMagnitude, float duration) {
        Debug.Assert(duration >= 0f);

        if (runningInertiaSequence != null) {
            StopCoroutine(runningInertiaSequence);
        }

        runningInertiaSequence = StartCoroutine(intertiaInterpolationSequence(inertiaMagnitude, duration));
    }


    // Main sequence to do intertia interpolation for this unit
    //  Pre: intertiaMagnitude is the magnitude the intertia is effected, duration is how long the inertia is interpolated to 0
    //  Post: starts with magnitude intertia and linearly interpolates the inertia 
    private IEnumerator intertiaInterpolationSequence(float inertiaMagnitude, float duration) {
        Debug.Assert(duration >= 0f);

        // Setup
        float timer = 0f;
        inertiaRatio = inertiaMagnitude;

        // main timer loop
        while (timer < duration) {
            yield return 0;

            timer += Time.deltaTime;
            inertiaRatio = Mathf.Lerp(inertiaMagnitude, 0f, timer / duration);
        }

        // Cleanup
        inertiaRatio = 0f;
        runningInertiaSequence = null;
    }


    // Main private helper function to calculate starting jump velocity given these variables
    //  Pre: acceleration should ALWAYS be negative and jump height should ALWAYS be positive
    //  Post: returns the starting velocity of the jump
    private float calculateStartingJumpVelocity(float acceleration, float jumpHeight) {
        Debug.Assert(acceleration < 0f && jumpHeight > 0f);

        return Mathf.Sqrt(-2f * acceleration * jumpHeight);
    }

}
