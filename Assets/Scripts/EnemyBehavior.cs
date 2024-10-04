using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {

	public LayerMask redropMask;
	public float speed_amble = 5f;
	public float speed_dash = 11f;

	[HideInInspector]
	public bool bHasStruckPlayer = false;

	public float dash_radius = 7f;	//At what point do we speed up?

	public float attention_radius = 15f; //If our player is outside of this range we just do Zombie stuff
	public Animator targetCharacter;


	float EnemyFallSpeed = 0;
	float gravity = 20f;

	CharacterController characterController;

	void Start() {
		characterController = gameObject.GetComponent<CharacterController>();
	}

	public void Update() {
		DoEnemyMove();  //Move our enemy towards our player
						//PickEnemyFrame(); //Our enemies will play a "grab" animation when they're close
						//If we're behind the player we should "re-drop" forward of the player somewhere to be an enemy a second time around (same as if we die)
						//if (PC_FPSController.Instance.gameObject.transform.position.z > gameObject.transform.position.z || gameObject.transform.position.z - PC_FPSController.Instance.gameObject.transform.position.z > 50) {
						//So we need a smarter way to tell if we're behind our player...
		//Debug.Log(Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position)));
		
		if (Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(gameObject.transform.position - PC_FPSController.Instance.gameObject.transform.position)) < -0.5f) { 
			ReDropEnemy();
		}
		
	}

	public void HitPlayer() {
		//Play our animation stuff for hitting our player. This'll also have to be reflective of our current animation state
		//PROBLEM: Take into account player actions when doing this
		if (targetCharacter)
        {
			targetCharacter.SetTrigger("ZombieStrike");	//Do our strike animation
		}
	}


	public void ReDropEnemy() {
		Debug.Log("Doing Enemy redrop");
		Vector3 dropPoint = LevelController.Instance.GetEnemyDropPoint(redropMask);
		RespawnEnemy(dropPoint);
	}

	public void RespawnEnemy(Vector3 thisPos) {
		gameObject.transform.position = thisPos + Vector3.up * 1.5f;
		bHasStruckPlayer = false;
	}

	public void DoEnemyMove()
	{
		//PROBLEM: This will need to be replaced with a curve sample for our direction
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		Vector3 playerDir = PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position;
		float distToPlayer = playerDir.magnitude;
		/*
		if (distToPlayer > attention_radius) {
			return; //Don't movie our zombie, save some process
		}*/

		float playerAngle = Mathf.Atan2(playerDir.x, playerDir.z);
		//Debug.Log(playerAngle);
		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;
		//It's actually a little pointless to have these different speeds as the player doesn't get to see it
		float moveSpeed = speed_amble; // distToPlayer > dash_radius ? speed_amble : speed_dash;

		Vector3 moveDirection = playerDir * moveSpeed;  //Get the net of how we should be ambling

		if (characterController.isGrounded)
        {
			EnemyFallSpeed = 0;
        } else
        {
			EnemyFallSpeed -= gravity * Time.deltaTime;
        }

		moveDirection.y = EnemyFallSpeed;
		characterController.Move(moveDirection * Time.deltaTime);   //Actually do our move
		//We need to point our enemy at our player
		//This isn't good enough for our direction. While I don't feel we need pathfinding we do need enemies that don't operate like turrets
		//gameObject.transform.LookAt(PC_FPSController.Instance.gameObject.transform.position, Vector3.up);
		gameObject.transform.eulerAngles = new Vector3(0, playerAngle * 180f/ Mathf.PI, 0);
	}

}
