using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_NullState : PC_BaseState {
	public PC_NullState(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
	: base(currentContext, playerStateFactory)
	{ }

}
