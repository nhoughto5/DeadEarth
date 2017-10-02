using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState {
    private int _eatingLayerIndex = -1;
    private int _eatStateHash = Animator.StringToHash("Feeding State");
    [SerializeField] private float _slerpSpeed = 5.0f;

    public override AIStateType GetStateType() {
        return AIStateType.Feeding;
    }

    public override void OnEnterState() {
        Debug.Log("Entering Feeding State");
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;
        if (_eatingLayerIndex == -1) {
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");
        }
        _zombieStateMachine.feeding = true;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.attackType = 0;

        _zombieStateMachine.NavAgentControl(true, false);
    }

    public override void OnExitState() {
        if (_zombieStateMachine != null)
            _zombieStateMachine.feeding = false;
    }

    public override AIStateType OnUpdate() {
        if (_zombieStateMachine.satisfaction > 0.9f) {
            // Sets waypoint as next target
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type != AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Food) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash == _eatStateHash) {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + 0.01f * (Time.deltaTime * _zombieStateMachine.replenishRate), 1.0f);
        }

        if (!_zombieStateMachine.useRootRotation) {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }
        return AIStateType.Feeding;
    }
}