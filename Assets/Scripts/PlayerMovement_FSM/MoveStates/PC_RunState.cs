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
			baseController.DoFall();
        }

		CheckSwitchState();
	}

	public override void CheckSwitchState()
	{
		if (baseController.bJumpPressed())
		{
			Debug.Log("Doing Jump");
			baseController.DoJump(Vector3.up);
			SwitchState(factory.PCAirbourne());
		}
	}
}
