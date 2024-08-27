﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PC_FPSController : MonoBehaviour
{

    public bool bClimbing = false;

    public float slowSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float sprintingSpeed = 15f;

    public float strafeSpeed = 9f;

    public float climbSpeed = 5f;

    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    //A quick little bit of testing for our raycast colliders
    float trickRayDist = 1f; //How far out do we test for raycasts in our world for the different tricks we'll be doing?

    //Details for mantling
    float mantleGrabHeight = 1.5f; //Above our zero
    float mantleGrabDepth = 1f; //From in front of our character


    public LayerMask worldRaycastMask;  //So we don't do tricks against the wrong thing

    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 30f;
    public float lookYLimit = 45f;
    public enum enPCMoveState { NULL, RUNNING, AIRBOURNE, WALLKICK, MANTLE };
    public enPCMoveState PC_MoveState;

    public enPCMoveState PC_startingState;
    //Movement state factory setup
    private PC_BaseState currentState;
    private PC_MoveStateFactory states;
    public PC_BaseState CurrentState { get { return currentState; } set { currentState = value; } }

    CharacterController characterController;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0, rotationY = 0;

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

    float targetHeightScale = 1f;
    float heightScale = 1f;

    void Start()
    {
        states = new PC_MoveStateFactory(this);
        // currentState = states.EnemyNullState();
        Func<PC_BaseState>[] allStates = new Func<PC_BaseState>[] { states.PCNullState, states.PCRunState, states.PCAirbourne, states.PCWallKick, states.PCMantleState };
        currentState = allStates[(int)PC_startingState]();
        currentState.EnterState();

        characterController = GetComponent<CharacterController>();
        controllerHeight = characterController.height;
        cameraStartPosition = playerCamera.transform.localPosition;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #region InputMethodsForFSM
    public bool bJumpPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
        //return Input.GetButtonDown("Jump");
    }

    public bool bJumpHeld()
    {
        return Input.GetButton("Jump");
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
    }

    public void DoFlatMove()
    {
        //PROBLEM: This will need to be replaced with a curve sample for our direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool addEffort = bAddEffort();

        float moveSpeed = addEffort ? sprintingSpeed : Mathf.Lerp(slowSpeed, runningSpeed, Input.GetAxis("Vertical") * 0.5f + 0.5f);

        float curSpeedX = moveSpeed;
        float curSpeedY = strafeSpeed * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y; //A quick save to preserve values
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = movementDirectionY;

        characterController.Move(moveDirection * Time.deltaTime);   //Actually do our move
    }

    //The jump direction in this case is for jumping off a wall. We'll get to that
    public void DoJump(Vector3 jumpDirection)
    {
        moveDirection.y = jumpSpeed;
    }

    public bool bIsGrounded()
    {
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

        Vector3 mantleGrabLip = mantleGrabReach + Char_Forward * mantleGrabDepth;   //This is the point we cast down from to see if we're doing a mantle
        if (Physics.Raycast(mantleGrabReach, -Vector3.up * mantleGrabHeight, out hit, mantleGrabHeight, worldRaycastMask))
        {
            _mantlePoint = hit.point;
            return hit.point; //We're not grabbing above an object
        }

        return Vector3.zero;
    }

    public void DoFall(bool bCanControl)
    {
        //Lets get some jump control in here
        if (moveDirection.y > 0 && !bJumpHeld() && bCanControl)
        {
            moveDirection.y -= gravity * Time.deltaTime * 3f;
        }

        moveDirection.y -= gravity * Time.deltaTime;
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
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -lookYLimit, lookYLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    }
    #endregion

    void Update()
    {
        currentState.UpdateState(); //Update our current movement state

        //And I don't see why we can't just leave the camera controller here...
        ControlCamera();
    }

    void LateUpdate()
    {

    }
    void OldUpdate()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool addEffort = Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = addEffort ? sprintingSpeed : Mathf.Lerp(slowSpeed, runningSpeed, Input.GetAxis("Vertical") * 0.5f + 0.5f);
        
        float curSpeedX = canMove ? moveSpeed : 0;
        float curSpeedY = canMove ? strafeSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (!bClimbing)
        {
            if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            {
                moveDirection.y = jumpSpeed;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);
        } else
        {
            characterController.Move((Vector3.up * climbSpeed + Vector3.forward * slowSpeed) * Time.deltaTime);
        }

        // Player and Camera rotation
        if (canMove)
        {
            //So the idea behind our runner is to have something that'll have the camera looking as though we're still running in a stright line

            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -lookYLimit, lookYLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            //playerCamera.transform.localRotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            //transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    #region LadderFunctions
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Got Trigger Enter");
        if (other.gameObject.layer == 9)    //PROBLEM: nasty hack to check for the climbable layer
        {
            Debug.Log("Collided with object" + other.gameObject.name);
            // Additional logic for handling the trigger
            bClimbing = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Got Trigger Enter");
        if (other.gameObject.layer == 9)    //PROBLEM: nasty hack to check for the climbable layer
        {
            Debug.Log("Collided with object" + other.gameObject.name);
            // Additional logic for handling the trigger
            bClimbing = false;
        }
    }
    #endregion
}