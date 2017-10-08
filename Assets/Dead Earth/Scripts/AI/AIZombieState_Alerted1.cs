using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState {
	[SerializeField] private float _directionChangeTime = 1.5f;

	private float _directionChangeTimer;

	// Inspector Assigned
	[SerializeField, Range(1, 60)] private float _maxDuration = 10.0f;

	[SerializeField] private float _threatAngleThreshold = 10.0f;

	// Private Fields
	private float _timer;

	[SerializeField] private float _waypointAngleThreshold = 90.0f;

	// ------------------------------------------------------------------ Name : GetStateType Desc :
	// Returns the type of the state ------------------------------------------------------------------
	public override AIStateType GetStateType() { return AIStateType.Alerted; }

	// ------------------------------------------------------------------ Name : OnEnterState Desc :
	// Called by the State Machine when first transitioned into this state. It initializes a timer
	// and configures the the state machine ------------------------------------------------------------------
	public override void OnEnterState() {
		Debug.Log("Entering Alerted State");
		base.OnEnterState();
		if (_zombieStateMachine == null) { return; }

		// Configure State Machine
		_zombieStateMachine.NavAgentControl(true, false);
		_zombieStateMachine.speed = 0;
		_zombieStateMachine.seeking = 0;
		_zombieStateMachine.feeding = false;
		_zombieStateMachine.attackType = 0;
		_directionChangeTimer = 0.0f;
		_timer = _maxDuration;
	}

	// --------------------------------------------------------------------- Name : OnUpdate Desc :
	// The engine of this state ---------------------------------------------------------------------
	public override AIStateType OnUpdate() {
		// Reduce Timer
		_timer -= Time.deltaTime;
		_directionChangeTimer += Time.deltaTime;

		// Transition into a patrol state if available
		if (_timer <= 0.0f) {
			_zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
			_zombieStateMachine.navAgent.isStopped = false;
			_timer = _maxDuration;
		}

		// Do we have a visual threat that is the player. These take priority over audio threats
		if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
			_zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
			return AIStateType.Pursuit;
		}

		if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio) {
			_zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
			_timer = _maxDuration;
		}

		if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
			_zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
			_timer = _maxDuration;
		}

		if ((_zombieStateMachine.AudioThreat.type == AITargetType.None) &&
		    (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food) && (_zombieStateMachine.targetType == AITargetType.None)) {
			_zombieStateMachine.SetTarget(_stateMachine.VisualThreat);
			return AIStateType.Pursuit;
		}

		float angle;

		if (((_zombieStateMachine.targetType == AITargetType.Audio) || (_zombieStateMachine.targetType == AITargetType.Visual_Light)) &&
		    !_zombieStateMachine.isTargetReached) {
			angle = FindSignedAngle(_zombieStateMachine.transform.forward,
				_zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);

			if ((_zombieStateMachine.targetType == AITargetType.Audio) && (Mathf.Abs(angle) < _threatAngleThreshold)) { return AIStateType.Pursuit; }

			if (_directionChangeTimer > _directionChangeTime) {
				if (Random.value < _zombieStateMachine.intelligence) { _zombieStateMachine.seeking = (int) Mathf.Sign(angle); }
				else { _zombieStateMachine.seeking = (int) Mathf.Sign(Random.Range(-1.0f, 1.0f)); }
				_directionChangeTimer = 0.0f;
			}
		}
		else if ((_zombieStateMachine.targetType == AITargetType.Waypoint) && !_zombieStateMachine.navAgent.pathPending) {
			angle = FindSignedAngle(_zombieStateMachine.transform.forward,
				_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);

			if (Mathf.Abs(angle) < _waypointAngleThreshold) { return AIStateType.Patrol; }

			if (_directionChangeTimer > _directionChangeTime) {
				_zombieStateMachine.seeking = (int) Mathf.Sign(angle);
				_directionChangeTimer = 0.0f;
			}
		}
		else {
			if (_directionChangeTimer > _directionChangeTime) {
				_zombieStateMachine.seeking = (int) Mathf.Sign(Random.Range(-1.0f, 1.0f));
				_directionChangeTimer = 0.0f;
			}
		}

		return AIStateType.Alerted;
	}
}