using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Patrol1 : AIZombieState
{
    [SerializeField] private AIWaypointNetwork _waypointNetwork = null;
    [SerializeField] private bool _randomPatrol = false;
    [SerializeField] private int _currentWaypoint = 0;
    [SerializeField] [Range(0.0f, 3.0f)] private float _speed = 1.0f;


    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        return AIStateType.Patrol;
    }
}
