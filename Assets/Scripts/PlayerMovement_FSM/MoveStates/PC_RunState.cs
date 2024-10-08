using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_RunState : PC_BaseState {
	public PC_RunState(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
	: base(currentContext, playerStateFactory)
	{ }

	public override void UpdateState() {
		//For the moment lets just use our move functions
		baseController.DoFlatMove();

		if (!baseController.bIsGrounded())
        {
			baseController.DoFall(true, 1f);
        }

		CheckSwitchState();
	}

	public override void CheckSwitchState()
	{
		if (baseController.bJumpPressed())
		{
			//Debug.Log("Doing Jump");
			baseController.setCurrentAnimation("RunningJump");	//This'll need to be reset before we call it again
			baseController.DoJump(0f, 1f);
			SwitchState(factory.PCAirbourne());
		}

		if (baseController.bHitWall())
        {
			//Debug.Log("Doing Wall Kick");
			baseController.DoJump(0f, 1f);
			SwitchState(factory.PCWallKick());
		}
	}
}
