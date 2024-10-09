using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PathCreation;

[RequireComponent(typeof(CharacterController))]

public class PC_FPSController : MonoBehaviour
{

    private static PC_FPSController instance = null;
    public static PC_FPSController Instance { get { return instance; } }

    public PathCreator pathCreator;

    //PROBLEM: This UI stuff needs fixed up
    [Space]
    [Header("HUD Settings")]
    public DamageIndicatorHandler ourDamageIndicator; //Really this should go through a UI handler, but for the moment...
    public Image FollowIndicator; //Terrible form here too...
    public GameObject DeadIndicator;

    public float PlayerLeadTime = 3f;

    bool bClimbing = false;
    [Space]
    [Header("Player Speed Settings")]
    public Animator playerAnimator;
    public GameObject characterPosition;
    public float slowSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float boostSpeed = 15f;

    public float strafeSpeed = 9f;

    public float climbSpeed = 5f;

    public float jumpSpeed = 8.0f;
    float kickMomentum = 10.0f;
    public float gravity = 20.0f;

    float targetRunType = 0.5f;
    float runType = 0.5f;
    Range RunScaleRange = new Range(0.8f, 1.3f); //So that our movement speeds scale

    //A quick little bit of testing for our raycast colliders
    float trickRayDist = 1f; //How far out do we test for raycasts in our world for the different tricks we'll be doing?

    //Details for mantling
    float mantleGrabHeight = 1.5f; //Above our zero
    float mantleGrabDepth = 1f; //From in front of our character


    public LayerMask worldRaycastMask;  //So we don't do tricks against the wrong thing
    [Space]
    [Header("Camera Controls")]
    public Camera playerCamera;
    public GameObject eyePosition;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 30f;
    public float lookYLimit = 45f;
    public enum enPCMoveState { NULL, RUNNING, AIRBOURNE, WALLKICK, MANTLE, WALLRUN };
    public enPCMoveState PC_MoveState;

    public enPCMoveState PC_startingState;
    //Movement state factory setup
    private PC_BaseState currentState;
    private PC_MoveStateFactory states;
    public PC_BaseState CurrentState { get { return currentState; } set { currentState = value; } }

    CharacterController characterController;
    #region BoostTriggerDetails
    //We need something to give us speed boosts
    [HideInInspector]
    public float boostTime = 0;         //How much is left on our boost?
    [HideInInspector]
    public float jumpReleaseTime = 0;   //What time did we release jump?
    [HideInInspector]
    public float boostTriggerTime = 0;  //This will be set from one of our states and compared against the jump release time
    [HideInInspector]
    public bool bBoostTriggerReady = false; //Have we had a boost set from our states? This is just a bit of overprogramming
    [HideInInspector]
    public float boostTriggerDuration = 0f; //How long does our boost trigger go for?
    public Range boostTriggerThreshold = new Range(-0.5f, 1.5f);  //Will need to adjust this to suit, and the values returned can be a little funny... :)
    #endregion

    #region StumbleDetails
    //Handler components for our "stumble" which will affect us if we're hit, or trip over something
    public float stumbleTime = 0;
    public float stumbleMax = 1.5f; //This'll possibly be variable? I dunno
    public AnimationCurve stumbleRecoveryCurve;
    public float stumbleSpeedPenalty = 0.5f; //A multiplier that's compared against the stumble recovery curve to dictate our recovery
    #endregion

    Vector3 moveDirection = Vector3.forward;
    float rotationX = 0, rotationY = 0;

    //A few little extra values to help with the sense of momentium
    float SideMomentum = 0;
    [HideInInspector]
    public float WallRunBias = 0; //Basically this value goes from -1 to 1 and increases while we're doing a wall run, or if we do a kick, thus dropping us out of a wall run or preventing wall hopping
    [HideInInspector]
    public Vector3 LastWallNormal = Vector3.zero;
    [HideInInspector]
    public Vector3 WallHitNormal = Vector3.zero;
    public TextMeshProUGUI StateDisplay;
    #region RunSetValues
    protected Vector3 _mantlePoint;
    public Vector3 GetSetMantlePoint { get { return _mantlePoint; }}
    public float GetColliderHeight {  get { return characterController.height;  } }
    #endregion

    [HideInInspector]
    public bool canMove = true; //Can't remember what this was for...

    //Some stored details
    float controllerHeight;
    Vector3 cameraStartPosition;
    Vector3 characterPositionStart;

    float targetHeightScale = 1f;
    public float TargetHeightScale
    {
        get { return targetHeightScale; }
        set { targetHeightScale = value; }
    }
    float heightScale = 1f;
    float heightScaleSpeed = 5f;

    bool bPlayerDead = false;
    public bool bPlayerInvincible = false;

    //Details that are used to calculate where we are along our path:
    [HideInInspector]
    public float bestDistance = 0;
    float bestDistanceSpan = 0; //How far are we away from the center of the path?
    float bestTime = 0;
    float moveSpeed = 0; //Stored here so that we can used it without having to do a square distance on a prior position
    float flatMoveDistance= 0; //How much have we moved excluding our vertical?
    float movedDistance = 0; // How far did we actually move this tick?
    Vector3 priorPosition = Vector3.zero;
    Vector3 priorForward = Vector3.forward; //What was our former forward vector?


    void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate Player in the scene");
			Debug.Log(gameObject.name);
			return; //cancel this
		}

		instance = this;
	}

    Vector3 StartPosition = Vector3.zero;
    void Start()
    {
        states = new PC_MoveStateFactory(this);
        // currentState = states.EnemyNullState();
        Func<PC_BaseState>[] allStates = new Func<PC_BaseState>[] { states.PCNullState, states.PCRunState, states.PCAirbourne, states.PCWallKick, states.PCMantleState, states.PCWallRunState };
        currentState = allStates[(int)PC_startingState]();
        currentState.EnterState();

        characterController = GetComponent<CharacterController>();
        controllerHeight = characterController.height;
        cameraStartPosition = playerCamera.transform.localPosition;
        characterPositionStart = characterPosition.transform.localPosition;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartPosition = gameObject.transform.position;

        //Handle our curve position
        bestTime = pathCreator.path.GetClosestTimeOnPath(gameObject.transform.position);  //This is actually well optimised...
        bestDistance = pathCreator.path.GetClosestDistanceAlongPath(gameObject.transform.position);
        priorPosition = gameObject.transform.position;
    }

    string currentAnimation = "";
    public void setCurrentAnimation(string animName)
    {
        if (currentAnimation != animName)
        {
            currentAnimation = animName;
            playerAnimator.CrossFade(animName, 0.25f, 0);
        }
    }

    //because there are a couple of different runs this is important for sending calls through as necessary. For the moment we're just going to be dumb
    public void checkRunAnim()
    {
        setCurrentAnimation("RunBlends");
    }


    #region InputMethodsForFSM
    public bool bJumpPressed()
    {
#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Space);
#else
        return Input.GetButtonDown("Left Shoulder");
#endif
        //return Input.GetButtonDown("Jump");
    }

    public bool bJumpHeld()
    {
#if UNITY_EDITOR
        return Input.GetKey(KeyCode.Space);
#else
        return Input.GetButton("Left Shoulder");
#endif
    }

    //Basically this is a little watcher to see when the player released the jump button and put a timestamp on it. There's probably better ways of doing this, but for the moment lets do this
    public void GetJumpReleaseTime()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //Debug.Log("Jump Released");
            jumpReleaseTime = Time.time;
        }
#else
        if (Input.GetButtonUp("Left Shoulder")) {
            jumpReleaseTime = Time.time;
        }
#endif
    }

    public bool bAddEffort()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }
    #endregion

    #region CharacterMoveFunctions

    public void SetHeightScale(float toThis)
    {
        characterController.height = controllerHeight * toThis;
        playerCamera.transform.localPosition = Vector3.Lerp(Vector3.zero, cameraStartPosition, toThis);
        characterPosition.transform.localPosition = Vector3.Lerp(Vector3.zero, characterPositionStart, toThis);
    }

    public void HandleControllerScale()
    {
        heightScale = Mathf.Lerp(heightScale, targetHeightScale, Time.deltaTime * heightScaleSpeed);
        SetHeightScale(heightScale);
    }

    public void HandleMomentumControl()
    {
        SideMomentum = Mathf.Lerp(SideMomentum, 0, Time.deltaTime); //We may need to set a decay value for this somewhere
    }

    public float GetStumbleValue() {
        stumbleTime -= Time.deltaTime;
        stumbleTime = Mathf.Clamp(stumbleTime, 0f, stumbleMax);
        return Mathf.Lerp(stumbleSpeedPenalty, 1f, stumbleRecoveryCurve.Evaluate(stumbleTime/stumbleMax));
    }

    //PROBLEM: This can't be hammered every frame as it causes a massive drop in FPS. The "find closest time on path" is fine on PC but it drills the Vita FPS down like a bitch
    public Vector3 getForwardDirection()
    {
        if (!pathCreator)
        {
            Debug.LogError("No assigned path on the player controller!");
            return Vector3.forward;
        }

        //So I think I need a new approach. We'll get the time to kick off with, and then go off of distance with a guess based off of how fast we're travelling, and a bit of wriggle ahead/behind then take the closest as gospel
        float distanceGuess = bestDistance + flatMoveDistance;// * Time.deltaTime;
        bestDistance = distanceGuess;
        bestDistanceSpan = Vector3.SqrMagnitude(gameObject.transform.position-pathCreator.path.GetPointAtDistance(distanceGuess));

        float distanceRange = moveSpeed * Time.deltaTime *0.5f; //how far we'll shift with each check. This should be self-correcting
        for (int i = -1; i<2; i++)
        {
            if (i != 0) //We've already got this point
            { 
                float offsetDistance = distanceGuess + i * distanceRange;
                float newDistanceSpan = Vector3.SqrMagnitude(gameObject.transform.position - pathCreator.path.GetPointAtDistance(offsetDistance));
                if (newDistanceSpan < bestDistanceSpan) //If we're closer to the line reset our best settings
                {
                    bestDistance = offsetDistance;
                    bestDistanceSpan = newDistanceSpan;
                }
            }
        }

        //Lets see how good the above actually is
        //Debug.Log("Distance Guess: " + (bestDistance - pathCreator.path.GetClosestDistanceAlongPath(gameObject.transform.position)));

        //we really only  need the path normal for our heading
        Vector3 pathHeading = pathCreator.path.GetDirectionAtDistance(bestDistance); // .GetDirection(bestTime); // (distance, EndOfPathInstruction.Stop);
        return pathHeading; //We assume that this is forward
    }
    
    public void DoFlatMove()
    {
        if (bPlayerDead) {return;}
        //PROBLEM: This will need to be replaced with a curve sample for our direction
        Vector3 forward = getForwardDirection(); // transform.TransformDirection(Vector3.forward);
        Vector3 right = Quaternion.AngleAxis(90f, Vector3.up)*forward; //transform.TransformDirection(Vector3.right);

        bool addEffort = bAddEffort();
        boostTime -= Time.deltaTime;    //PROBLEM: This needs refactoring - Tick our boost speed down
        float calcMoveSpeed = boostTime > 0 ? boostSpeed : runningSpeed;    //Take into account our boost but still allow for slowing our player

        moveSpeed = Mathf.Lerp(slowSpeed, calcMoveSpeed, Mathf.Clamp01(Input.GetAxis("Vertical") + 1f));
        
        if (bIsGrounded())
        {
            //Handle our runspeed details. This'll also have to affect our actual movement speed
            if (groundObject && groundObject.GetComponent<Terrain>() != null) //Because this is square distance. Essentially we're trying to see when we're in the grass
            {
                targetRunType = 0;
                moveSpeed = slowSpeed;  //Slow us down because we're in the grass
            }
            else
            {
                targetRunType = Mathf.Lerp(1f/3f, 2f/3f, Mathf.Clamp01(Input.GetAxis("Vertical") + 1f));
            }            
        }

        runType = Mathf.Lerp(runType, targetRunType, Time.deltaTime * 2f);  //So that our shifts aren't jarring

        playerAnimator.SetFloat("RunSpeedMultiplier", RunScaleRange.GetLerp(runType));
        playerAnimator.SetFloat("RunType", runType);

        float curSpeedX = moveSpeed;
        float curSpeedY = strafeSpeed * Input.GetAxis("Horizontal") + SideMomentum; //So we can move extra fast if we've done a side kick. What should our air control be?

        //Vita Control injection      
#if !UNITY_EDITOR
        moveSpeed = Mathf.Lerp(slowSpeed, calcMoveSpeed, Mathf.Clamp01(Input.GetAxis("Left Stick Vertical") +1f));  
        curSpeedY = strafeSpeed * Input.GetAxis("Left Stick Horizontal") + SideMomentum; //So we can move extra fast if we've done a side kick. What should our air control be?
#endif

        //Add in our stumble effect
        float stumbleValue = GetStumbleValue();
        curSpeedX *= stumbleValue;
        curSpeedY *= stumbleValue;

        //We need to check our speed and adjust our leadTime accordingly
        //PROBLEM: This lead timer doesn't take into account actual movement, and any cool stuff we might be doing with Parkour
        PlayerLeadTime += (curSpeedX - (slowSpeed + runningSpeed) * 0.5f) * Time.deltaTime;  //The crazy lazy way
        
        
        if (PlayerLeadTime <= 0 && !bPlayerInvincible) {
            DeadIndicator.SetActive(true);
            bPlayerDead = true; //Kill our player
        }

        PlayerLeadTime = Mathf.Clamp(PlayerLeadTime, 0, 5); //So we can't get too much of a lead!


        //PROBLEM: Should clamp the side momentium so that we can't do insaine move speeds. Or just leave it as it is
        float movementDirectionY = moveDirection.y; //A quick save to preserve values
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = movementDirectionY;

        characterController.Move(moveDirection * Time.deltaTime);   //Actually do our move
        //moveSpeed = Vector3.Distance(priorPosition, gameObject.transform.position)/Time.deltaTime;  //This could be really irregular...
        flatMoveDistance = Vector2.Distance(new Vector2(priorPosition.x, priorPosition.z), new Vector2(transform.position.x, transform.position.z));
        priorPosition = gameObject.transform.position; //Reset this so that we get a read for the next tick
        //We'd be wise to align our character to the movement direction here too (as it'll fix forward issues)
        gameObject.transform.LookAt(gameObject.transform.position + forward * 3f, Vector3.up);
    }

    //The jump direction in this case is for jumping off a wall. We'll get to that
    public void DoJump(float sideMomentum, float scaleFactor)
    {
        moveDirection.y = jumpSpeed * scaleFactor;
        if (sideMomentum != 0)  //Only apply momentum if we've got momentum
        {
            SideMomentum = sideMomentum * kickMomentum;
        }
    }

    public bool bValidWallRun()
    {
        if (LastWallNormal == Vector3.zero)
        {
            return true;
        }
        if (Vector3.Dot(LastWallNormal, WallHitNormal) > 0.75f) { //this isn't a different enough angled wall
            return false;
        }
        return true;
    }

    [HideInInspector]
    GameObject groundObject = null;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.9f)    //Change if we want incline
            groundObject = hit.collider.gameObject;
    }

    public bool bIsGrounded()
    {
        if (characterController.isGrounded)
        {
            WallRunBias = 0; //Reset our bias so we can wall run again
            SideMomentum = 0; //Get our side momentium under control again
            LastWallNormal = Vector3.zero;
            checkRunAnim(); //So that we're playing the correct animation for being on the ground
        }
        return characterController.isGrounded;
    }

    //For the moment (before we add spline controls) lets just do this
    public Vector3 Char_Forward {  get { return Vector3.forward;  } }

    public Vector3 Char_Right { get { return Vector3.right; } }

    public bool bHitWall()
    {
        //I think that this is going to have to be a raycast...
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Char_Forward, out hit, trickRayDist, worldRaycastMask))
        {
            return true; //We've hit an object
        }
        return false;   //No object hit
    }

    public float WallOnSide()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Char_Right, out hit, trickRayDist, worldRaycastMask))
        {
            WallHitNormal = hit.normal;
            return 1f; //We've a wall on our right
        }

        if (Physics.Raycast(transform.position, -Char_Right, out hit, trickRayDist, worldRaycastMask))
        {
            WallHitNormal = hit.normal;
            return -1f; //We've a wall on our left
        }

        return 0f;
    }

    public Vector3 MantlePoint()
    {
        //Check our up reach
        RaycastHit hit;
        _mantlePoint = Vector3.zero;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, mantleGrabHeight, worldRaycastMask))
        {
            return Vector3.zero; //We're grabbing into a ceiling
        }

        //See if our grab goes over the lip of something
        Vector3 mantleGrabReach = transform.position + Vector3.up * mantleGrabHeight;
        
        if (Physics.Raycast(mantleGrabReach, Char_Forward * mantleGrabDepth, out hit, mantleGrabHeight, worldRaycastMask))
        {
            return Vector3.zero; //We're not grabbing above an object
        }
        //Debug.DrawLine(mantleGrabReach, mantleGrabReach + Char_Forward * mantleGrabDepth, Color.red, 5f);

        Vector3 mantleGrabLip = mantleGrabReach + Char_Forward * mantleGrabDepth;   //This is the point we cast down from to see if we're doing a mantle
        if (Physics.Raycast(mantleGrabLip, -Vector3.up * mantleGrabHeight, out hit, mantleGrabHeight, worldRaycastMask))
        {
            
            //Debug.DrawLine(transform.position, transform.position + Vector3.up * mantleGrabHeight, Color.red, 15f);
            //Debug.DrawLine(mantleGrabReach, mantleGrabReach + Char_Forward * mantleGrabDepth, Color.red, 15f);
            //Debug.DrawLine(mantleGrabLip, hit.point, Color.red, 15f);
            //Debug.Log(hit.collider.gameObject.name);
            
            
            _mantlePoint = hit.point;
            return hit.point; //We're not grabbing above an object
        }

        return Vector3.zero;
    }

    public void DoFall(bool bCanControl, float scaleFactor)
    {
        //Lets get some jump control in here
        if (moveDirection.y > 0 && !bJumpHeld() && bCanControl)
        {
            moveDirection.y -= gravity * Time.deltaTime * scaleFactor;
        }

        moveDirection.y -= gravity * Time.deltaTime * scaleFactor;
    }

    public void DoClimb()
    {
        characterController.Move((Vector3.up * climbSpeed + Vector3.forward * slowSpeed) * Time.deltaTime);
    }


    #endregion

    #region CameraHandler
    void ControlCamera()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        //Add in our VitaControls
        rotationX += Input.GetAxis("Right Stick Vertical") * 100f * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;
        //More proxy Vita Controls
        rotationY += Input.GetAxis ("Right Stick Horizontal") * 100f * Time.deltaTime;

        rotationY = Mathf.Clamp(rotationY, -lookYLimit, lookYLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        if (eyePosition)    //Lock our camera to our characters head
        {
            playerCamera.transform.position = eyePosition.transform.position; 
        }
    }
    #endregion

    void Update()
    {
        currentState.UpdateState(); //Update our current movement state

        HandleControllerScale();
        HandleMomentumControl();
        //And I don't see why we can't just leave the camera controller here...
        ControlCamera();
        AdjustFollowDisplay();

        //Checks for our movement boosts on Jump release
        GetJumpReleaseTime();
        if (bBoostTriggerReady && !bJumpHeld())
        {
            doBoostTrigger();
        }

        if (bPlayerDead) {
            if (Input.GetKey(KeyCode.Return) || Input.GetButton("Circle")) {
                bPlayerDead = false;
                DeadIndicator.SetActive(false);
                //Respawn our player
                gameObject.transform.position = StartPosition;
                priorPosition = StartPosition;
                //Handle our curve position
                bestTime = pathCreator.path.GetClosestTimeOnPath(gameObject.transform.position);  //This is actually well optimised...
                bestDistance = pathCreator.path.GetClosestDistanceAlongPath(gameObject.transform.position);

                PlayerLeadTime = 3f;
            }
        }

        //Housekeeping
        //Because our character body keeps drifting with animations...
        playerAnimator.transform.localPosition = Vector3.Lerp(playerAnimator.transform.localPosition, Vector3.zero, Time.deltaTime);
        playerAnimator.transform.localRotation = Quaternion.Slerp(playerAnimator.transform.localRotation, Quaternion.identity, Time.deltaTime);


    }

    void AdjustFollowDisplay() {
        FollowIndicator.GetComponent<RectTransform>().sizeDelta = new Vector2(FollowIndicator.GetComponent<RectTransform>().sizeDelta.x, Mathf.Lerp(400f, 100f, PlayerLeadTime / 3f));
    }

    //This might need more information passed through at some stage, but we're starting with a MVP here
    public void EnemyHitPlayer(GameObject Instigator) {

        ourDamageIndicator.TakeDamage(Instigator.transform.position.x > gameObject.transform.position.x);
        //This needs to put in place a hit effect, and also a speed penalty
        //Essentially this is a "stumble"
        stumbleTime = stumbleMax;
        boostTime = 0; //Getting hit cancels our bost
        setCurrentAnimation("Hit_Left");    //Our hits need a special handler as technically they're grounded
        //setCurrentAnimation("Hit_Guard");
    }

    #region BoostFunctions

    //This is used for jump button release boosts
    public void SetBoostTrigger(float boostDuration)
    {
        //Debug.Log("setting boost trigger");
        boostTriggerTime = Time.time;
        bBoostTriggerReady = true;
        boostTriggerDuration = boostDuration;
    }

    void doBoostTrigger()
    {
        //Debug.Log(boostTriggerTime - jumpReleaseTime);
        if (boostTriggerThreshold.ValueWithin(Mathf.Abs(boostTriggerTime - jumpReleaseTime)))
        {
            BoostPlayer(boostTriggerDuration);
        }
        bBoostTriggerReady = false; //Call this done done :)
    }

    public void BoostPlayer(float extraboostTime)
    {
        if (boostTime < 0)
        {
            boostTime = 0;
        }
        boostTime += extraboostTime;
    }
    #endregion

    #region LadderFunctions
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Got Trigger Enter");
        if (other.gameObject.layer == 9)    //PROBLEM: nasty hack to check for the climbable layer
        {
            //Debug.Log("Collided with object" + other.gameObject.name);
            // Additional logic for handling the trigger
            bClimbing = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("Got Trigger Enter");
        if (other.gameObject.layer == 9)    //PROBLEM: nasty hack to check for the climbable layer
        {
            //Debug.Log("Collided with object" + other.gameObject.name);
            // Additional logic for handling the trigger
            bClimbing = false;
        }
    }
    #endregion
}