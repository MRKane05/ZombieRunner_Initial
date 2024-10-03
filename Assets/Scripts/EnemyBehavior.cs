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


	// Update is called once per frame

	CharacterController characterController;

	void Start() {
		characterController = gameObject.GetComponent<CharacterController>();
	}

	public void Update() {
		DoEnemyMove();	//Move our enemy towards our player
		//PickEnemyFrame(); //Our enemies will play a "grab" animation when they're close
		//If we're behind the player we should "re-drop" forward of the player somewhere to be an enemy a second time around (same as if we die)
		if (PC_FPSController.Instance.gameObject.transform.position.z > gameObject.transform.position.z || gameObject.transform.position.z - PC_FPSController.Instance.gameObject.transform.position.z > 50) {
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
		//Find somewhere forward of our player. For the moment things are straight.
		bool bFoundDropPoint = false;
		int cycles = 0;
		while (!bFoundDropPoint && cycles < 30) {
			//PROBLEM: This'll need re-worked once we've got curves
			float baseRandZ = PC_FPSController.Instance.gameObject.transform.position.z + Random.RandomRange(30f, 40f);
			float baseRandX = Random.RandomRange(-10f, 10f);
			//Do a raycast down to see if we hit the ground and not a vehicle, then plonk our enemy here
			 RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(new Vector3(baseRandX, 20, baseRandZ), -Vector3.up, out hit, 30, redropMask))
			{
				Debug.DrawRay(new Vector3(baseRandX, 20, baseRandZ), -Vector3.up * hit.distance, Color.yellow);
				if (hit.collider.gameObject.tag == "Ground") {	//We're good
					RespawnEnemy(hit.point);
					bFoundDropPoint = true;
				}
			}
			else
			{
				//This shouldn't have happened...
				Debug.DrawRay(new Vector3(baseRandX, 20, baseRandZ), -Vector3.up * 30, Color.red);
			}
			cycles++; //Increment our cycles up so we've got an out. I should write something in here to make the randoms wider the higher the count gets
		}

	}

	public void RespawnEnemy(Vector3 thisPos) {
		gameObject.transform.position = thisPos + Vector3.up * 0.9f;
		bHasStruckPlayer = false;
	}

	public void DoEnemyMove()
	{
		//PROBLEM: This will need to be replaced with a curve sample for our direction
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		Vector3 playerDir = PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position;
		float distToPlayer = playerDir.magnitude;
		if (distToPlayer > attention_radius) {
			return; //Don't movie our zombie, save some process
		}

		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;
		//It's actually a little pointless to have these different speeds as the player doesn't get to see it
		float moveSpeed = speed_amble; // distToPlayer > dash_radius ? speed_amble : speed_dash;

		Vector3 moveDirection = playerDir * moveSpeed;	//Get the net of how we should be ambling

		characterController.Move(moveDirection * Time.deltaTime);   //Actually do our move
																	//We need to point our enemy at our player
		gameObject.transform.LookAt(PC_FPSController.Instance.gameObject.transform.position, Vector3.up);
	}

}
