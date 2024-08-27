using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_MoveStateFactory {

	private PC_FPSController context;

	public PC_MoveStateFactory(PC_FPSController currentContext)
    {
		context = currentContext;
    }
	public PC_BaseState PCNullState()
    {
		return new PC_NullState(context, this);
    }

	public PC_BaseState PCRunState()
    {
		return new PC_RunState(context, this);
    }

	public PC_BaseState PCAirbourne()
	{
		return new PC_Airbourne(context, this);
	}

	public PC_BaseState PCWallKick()
    {
		return new PC_WallKick(context, this);
    }

	public PC_BaseState PCMantleState()
	{
		return new PC_Mantle(context, this);
	}
}
