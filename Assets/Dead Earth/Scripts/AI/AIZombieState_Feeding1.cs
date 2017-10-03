using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState {
    [SerializeField] [Range(1, 100)] private int _bloodParticlesBurstAmount = 10;
    [SerializeField] [Range(0.01f, 1.0f)] private float _bloodParticlesBurstTime = 0.1f;
    [SerializeField] private Transform _bloodParticlesMount = null;
    private int _eatingLayerIndex = -1;
    private int _eatStateHash = Animator.StringToHash("Feeding State");
    [SerializeField] private float _slerpSpeed = 5.0f;
    private float _timer = 0.0f;

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
        _timer = 0.0f;
    }

    public override void OnExitState() {
        if (_zombieStateMachine != null)
            _zombieStateMachine.feeding = false;
    }

    public override AIStateType OnUpdate() {
        _timer += Time.deltaTime;
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

        // Is the feeding animation playing?
        if (_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash == _eatStateHash) {

            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + 0.01f * (Time.deltaTime * _zombieStateMachine.replenishRate), 1.0f);
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles && _bloodParticlesMount)
            {
                if (_timer > _bloodParticlesBurstTime)
                {
                    ParticleSystem system = GameSceneManager.instance.bloodParticles;
                    system.transform.position = _bloodParticlesMount.transform.position;
                    system.transform.rotation = _bloodParticlesMount.transform.rotation;
                    ParticleSystem.MainModule main = system.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    system.Emit(_bloodParticlesBurstAmount);
                    _timer = 0.0f;
                }
            }
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