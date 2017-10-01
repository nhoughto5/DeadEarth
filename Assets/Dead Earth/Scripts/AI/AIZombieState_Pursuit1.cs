using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState {
    [SerializeField] private float _maxDuration = 40.0f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;

    private float _repathTimer = 0.0f;
    private float _timer = 0.0f;

    public override AIStateType GetStateType() {
        return AIStateType.Pursuit;
    }

    public override void OnEnterState() {
        Debug.Log("Entering Pursuit State");
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.isStopped = false;
        _timer = _repathTimer = 0.0f;
    }

    public override AIStateType OnUpdate() {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration) {
            return AIStateType.Patrol;
        }

        if (_stateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange) {
            return AIStateType.Attack;
        }

        if (_zombieStateMachine.isTargetReached) {
            switch (_stateMachine.targetType) {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();
                    return AIStateType.Alerted;

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }

        if (_zombieStateMachine.navAgent.isPathStale || !_zombieStateMachine.navAgent.hasPath || _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete) {
            return AIStateType.Alerted;
        }

        // Keep Zombie oriented towards player in pursuit
        if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player && _zombieStateMachine.isTargetReached) {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = newRot;
        } else if (!_stateMachine.useRootRotation && !_zombieStateMachine.isTargetReached) {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        } else if (_zombieStateMachine.isTargetReached) {
            return AIStateType.Alerted;
        }

        // Is the visual threat the player?
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position) {
                if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer) {
                    // Repath the agent
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }
            _stateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        // Player is not registering as visual threat (i.e. hiding) but know it's last known
        // position. Don't want that target overriden
        if (_zombieStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
            if (_zombieStateMachine.targetType == AITargetType.Audio ||
                _zombieStateMachine.targetType == AITargetType.Visual_Food) {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Alerted;
            } else if (_zombieStateMachine.targetType == AITargetType.Visual_Light) {
                int currentID = _zombieStateMachine.targetColliderID;

                // Is it the same visual threat as before?
                if (currentID == _zombieStateMachine.VisualThreat.collider.GetInstanceID()) {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position) {
                        if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier,
                                _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer) {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                } else {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }
            }
        } else if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            if (_zombieStateMachine.targetType == AITargetType.Visual_Food) {
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                return AIStateType.Alerted;
            } else if (_zombieStateMachine.targetType == AITargetType.Audio) {
                int currentID = _zombieStateMachine.targetColliderID;

                // Is it the same audio threat as before
                if (currentID == _zombieStateMachine.AudioThreat.collider.GetInstanceID()) {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.AudioThreat.position) {
                        if (Mathf.Clamp(_zombieStateMachine.AudioThreat.distance * _repathDistanceMultiplier,
                                _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer) {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.Pursuit;
                } else {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        return AIStateType.Pursuit;
    }
}