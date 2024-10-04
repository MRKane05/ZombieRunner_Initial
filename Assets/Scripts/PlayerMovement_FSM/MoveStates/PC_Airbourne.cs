using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The distinction here is that Airbourne branches into different states than running, so the player has to
//jump to do a wallrun, they can't go from walking to wallrunning
public class PC_Airbourne : PC_BaseState {
	public PC_Airbourne(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
		: base(currentContext, playerStateFactory)
	{ }

	public override void UpdateState()
	{
		//For the moment lets just use our move functions
		baseController.DoFlatMove();

		if (!baseController.bIsGrounded())	//This should be caught by our exit state, but is here as a bit of overprogramming
		{
			baseController.DoFall(true, 1f);
		}

		CheckSwitchState();
	}

	public override void CheckSwitchState() { 
		if (baseController.bIsGrounded())
        {
			SwitchState(factory.PCRunState());
        }

		if (baseController.bHitWall())
		{
			Debug.Log("Doing Wall Kick");
			baseController.DoJump(0f, 1f);
			SwitchState(factory.PCWallKick());
		}

		Vector3 MantlePoint = baseController.MantlePoint();
		if (MantlePoint != Vector3.zero)
		{
			SwitchState(factory.PCMantleState());
		}

		//So logically we can enter our wallrun state from this one
		float WallRunValue = baseController.WallOnSide();
		if (WallRunValue != 0 && baseController.bValidWallRun()) {    //We can move into our wallrun state
			//Debug.Log("Value: " + WallRunValue + " Bias: " + baseController.WallRunBias);
			//baseController.WallRunBias = WallRunValue;	//Really this should be set in the state itself
			SwitchState(factory.PCWallRunState());
        }
	}
}
