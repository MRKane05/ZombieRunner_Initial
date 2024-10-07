using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The distinction here is that Airbourne branches into different states than running, so the player has to
//jump to do a wallrun, they can't go from walking to wallrunning
public class PC_Mantle : PC_BaseState {
	public PC_Mantle(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
		: base(currentContext, playerStateFactory)
	{ }

	public override void EnterState()
	{
		baseController.setAnimTrigger("MantleTrigger");
		baseController.TargetHeightScale = 0.3f;	//reduce our scale so that it'll give us the illusion of standing up after a mantle
	}

	public override void UpdateState()
	{
		//For the moment lets just use our move functions
		baseController.DoClimb();
		CheckSwitchState();
	}

	public override void CheckSwitchState() {
		//We've fallen off for whatever reason
		if (baseController.bIsGrounded())
        {
			SwitchState(factory.PCRunState());
        }
		//Technically we need to figure out where our exit point is as far as climbing up goes
		if (baseController.transform.position.y > baseController.GetSetMantlePoint.y + baseController.GetColliderHeight*0.5f)	//Then logically we've passed over the lip of the object we're mantling
        {
			baseController.SetBoostTrigger(0.75f);	//Do our jump-mantle for clearing the top of the object
			SwitchState(factory.PCRunState());
		}
	}
}
