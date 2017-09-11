using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState
{
    [SerializeField] [Range(1, 60)] private float _maxDuration = 10.0f;
    [SerializeField] private float _waypointAngleThreshold = 90.0f;
    [SerializeField] private float _threatAngleThreshold = 10.0f;

    private float _timer = 0.0f;

    public override void OnEnterState()
    {
        Debug.Log("Entering Alerted State");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _timer = _maxDuration;
    }

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override AIStateType OnUpdate()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0.0f)
        {
            return AIStateType.Patrol;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.None && _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            _stateMachine.SetTarget(_stateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        float angle = 0.0f;

        if (_zombieStateMachine.targetType == AITargetType.Audio ||
            _zombieStateMachine.targetType == AITargetType.Visual_Light)
        {
            angle = FindSignAngle(_zombieStateMachine.transform.forward,
                _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);
            if (_zombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs((angle)) < _threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }
            if (Random.value < _zombieStateMachine.intelligence)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
            }
            else
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
            }
        }
        else if (_zombieStateMachine.targetType == AITargetType.Waypoint)
        {
            angle = FindSignAngle(_zombieStateMachine.transform.forward,
                _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;
            _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
        }
        return AIStateType.Alerted;
    }
}
