using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_WallRun : PC_BaseState {
	public PC_WallRun(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
	: base(currentContext, playerStateFactory)
	{ }

	float runTimer = 0;
	float maxRunDuration = 1;
    public override void EnterState()
    {
		base.EnterState();
		runTimer = 0; //reset our wallrun timer

		baseController.DoJump(0f, 0.33f); //set our upward velocity to a small hop for the wallrun
		//baseController.WallRunBias = baseController.WallOnSide();
	}

    public override void UpdateState() {
		runTimer += Time.deltaTime;
		//For the moment lets just use our move functions
		baseController.DoFlatMove();
		float WallContact = baseController.WallOnSide();

		if (WallContact == 0)
        {
			//really we should drop out of our state
			baseController.DoFall(false, 1f);	//Apply some gravity decay while we're wallrunning
        } else
        {
			baseController.DoFall(false, 0.125f);
		}

		CheckSwitchState();
	}

	public override void CheckSwitchState()
	{
		float WallContact = baseController.WallOnSide();
		if (baseController.bJumpPressed())
		{
			Debug.Log("Doing Jump");
			baseController.DoJump(-WallContact, 1f);	//So in sense we'll have to add momentium for our "kickoff" from the wall
			SwitchState(factory.PCAirbourne());
		}

		if (baseController.bHitWall())
        {
			Debug.Log("Doing Wall Kick");
			baseController.DoJump(0f, 1f);
			SwitchState(factory.PCWallKick());
		}

		//See if we should fall out of our run
		if (runTimer > maxRunDuration)
		{
			Debug.Log("Wall run timed out");
			SwitchState(factory.PCAirbourne());
		}

		if (WallContact == 0)
        {
			SwitchState(factory.PCAirbourne());
		}

		if (baseController.bIsGrounded())
		{
			SwitchState(factory.PCRunState());
		}
	}

    public override void ExitState()
    {
        base.ExitState();
		baseController.LastWallNormal = baseController.WallHitNormal;
    }
}
