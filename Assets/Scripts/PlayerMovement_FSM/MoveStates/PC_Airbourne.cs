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
			baseController.DoFall();
		}

		CheckSwitchState();
	}

	public override void CheckSwitchState() { 
		if (baseController.bIsGrounded())
        {
			SwitchState(factory.PCRunState());
        }	
	}
}
