using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    public void SetStateMachine(AIStateMachine machine) { _stateMachine = machine; }

    // Default Handlers
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }
    public virtual void OnAnimatorUpdated()
    {
        if (_stateMachine.useRootPosition)
        {
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;
        }

        if (_stateMachine.useRootRotation)
        {
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
        }
    }

    public abstract AIStateType OnUpdate();
    public abstract AIStateType GetStateType();

    protected AIStateMachine _stateMachine;
}
