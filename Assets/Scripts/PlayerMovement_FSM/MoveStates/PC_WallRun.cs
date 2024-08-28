using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_WallRun : PC_BaseState {
	public PC_WallRun(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
	: base(currentContext, playerStateFactory)
	{ }

    public override void EnterState()
    {
		base.EnterState();
		baseController.DoJump(0f, 0.25f); //set our upward velocity to a small hop for the wallrun
    }

    public override void UpdateState() {
		//For the moment lets just use our move functions
		baseController.DoFlatMove();
		float WallContact = baseController.WallOnSide();

		if (WallContact == 0)
        {
			//really we should drop out of our state
			baseController.DoFall(false, 1f);	//Apply some gravity decay while we're wallrunning
        } else
        {
			baseController.DoFall(false, 0.25f);
			Debug.Log(baseController.WallRunBias);
			baseController.WallRunBias += WallContact * Time.deltaTime;  //Give the player about 3 seconds of wallrun
			Debug.Log(WallContact + ", " + baseController.WallRunBias);
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
		if (Mathf.Abs(baseController.WallRunBias) > 1f)
		{
			if (Mathf.Sign(baseController.WallRunBias) == Mathf.Sign(WallContact))	//Drop out of our wall run. We'll need a bit more handler logic here
            {
				SwitchState(factory.PCAirbourne());
			}
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
}
