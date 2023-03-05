using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlatformerPackage : MonoBehaviour
{
    // Unity events for animations
    public UnityEvent platformerLandEvent;
    private PlatformerAudioManager audioManager;

    [Header("Player Package Components")]
    [SerializeField]
    private PlayerFeet feet;
    [SerializeField]
    private IBlockerSensor leftSensor;
    [SerializeField]
    private IBlockerSensor rightSensor;
    [SerializeField]
    private IBlockerSensor ceilingSensor;
    [SerializeField]
    private IPauseMenu pauseMenu;
    public bool isPaused {
        get {return pauseMenu.inPauseState();}
    }
    public bool isAlive = true;

    [Header("Jump properties")]
    [SerializeField]
    [Min(0f)]
    private float longJumpHeight = 3f;
    [SerializeField]
    [Min(0f)]
    private float stoppedJumpHeight = 1f;
    [SerializeField]
    [Min(0f)]
    private float gravityAcceleration = 35f;
    [SerializeField]
    [Range(0.1f, 1f)]
    private float apexGravityReduction = 0.7f;
    [SerializeField]
    [Min(0f)]
    private float apexVelocityDefinition = 1.5f;
    [SerializeField]
    [Min(0f)]
    private float maxFallSpeed = 20f;
    [SerializeField]
    protected LayerMask collisionMask;
    [SerializeField]
    [Min(0f)]
    private float coyoteTime = 0.05f;
    [SerializeField]
    [Min(0f)]
    private float jumpBufferTime = 0.15f;
    [SerializeField]
    [Min(0f)]
    private float ceilingKnockback = 1.3f;
    [SerializeField]
    [Range(0f, 0.075f)]
    private float landingOffset = 0.05f;

    private bool falling = true;
    private bool holdJumpButton = true;
    private float curFallVelocity = 0f;
    private Coroutine currentJumpBufferSequence = null;

    [Header("Move properties")]
    [SerializeField]
    [Min(0f)]
    private float walkSpeed = 8f;
    [SerializeField]
    [Range(0f, 1f)]
    private float airReduction = 0.75f;
    [SerializeField]
    [Min(0f)]
    private float maxHorizontalSpeed = 10f;
    [SerializeField]
    [Range(0f, 1f)]
    private float momentumEaseIn = 0.6f;
    [SerializeField]
    [Range(0f, 0.1f)]
    private float wallOffset = 0.05f;
    private bool isMoving = false;
    private float walkingDirection = 0f;
    private bool facingRight = true;
    public virtual Vector2 forwardDir {
        get {
            if (isGrabbingWall()) {
                return (curGrabbedWall == rightSensor) ? transform.right : -1f * transform.right;
            }

            return (facingRight) ? transform.right : -1f * transform.right;
        }
    }

    // The direction that the unit wants to move
    private float inertiaRatio = 0f;
    private Coroutine runningInertiaSequence = null;
    private float runningInertiaForce = 0f;
    private IWindZone roomWindZone = null;

    // Wall jump properties
    [Header("Wall Jump Properties")]
    [SerializeField]
    [Range(0.01f, 1f)]
    private float wallSlideReduction = 0.4f;
    [SerializeField]
    [Min(0f)]
    private float maxWallSlideSpeed = 3f;
    [SerializeField]
    [Min(0f)]
    private float wallJumpHorizontalInertia = 2.5f;
    [SerializeField]
    [Min(0f)]
    private float wallInertiaEffectDuration = 0.1f;
    [SerializeField]
    [Min(0f)]
    private float wallJumpHeight = 1.5f;
    private IBlockerSensor curGrabbedWall = null;
    private bool climbing = false;

    [Header("Extra Jumps")]
    [SerializeField]
    [Min(0)]
    private int extraJumps = 0;
    [SerializeField]
    private bool extraJumpsCancelsMomentum = true;
    [SerializeField]
    [Min(0.01f)]
    private float extraJumpHeight = 1f;
    private int extraJumpsLeft = 0;

    [Header("Edge Detection")]
    [SerializeField]
    [Range(0f, 1f)]
    private float edgeTransversibleThreshold = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float edgeAdjustmentOffset = 0.1f;

    [Header("Gizmo Display")]
    [SerializeField]
    [Range(0, 2f)]
    private float singleJumpDisplayT = 0.5f;
    [SerializeField]
    [Range(10, 100)]
    private int jumpTrajectoryQuality = 20;
    [SerializeField]
    private bool displayDoubleJump = false;
    [SerializeField]
    private bool showApexDoubleJump = false;
    [SerializeField]
    [Range(0.01f, 2f)]
    private float doubleJumpDisplayT = 0.5f;



    // Accessible elements for animators
    public virtual bool isJumping {
        get {return falling && !isGrabbingWall();}
    }

    public float jumpScaleStatus {
        get { return Mathf.Clamp(Mathf.Abs(curFallVelocity) / calculateStartingJumpVelocity(-gravityAcceleration, longJumpHeight), 0f, 1f);}
    }

    public bool grounded {
        get { return !falling; }
    }

    public float getVerticalSpeed {
        get { return curFallVelocity; }
    }

    public float getHorizontalAxis {
        get { return (!isMoving) ? 0f : walkingDirection; }
    }


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

        leftSensor.blockedStartEvent.AddListener(onLeftSideCollision);
        rightSensor.blockedStartEvent.AddListener(onRightSideCollision);

        // Check horizontal sensors and set them up
        if (leftSensor == null || rightSensor == null) {
            Debug.LogError("ONE OF THE HORIZONTAL SENSORS IS NULL");
        }

        // Check audio
        audioManager = GetComponent<PlatformerAudioManager>();
        if (audioManager == null) {
            Debug.LogWarning("No audio manager found for player. No sound will come out for platformer actions");
        }

        curFallVelocity = 0f;

        // Set extra jumps to jumps left
        extraJumpsLeft = extraJumps;

        initialize();
    }


    // Main function to do initialization
    protected virtual void initialize() {}


    // Update is called once per frame
    void Update()
    {
        updateWallGrabState();
        transform.Translate(new Vector2(handleMovement(), handleJump()));
    }


    // Main private helper function to handle the jumping action per frame
    //  Post: returns the Y direction of the vector
    protected virtual float handleJump() {
        // If falling, do falling sequence
        if (falling) {
            // Check celing condition
            if (ceilingSensor.isBlocked()) {
                curFallVelocity = -ceilingKnockback;
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
            curFallVelocity = Mathf.Max(-getMaxFallSpeed(), curFallVelocity);

            float curDistDelta = curFallVelocity * Time.deltaTime;

            // Cast ray in that direction to check if you hit a floor so you don't glitch through (move to another function)
            Transform verticalCollisionSensor = (curDistDelta > 0f) ? ceilingSensor.transform : feet.transform;
            Vector3 mainBoxcastPos = verticalCollisionSensor.position;
            Vector3 mainBoxcastSize = 0.5f * verticalCollisionSensor.lossyScale;
            
            RaycastHit2D hit = Physics2D.BoxCast(mainBoxcastPos, mainBoxcastSize, 0f, Vector2.up, curDistDelta, collisionMask);
            if (hit.collider != null) {
                float dir = Mathf.Sign(curDistDelta);
                curDistDelta = dir * (hit.distance - 0.02f);
            }

            // Actually translate the function
            return curDistDelta;
        }

        return 0f;
    }


    // Main private helper function to handle the act of horizontal movewment per frame
    //  Post: returns the x direction of the vector
    protected virtual float handleMovement() {
        // Initially apply inertia to the current speed
        float currentSpeed = inertiaRatio * getCurrentWalkingSpeed();
        currentSpeed += (roomWindZone != null) ? roomWindZone.getHorizontalWindSpeed() : 0f;

        // If you're moving, apply walk speed in inputted direction
        if (isMoving) {
            float inputPlayerSpeed = walkingDirection * getCurrentWalkingSpeed();
            inputPlayerSpeed *= (falling) ? airReduction : 1f;
            currentSpeed += inputPlayerSpeed;
        }

        // Clamp the speed to make sure its between maximum horizontal speeds
        currentSpeed = Mathf.Clamp(currentSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);

        // Check Collisions - if you're being blocked by something to stop movement and establish wallGrabState
        IBlockerSensor opposingBlocker = (currentSpeed < 0f) ? leftSensor : rightSensor;
        if (Mathf.Abs(currentSpeed) > 0.1f) {
            edgeAdjustment(currentSpeed > 0f);
        }

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
        return curDistDelta;
    }


    // Main function to check for edge detection given a vector
    //  Pre: a bool representing if it's moving to the right or to the left
    //  Post: will adjust the player's verticle position if it's within edge threshold
    private void edgeAdjustment(bool goingRight) {
        // Create ray cast variables
        Vector2 rayPosOffsetDir = (goingRight) ? Vector2.right : Vector2.left;
        Vector2 rayPos = (Vector2)transform.position + (0.6f * transform.lossyScale.x * rayPosOffsetDir) + (0.5f * transform.lossyScale.y * Vector2.up);
        float rayDist = transform.lossyScale.y * 0.98f;

        // Check that area between player and ray point is not blocked
        RaycastHit2D hit = Physics2D.Raycast(rayPos,
                                             ((Vector2)transform.position - rayPos).normalized,
                                             Vector2.Distance(transform.position, rayPos),
                                             collisionMask);
        if (!hit.collider) {
            hit = Physics2D.Raycast(rayPos, Vector2.down, rayDist, collisionMask);

            // Adjust if there's a collision
            if (hit.collider) {
                float blockerFloorPos = hit.point.y;
                float playerFeetPos = transform.position.y - (0.5f * transform.lossyScale.y);

                bool edgeHigher = blockerFloorPos > playerFeetPos;
                bool edgeInRange = blockerFloorPos - playerFeetPos < (transform.lossyScale.y * edgeTransversibleThreshold);

                if (edgeHigher && edgeInRange) {
                    float adjustMagnitude = (blockerFloorPos + edgeAdjustmentOffset) - playerFeetPos;
                    transform.Translate(adjustMagnitude * Vector2.up);
                }
            }
        }
    }


    // Main function to update grab state
    //  Pre: none
    //  Post: update the grab state so that it will be accurate
    private void updateWallGrabState() {
        // If you're not in the air OR curFallVelocity > 0f, set all wall grab states to false
        if (!falling || curFallVelocity > 0f) {
            audioManager.setWallSlideSound(false);
            curGrabbedWall = null;
            climbing = false;
        }

        // Else if you're currently grabbing a wall
        else if (isGrabbingWall())
        {
            // Check if you're even next to the wall anymore
            if (!curGrabbedWall.isBlocked()) {
                audioManager.setWallSlideSound(false);
                curGrabbedWall = null;
                climbing = false;
            }

            // Check if you're moving in the opposing direction if you're still grabbing a wall
            if (isGrabbingWall() && isMoving) {
                bool movingOpposingDirection = (curGrabbedWall == leftSensor && walkingDirection > 0f) ||
                                           (curGrabbedWall == rightSensor && walkingDirection < 0f);

                if (movingOpposingDirection) {
                    audioManager.setWallSlideSound(false);
                    curGrabbedWall = null;
                    climbing = false;
                }
            }
        }

        // If you're not grabbing a wall, falling in the air, and moving, check if move towards wall
        else if (!isGrabbingWall() && isMoving) {
            IBlockerSensor opposingBlocker = (walkingDirection < 0f) ? leftSensor : rightSensor;
            if (opposingBlocker.isBlocked()) {
                curGrabbedWall = opposingBlocker;
                audioManager.setWallSlideSound(true);
                stopAllMomentum();
            }
        }
    }


    // Main function to grab nearest wall
    //  Pre: none
    //  Post: grabs the wall. Prioritizes the left wall over the right wall. If already grabbing wall, do nothing
    protected void autoGrabWall() {
        StartCoroutine(autoGrabWallSequence());
    }


    // IEnumerator to auto wall grab after a frame to check for the collisions
    private IEnumerator autoGrabWallSequence() {
        for (int i = 0; i < 2; i++) {
            yield return 0;
        }

        if (!isGrabbingWall()) {
            IBlockerSensor touchedBlocker = null;

            // See if you're touching a wall
            if (leftSensor.isBlocked()) {
                touchedBlocker = leftSensor;
            } else if (rightSensor.isBlocked()) {
                touchedBlocker = rightSensor;
            }

            // If you are, grab the wall
            if (touchedBlocker != null) {
                curGrabbedWall = touchedBlocker;
                audioManager.setWallSlideSound(true);
                stopAllMomentum();
            }
        }
    }


    // Main boolean to check if you're grabbing a wall
    //  Post: returns a bool to check if you're sliding down a wall
    public bool isGrabbingWall() {
        return curGrabbedWall != null;
    }


    // Main event handler function to check when feet has landed
    //  Post: when feet land, stop falling
    private void onFeetLand() {
        curFallVelocity = 0f;
        extraJumpsLeft = extraJumps;
        falling = false;

        transform.position = feet.getAutomatedGroundPosition(transform.position, (transform.lossyScale.y * 0.5f) + landingOffset);

        refreshResourcesOnLanding();
        platformerLandEvent.Invoke();
    }


    // Main event function to check when you hit the left side
    //  Post: when you hit the left side, offset from that collision
    private void onLeftSideCollision() {
        Vector3 mainBoxcastPos = transform.position - (0.1f * Vector3.left);
        Vector3 mainBoxcastSize = transform.lossyScale * 0.95f;
        float distDelta = transform.lossyScale.x * 0.3f;
        
        RaycastHit2D hit = Physics2D.BoxCast(mainBoxcastPos, mainBoxcastSize, 0f, Vector2.left, distDelta, collisionMask);
        if (hit.collider != null) {
            float adjustedXPos = hit.point.x + (transform.lossyScale.x / 2f + wallOffset);
            transform.position = new Vector3(adjustedXPos, transform.position.y, 0f); 
        }
    }


    // Main event function to check when you hit the left side
    //  Post: when you hit the left side, offset from that collision
    private void onRightSideCollision() {
        Vector3 mainBoxcastPos = transform.position - (0.1f * Vector3.right);
        Vector3 mainBoxcastSize = transform.lossyScale * 0.95f;
        float distDelta = transform.lossyScale.x * 0.3f;
        
        RaycastHit2D hit = Physics2D.BoxCast(mainBoxcastPos, mainBoxcastSize, 0f, Vector2.right, distDelta, collisionMask);
        if (hit.collider != null) {
            float adjustedXPos = hit.point.x + ((transform.lossyScale.x / 2f + wallOffset) * -1f);
            transform.position = new Vector3(adjustedXPos, transform.position.y, 0f);   
        }
    }


    // Refresh resources on landing
    //  Pre: unit has landed
    //  Post: refresh any resources that the specific package variant may need
    protected virtual void refreshResourcesOnLanding() {}


    // Main event handler function to keep track when feet stopped sensing ground
    //  Post: when feet stopped sensing ground, fall
    private void onFeetFall() {
        falling = true;
    }

    
    // Main event handler function for when jump button has been pressed
    //  Post: when jump button pressed, set velocity to jump velocity
    public virtual void onJumpPress(InputAction.CallbackContext context) {

        // If you started pressing the jump button
        if (context.started && !isPaused) {
            // If you're allowed to jump in the current state, then jump
            if (!falling) {
                falling = true;
                audioManager.playJumpSound();
                curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, longJumpHeight);

            // If you're grabbing a wall, wall jump with inertia
            } else if (isGrabbingWall()){
                // Apply inertia
                float inertiaMagnitude = wallJumpHorizontalInertia;
                inertiaMagnitude *= (curGrabbedWall == leftSensor) ? 1f : -1f;
                runInertiaSequence(inertiaMagnitude, wallInertiaEffectDuration);

                // Apply the jump
                audioManager.playJumpSound();
                curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, wallJumpHeight);

            // If you're in the air and you have extra jumps
            } else if (extraJumpsLeft > 0) {
                extraJumpsLeft--;
                audioManager.playJumpSound();
                curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, extraJumpHeight);

                if (extraJumpsCancelsMomentum) {
                    stopAllMomentum();
                }

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
            audioManager.playJumpSound();
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


    // Main event hanler function for when you pause the game
    public void onPauseMenuPress(InputAction.CallbackContext context) {
        if (context.started && pauseMenu != null) {
            pauseMenu.onPauseButtonPress();
        }
    }


    // Main event hanler function for when you are climbing
    public void onClimbPress(InputAction.CallbackContext context) {
        if (context.started && isGrabbingWall()) {
            climbing = true;
            
        } else if (context.canceled) {
            climbing = false;

        }
    }



    // Main helper function to run inertia sequence
    //  Pre: intertiaMagnitude is the magnitude the intertia is effected, duration is how long the inertia is interpolated to 0
    //  Post: starts with magnitude intertia and linearly interpolates the inertia 
    public void runInertiaSequence(float inertiaMagnitude, float duration) {
        Debug.Assert(duration >= 0f);

        if (runningInertiaSequence != null) {
            StopCoroutine(runningInertiaSequence);
        }

        runningInertiaSequence = StartCoroutine(intertiaInterpolationSequence(inertiaMagnitude, duration));
    }


    // Main helper function to run inertia sequence
    //  Pre: effectedSpeed > 0 and duration > 0
    //  Post: starts with magnitude intertia and linearly interpolates the inertia 
    public void runInertiaSequence(float forceSpeed, float duration, bool rightDir) {
        Debug.Assert(duration >= 0f && forceSpeed >= 0f);

        float iRatio = forceSpeed / walkSpeed;
        iRatio *= (rightDir) ? 1f : -1f;
        runInertiaSequence(iRatio, duration);
    }


    // Main function to set constant inertia
    //  Pre: player speed >= 0
    //  Post: inertia will be set to this. THE ONUS IS ON THE SETTER TO RESET IF THE PLAYER DOESN'T DIE
    public void setConstantHorizontalForce(float forceSpeed, bool rightDir) {
        Debug.Assert(forceSpeed >= 0f);
        float iRatio = forceSpeed / walkSpeed;
        iRatio *= (rightDir) ? 1f : -1f;

        // Stop inertia fade out if there's one running
        if (runningInertiaSequence != null) {
            StopCoroutine(runningInertiaSequence);
        }

        runningInertiaForce = iRatio;
        inertiaRatio = iRatio;
    }


    // Main function to reapply horizontal force that is currently running on this package
    //  Post: reapplies force if there is any
    protected void reapplyRunningHorizontalForce() {
        inertiaRatio = runningInertiaForce;
    }


    // Main sequence to do intertia interpolation for this unit
    //  Pre: intertiaMagnitude is the magnitude the intertia is effected, duration is how long the inertia is interpolated to 0
    //  Post: starts with magnitude intertia and linearly interpolates the inertia 
    private IEnumerator intertiaInterpolationSequence(float inertiaMagnitude, float duration) {
        Debug.Assert(duration >= 0f);

        // Setup
        float timer = 0f;
        float actualInertiaEaseIn = momentumEaseIn * duration;
        inertiaRatio = inertiaMagnitude;

        // Wait for a percentage of time stuck at this inertia magnitude
        float intertiaStayingDuration = duration - actualInertiaEaseIn;
        yield return new WaitForSeconds(intertiaStayingDuration);


        // main timer loop
        while (timer < actualInertiaEaseIn) {
            yield return 0;

            timer += Time.deltaTime;
            inertiaRatio = Mathf.Lerp(inertiaMagnitude, 0f, timer / actualInertiaEaseIn);
        }

        // Cleanup
        inertiaRatio = 0f;
        runningInertiaSequence = null;
    }


    // Main function to adjust movement vector based on the state of the colliders
    //  Pre: Vector2 is a normalized movement vector to be adjusted based on collision state
    //  Post: Vector2 is adjusted to consider the collision so that it will slide instead of being stuck
    protected Vector2 adjustMovementForCollision(Vector2 movementVector) {
        movementVector = movementVector.normalized;

        // Handle X variable
        float minX = (leftSensor.isBlocked()) ? 0f : -1f;
        float maxX = (rightSensor.isBlocked()) ? 0f : 1f;

        // Handle Y variable
        float minY = (grounded) ? 0f : -1f;
        float maxY = (ceilingSensor.isBlocked()) ? 0f : 1f;

        return new Vector2(Mathf.Clamp(movementVector.x, minX, maxX),
                           Mathf.Clamp(movementVector.y, minY, maxY));
    }



    // Main private helper function to stop all vertical velocity
    //  Pre: none
    //  Post: stops all vertical velocity
    protected void stopVerticalVelocity() {
        curFallVelocity = 0f;
    }


    // Main private helper function to launch the player vertically
    //  Pre: launchSpeed is the launch speed to launch the player upwards, if negative downwards
    //  Post: launch the player vertically
    public void launchVertically(float launchHeight) {
        curFallVelocity = calculateStartingJumpVelocity(-gravityAcceleration, launchHeight);
    }


    // Main private helper function to launch the player vertically
    //  Pre: launchSpeed is the launch speed to launch the player upwards, if negative downwards
    //  Post: launch the player vertically
    public void launchVerticallySpeed(float launchSpeed) {
        if (launchSpeed > 0f && !falling) {
            falling = true;
        }

        curFallVelocity = launchSpeed;
    }


    // Main private helper function to stop all momentum
    //  Pre: none
    //  Post: stops all momentum and sets the inertia is 0
    protected void stopAllMomentum() {
        if (runningInertiaSequence != null) {
            StopCoroutine(runningInertiaSequence);
            runningInertiaSequence = null;
        }

        inertiaRatio = 0f;
    }


    // Main private helper function to calculate starting jump velocity given these variables
    //  Pre: acceleration should ALWAYS be negative and jump height should ALWAYS be positive
    //  Post: returns the starting velocity of the jump
    private float calculateStartingJumpVelocity(float acceleration, float jumpHeight) {
        Debug.Assert(acceleration < 0f && jumpHeight >= 0f);

        return Mathf.Sqrt(-2f * acceleration * jumpHeight);
    }


    // Private helper function to calculate the max fall speed for this current frame
    //  Pre: none
    //  Post: returns a non-negative float representing the max fall speed of this frame
    protected virtual float getMaxFallSpeed() {
        if (isGrabbingWall()) {
            return (climbing) ? 0f : maxWallSlideSpeed;
        } else {
            return maxFallSpeed;
        }
    }


    // Private helper function to get current move speed at current frame
    //  Pre: none
    //  Post: returns a non-negative float representing the max waking speed
    protected virtual float getCurrentWalkingSpeed() {
        return walkSpeed;
    }


    // Main function to set the wind zone, overriding this wind zone
    //  Pre: none
    //  Post: if null, player is not affected by wind. Else, player will be effected by wind from the wind zone
    //          also starts the wind zone if its null and stop the wind zone the player was connected to before if it exists
    public void setWindZone(IWindZone weather) {
        // Check if the room zone input is not the same as current so that we don't constantly reset it
        if (weather != roomWindZone) {
            // Stop old wind zone
            if (roomWindZone != null) {
                roomWindZone.stopWindZone();
            } 

            // Set
            roomWindZone = weather;

            // Start new wind zone
            if (roomWindZone != null) {
                roomWindZone.startWindZone();
            }
        } 
    }


    // Main function to completely reset platformer package
    //  Pre: none
    //  Post: stops all running coroutines in this function
    public virtual void reset() {
        stopAllMomentum();
        runningInertiaForce = 0f;

        if (currentJumpBufferSequence != null) {
            StopCoroutine(currentJumpBufferSequence);
        }
        currentJumpBufferSequence = null;
    }



    // --------------------------------------------------------------------
    //    Below is for gizmo drawing to help level editors
    // --------------------------------------------------------------------


    // Main function to draw gizmos for this platformer package when selected
    private void OnDrawGizmosSelected() {
        // Display jump
        Gizmos.color = Color.blue;
        float maxJumpVelocity = calculateStartingJumpVelocity(-gravityAcceleration, longJumpHeight);
        drawJumpTrajectory(jumpTrajectoryQuality, maxJumpVelocity, transform.position, singleJumpDisplayT);

        // Display double jump
        if (displayDoubleJump) {
            Gizmos.color = Color.magenta;

            // Calculate velocity, t, and position
            float doubleJumpVelocity = calculateStartingJumpVelocity(-gravityAcceleration, extraJumpHeight);
            float actualDoubleJumpT = (showApexDoubleJump) ? getApexT(maxJumpVelocity) : doubleJumpDisplayT;
            Vector3 doubleJumpPosR = new Vector3(calculateMaxJumpX(actualDoubleJumpT), calculateMaxJumpY(actualDoubleJumpT, maxJumpVelocity));
            Vector3 doubleJumpPosL = new Vector3(-doubleJumpPosR.x, doubleJumpPosR.y) + transform.position;
            doubleJumpPosR += transform.position;

            // draw double jump on both sides
            drawJumpTrajectory(jumpTrajectoryQuality, doubleJumpVelocity, doubleJumpPosR, singleJumpDisplayT);
            drawJumpTrajectory(jumpTrajectoryQuality, doubleJumpVelocity, doubleJumpPosL, singleJumpDisplayT);
        }
    }


    // Main function to draw pre-apex jump trajectory
    //  Pre: numLines > 0, represent the number of lines to draw this section
    //  Post: returns the point in which this pre-apex jump trajectory ends if the jump started at origin
    private void drawJumpTrajectory(int numLines, float startingJumpSpeed, Vector3 startingPosition, float displayT) {
        Debug.Assert(numLines > 0);

        // Calculate at what time the apex starts and how mch time is in a given line
        float deltaT = displayT / (float)(numLines);

        // Draw the lines
        for (int i = 0; i < numLines; i++) {
            // Get the initial point
            float initialT = i * deltaT;
            Vector3 initialPoint = new Vector3(calculateMaxJumpX(initialT), calculateMaxJumpY(initialT, startingJumpSpeed));
            Vector3 initialPoint2 = new Vector3(-initialPoint.x, initialPoint.y);
            initialPoint += startingPosition;
            initialPoint2 += startingPosition;

            // Get the final point
            float finalT = (i + 1) * deltaT;
            Vector3 finalPoint = new Vector3(calculateMaxJumpX(finalT), calculateMaxJumpY(finalT, startingJumpSpeed));
            Vector3 finalPoint2 = new Vector3(-finalPoint.x, finalPoint.y);
            finalPoint += startingPosition;
            finalPoint2 += startingPosition;

            // Draw gizmo
            Gizmos.DrawLine(initialPoint, finalPoint);
            Gizmos.DrawLine(initialPoint2, finalPoint2);

        }
    }


    // Main helper function for calculating jump trajectory x
    //  Pre: t represent the inputted time to calculate
    //  Post: returns the max horizontal distance reached 
    private float calculateMaxJumpX(float t) {
        return airReduction * walkSpeed * t;
    }


    // Main helper function for calculating jump trajectory y
    //  Pre: t represent the inputted time to calculate
    //       g represents the downward acceleration applied to player (non-negative)
    //       initialV reprepresent the initial velocity of this function
    //  Post: returns the max vertical height reached at time T
    private float calculateMaxJumpY(float t, float initialV) {
        float apexStartT = (initialV - apexVelocityDefinition) / gravityAcceleration;
        float apexEndT = ((2 * apexVelocityDefinition) / (apexGravityReduction * gravityAcceleration)) + apexStartT;
        float g = (t > apexStartT && t <= apexEndT) ? gravityAcceleration * apexGravityReduction : gravityAcceleration;
        float initialY = 0f;

        // Pre apex: do nothing, your variables are fine

        // Post apex
        if (t > apexEndT) {
            t -= apexEndT;
            initialY = calculateMaxJumpY(apexEndT, initialV);
            initialV = -apexVelocityDefinition;

        // In apex
        } else if (t > apexStartT) {
            t -= apexStartT;
            initialY = calculateMaxJumpY(apexStartT, initialV);
            initialV = apexVelocityDefinition;
        }


        return (initialV * t) - (0.5f * g * t * t) + initialY;
    }


    // Main private helper function to get the apex t
    private float getApexT(float initialV) {
        float apexStartT = (initialV - apexVelocityDefinition) / gravityAcceleration;
        float apexEndT = ((2 * apexVelocityDefinition) / (apexGravityReduction * gravityAcceleration)) + apexStartT;

        return (apexStartT + apexEndT) / 2f;
    }
}
