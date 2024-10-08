using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Wallkick should be the same as airbourne with the difference being that while in this state we cannot do
//another wallkick
public class PC_WallKick : PC_Airbourne {
	public PC_WallKick(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
		: base(currentContext, playerStateFactory)
	{ }

	public override void EnterState()
	{
		baseController.setCurrentAnimation("WallKick");
	}

	public override void UpdateState()
	{
		//For the moment lets just use our move functions
		baseController.DoFlatMove();

		if (!baseController.bIsGrounded())	//This should be caught by our exit state, but is here as a bit of overprogramming
		{
			baseController.DoFall(false, 1f);
		}

		CheckSwitchState();
	}

	public override void CheckSwitchState() { 
		if (baseController.bIsGrounded())
        {
			SwitchState(factory.PCRunState());
        }

		Vector3 MantlePoint = baseController.MantlePoint();
		if (MantlePoint != Vector3.zero)
		{
			SwitchState(factory.PCMantleState());
		}
	}
}
