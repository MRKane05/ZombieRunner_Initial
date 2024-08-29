using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_BaseState {

    protected PC_FPSController baseController;
    protected PC_MoveStateFactory factory;

    public PC_BaseState(PC_FPSController currentContext, PC_MoveStateFactory playerStateFactory)
    {
        baseController = currentContext;
        factory = playerStateFactory;
    }

    public virtual void EnterState() {
        baseController.TargetHeightScale = 1f;
    }
    public virtual void UpdateState() { }
    public virtual void PhysicsUpdateState() { }
    public virtual void ExitState() { }
    public virtual void CheckSwitchState() { }
    protected void SwitchState(PC_BaseState newState)
    {
        baseController.StateDisplay.text = newState.ToString();
        //Debug.Log("Enemy: " + ctx.gameObject.name + " SS: " + newState);
        ExitState();
        newState.EnterState();
        baseController.CurrentState = newState;
        Debug.Log("Player in State: " + newState);
    }

    public void SetStartState(PC_BaseState newState)
    {
        newState.EnterState();
        baseController.CurrentState = newState;
    }
}