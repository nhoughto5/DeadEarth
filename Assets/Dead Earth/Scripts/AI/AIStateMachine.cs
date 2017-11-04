using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Public Enums of the AI System
public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }
public enum AIBoneAlignmentType { XAxis, YAxis, ZAxis, XAxisInverted, YAxisInverted, ZAxisInverted }

// ----------------------------------------------------------------------
// Class	:	AITarget
// Desc		:	Describes a potential target to the AI System
// ----------------------------------------------------------------------
public struct AITarget {
	public AITargetType type { get; private set; }
	public Collider collider { get; private set; }
	public Vector3 position { get; private set; }
	public float distance { get; set; }
	public float time { get; private set; }

	public void Set(AITargetType t, Collider c, Vector3 p, float d) {
		type = t;
		collider = c;
		position = p;
		distance = d;
		time = Time.time;
	}

	public void Clear() {
		type = AITargetType.None;
		collider = null;
		position = Vector3.zero;
		time = 0.0f;
		distance = Mathf.Infinity;
	}
}

// ----------------------------------------------------------------------
// Class	:	AIStateMachine
// Desc		:	Base class for all AI State Machines
// ----------------------------------------------------------------------
public abstract class AIStateMachine : MonoBehaviour {
	// Public
	public AITarget AudioThreat = new AITarget();
	public AITarget VisualThreat = new AITarget();

	// Protected
	protected AIState _currentState;

	protected AITarget _target;
	protected bool _isTargetReached;
	protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
	protected int _rootPositionRefCount;
	protected int _rootRotationRefCount;
	protected bool _cinematicEnabled;

	// Protected Inspector Assigned
	[SerializeField, Range(0, 15)] protected float _stoppingDistance = 1.0f;
	[SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
	[SerializeField] protected AIWaypointNetwork _waypointNetwork = null;
	[SerializeField] protected bool _randomPatrol = false;
	[SerializeField] protected int _currentWaypoint = -1;
	[SerializeField] protected SphereCollider _sensorTrigger = null;
	[SerializeField] protected SphereCollider _targetTrigger = null;
	[SerializeField] protected Transform _rootBone = null;
	[SerializeField] protected AIBoneAlignmentType _rootBoneAlignment = AIBoneAlignmentType.ZAxis;

	// Component Cache
	protected Animator _animator;
	protected Collider _collider;
	protected int _aiBodyPartLayer = -1;
	protected List<Rigidbody> _bodyParts = new List<Rigidbody>();
	protected NavMeshAgent _navAgent;
	protected Transform _transform;

	// Public Properties
	public Animator animator { get { return _animator; } }
	public bool inMeleeRange { get; set; }
	public bool isTargetReached { get { return _isTargetReached; } }
	public NavMeshAgent navAgent { get { return _navAgent; } }

	public Vector3 sensorPosition {
		get {
			if (_sensorTrigger == null) { return Vector3.zero; }

			Vector3 point = _sensorTrigger.transform.position;
			point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
			point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
			point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
			return point;
		}
	}

	public float sensorRadius {
		get {
			if (_sensorTrigger == null) { return 0.0f; }

			float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
				_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

			return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
		}
	}

	public bool cinematicEnabled { get { return _cinematicEnabled; } set { _cinematicEnabled = value; } }
	public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
	public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
	public AITargetType targetType { get { return _target.type; } }
	public Vector3 targetPosition { get { return _target.position; } }

	public int targetColliderID {
		get {
			if (_target.collider) { return _target.collider.GetInstanceID(); }

			return -1;
		}
	}

	// -----------------------------------------------------------------------------
	// Name	:	GetWaypointPosition
	// Desc	:	Fetched the world space position of the state machine's currently
	//			set waypoint with optional increment
	// -----------------------------------------------------------------------------
	public Vector3 GetWaypointPosition(bool increment) {
		if (_currentWaypoint == -1) {
			if (_randomPatrol) { _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count); } else { _currentWaypoint = 0; }
		} else if (increment) { NextWaypoint(); }

		// Fetch the new waypoint from the waypoint list
		if (_waypointNetwork.Waypoints[_currentWaypoint] != null) {
			Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

			// This is our new target position
			SetTarget(AITargetType.Waypoint,
				null,
				newWaypoint.position,
				Vector3.Distance(newWaypoint.position, transform.position));

			return newWaypoint.position;
		}

		return Vector3.zero;
	}

	// -------------------------------------------------------------------
	// Name :	SetTarget (Overload)
	// Desc	:	Sets the current target and configures the target trigger
	// -------------------------------------------------------------------
	public void SetTarget(AITargetType t, Collider c, Vector3 p, float d) {
		// Set the target info
		_target.Set(t, c, p, d);

		// Configure and enable the target trigger at the correct
		// position and with the correct radius
		if (_targetTrigger != null) {
			_targetTrigger.radius = _stoppingDistance;
			_targetTrigger.transform.position = _target.position;
			_targetTrigger.enabled = true;
		}
	}

	// --------------------------------------------------------------------
	// Name :	SetTarget (Overload)
	// Desc	:	Sets the current target and configures the target trigger.
	//			This method allows for specifying a custom stopping
	//			distance.
	// --------------------------------------------------------------------
	public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s) {
		// Set the target Data
		_target.Set(t, c, p, d);

		// Configure and enable the target trigger at the correct
		// position and with the correct radius
		if (_targetTrigger != null) {
			_targetTrigger.radius = s;
			_targetTrigger.transform.position = _target.position;
			_targetTrigger.enabled = true;
		}
	}

	// -------------------------------------------------------------------
	// Name :	SetTarget (Overload)
	// Desc	:	Sets the current target and configures the target trigger
	// -------------------------------------------------------------------
	public void SetTarget(AITarget t) {
		// Assign the new target
		_target = t;

		// Configure and enable the target trigger at the correct
		// position and with the correct radius
		if (_targetTrigger != null) {
			_targetTrigger.radius = _stoppingDistance;
			_targetTrigger.transform.position = t.position;
			_targetTrigger.enabled = true;
		}
	}

	// -------------------------------------------------------------------
	// Name :	ClearTarget
	// Desc	:	Clears the current target
	// -------------------------------------------------------------------
	public void ClearTarget() {
		_target.Clear();
		if (_targetTrigger != null) { _targetTrigger.enabled = false; }
	}

	// ------------------------------------------------------------
	// Name	:	OnTriggerEvent
	// Desc	:	Called by our AISensor component when an AI Aggravator
	//			has entered/exited the sensor trigger.
	// -------------------------------------------------------------
	public virtual void OnTriggerEvent(AITriggerEventType type, Collider other) {
		if (_currentState != null) { _currentState.OnTriggerEvent(type, other); }
	}

	// ----------------------------------------------------------
	// Name	:	NavAgentControl
	// Desc	:	Configure the NavMeshAgent to enable/disable auto
	//			updates of position/rotation to our transform
	// ----------------------------------------------------------
	public void NavAgentControl(bool positionUpdate, bool rotationUpdate) {
		if (_navAgent) {
			_navAgent.updatePosition = positionUpdate;
			_navAgent.updateRotation = rotationUpdate;
		}
	}

	// ----------------------------------------------------------
	// Name	:	AddRootMotionRequest
	// Desc	:	Called by the State Machine Behaviours to
	//			Enable/Disable root motion
	// ----------------------------------------------------------
	public void AddRootMotionRequest(int rootPosition, int rootRotation) {
		_rootPositionRefCount += rootPosition;
		_rootRotationRefCount += rootRotation;
	}

	public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager,
								   int hitDirection = 0) {
	}

	// -----------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Cache Components
	// -----------------------------------------------------------------
	protected virtual void Awake() {
		// Cache all frequently accessed components
		_transform = transform;
		_animator = GetComponent<Animator>();
		_navAgent = GetComponent<NavMeshAgent>();
		_collider = GetComponent<Collider>();
		_aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
		// Do we have a valid Game Scene Manager
		if (GameSceneManager.instance != null) {
			// Register State Machines with Scene Database
			if (_collider) { GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this); }
			if (_sensorTrigger) { GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this); }
		}

		if (_rootBone != null) {
			Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody bodyPart in bodies)
				if ((bodyPart != null) && (bodyPart.gameObject.layer == _aiBodyPartLayer)) {
					_bodyParts.Add(bodyPart);
					GameSceneManager.instance.RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
				}
		}
	}

	// -----------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Called by Unity prior to first update to setup the object
	// -----------------------------------------------------------------
	protected virtual void Start() {
		// Set the sensor trigger's parent to this state machine
		if (_sensorTrigger != null) {
			AISensor script = _sensorTrigger.GetComponent<AISensor>();
			if (script != null) { script.parentStateMachine = this; }
		}

		// Fetch all states on this game object
		AIState[] states = GetComponents<AIState>();

		// Loop through all states and add them to the state dictionary
		foreach (AIState state in states)
			if ((state != null) && !_states.ContainsKey(state.GetStateType())) {
				// Add this state to the state dictionary
				_states[state.GetStateType()] = state;

				// And set the parent state machine of this state
				state.SetStateMachine(this);
			}

		// Set the current state
		if (_states.ContainsKey(_currentStateType)) {
			_currentState = _states[_currentStateType];
			_currentState.OnEnterState();
		} else { _currentState = null; }

		// Fetch all AIStateMachineLink derived behaviours from the animator
		// and set their State Machine references to this state machine
		if (_animator) {
			AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();
			foreach (AIStateMachineLink script in scripts)
				script.stateMachine = this;
		}
	}

	// -------------------------------------------------------------------
	// Name :	FixedUpdate
	// Desc	:	Called by Unity with each tick of the Physics system. It
	//			clears the audio and visual threats each update and
	//			re-calculates the distance to the current target
	// --------------------------------------------------------------------
	protected virtual void FixedUpdate() {
		VisualThreat.Clear();
		AudioThreat.Clear();

		if (_target.type != AITargetType.None) { _target.distance = Vector3.Distance(_transform.position, _target.position); }

		_isTargetReached = false;
	}

	// -------------------------------------------------------------------
	// Name :	Update
	// Desc	:	Called by Unity each frame. Gives the current state a
	//			chance to update itself and perform transitions.
	// -------------------------------------------------------------------
	protected virtual void Update() {
		if (_currentState == null) { return; }

		AIStateType newStateType = _currentState.OnUpdate();
		if (newStateType != _currentStateType) {
			AIState newState = null;
			if (_states.TryGetValue(newStateType, out newState)) {
				_currentState.OnExitState();
				newState.OnEnterState();
				_currentState = newState;
			} else if (_states.TryGetValue(AIStateType.Idle, out newState)) {
				_currentState.OnExitState();
				newState.OnEnterState();
				_currentState = newState;
			}

			_currentStateType = newStateType;
		}
	}

	// --------------------------------------------------------------------------
	//	Name	:	OnTriggerEnter
	//	Desc	:	Called by Physics system when the AI's Main collider enters
	//				its trigger. This allows the child state to know when it has
	//				entered the sphere of influence	of a waypoint or last player
	//				sighted position.
	// --------------------------------------------------------------------------
	protected virtual void OnTriggerEnter(Collider other) {
		if ((_targetTrigger == null) || (other != _targetTrigger)) { return; }

		_isTargetReached = true;

		// Notify Child State
		if (_currentState) { _currentState.OnDestinationReached(true); }
	}

	protected virtual void OnTriggerStay(Collider other) {
		if ((_targetTrigger == null) || (other != _targetTrigger)) { return; }

		_isTargetReached = true;
	}

	// --------------------------------------------------------------------------
	//	Name	:	OnTriggerExit
	//	Desc	:	Informs the child state that the AI entity is no longer at
	//				its destination (typically true when a new target has been
	//				set by the child.
	// --------------------------------------------------------------------------
	protected void OnTriggerExit(Collider other) {
		if ((_targetTrigger == null) || (_targetTrigger != other)) { return; }

		_isTargetReached = false;

		if (_currentState != null) { _currentState.OnDestinationReached(false); }
	}

	// -----------------------------------------------------------
	// Name	:	OnAnimatorMove
	// Desc	:	Called by Unity after root motion has been
	//			evaluated but not applied to the object.
	//			This allows us to determine via code what to do
	//			with the root motion information
	// -----------------------------------------------------------
	protected virtual void OnAnimatorMove() {
		if (_currentState != null) { _currentState.OnAnimatorUpdated(); }
	}

	// ----------------------------------------------------------
	// Name	: OnAnimatorIK
	// Desc	: Called by Unity just prior to the IK system being
	//		  updated giving us a chance to setup up IK Targets
	//		  and weights.
	// ----------------------------------------------------------
	protected virtual void OnAnimatorIK(int layerIndex) {
		if (_currentState != null) { _currentState.OnAnimatorIKUpdated(); }
	}

	// -------------------------------------------------------------------------
	// Name	:	NextWaypoint
	// Desc	:	Called to select a new waypoint. Either randomly selects a new
	//			waypoint from the waypoint network or increments the current
	//			waypoint index (with wrap-around) to visit the waypoints in
	//			the network in sequence. Sets the new waypoint as the the
	//			target and generates a nav agent path for it
	// -------------------------------------------------------------------------
	private void NextWaypoint() {
		// Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
		if (_randomPatrol && (_waypointNetwork.Waypoints.Count > 1)) {
			// Keep generating random waypoint until we find one that isn't the current one
			// NOTE: Very important that waypoint networks do not only have one waypoint :)
			int oldWaypoint = _currentWaypoint;
			while (_currentWaypoint == oldWaypoint) { _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count); }
		} else { _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1; }
	}
}