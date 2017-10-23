using UnityEngine;

public enum AIBoneControlType {
	Animated,
	Ragdoll,
	RagdollToAnim
}

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine {
	private int _attackHash = Animator.StringToHash("Attack");
	private int _feedingHash = Animator.StringToHash("Feeding");
	private int _seekingHash = Animator.StringToHash("Seeking");
	private int _speedHash = Animator.StringToHash("Speed");
	private AIBoneControlType _boneControlType = AIBoneControlType.Animated;

	[SerializeField, Range(0, 100)] private int _health = 100;
	[SerializeField, Range(0.0f, 1.0f)] private float _aggression = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private float _hearing = 1.0f;
	[SerializeField, Range(0.0f, 1.0f)] private float _intelligence = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private float _satisfaction = 1.0f;
	[SerializeField, Range(0.0f, 1.0f)] private float _sight = 0.5f;
	[SerializeField, Range(10.0f, 360.0f)] private float _fov = 50.0f;
	[SerializeField, Range(0, 100)] private int _lowerBodyDamage = 0;
	[SerializeField, Range(0, 100)] private int _upperBodyDamage = 0;
	[SerializeField] private float _depletionRate = 0.1f;
	[SerializeField] private float _replenishRate = 0.5f;

	public bool crawling { get; private set; }
	public bool feeding { get; set; }
	public float aggression { get { return _aggression; } set { _aggression = value; } }
	public float fov { get { return _fov; } }
	public float hearing { get { return _hearing; } }
	public float intelligence { get { return _intelligence; } }
	public float replenishRate { get { return _replenishRate; } }
	public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
	public float sight { get { return _sight; } }
	public float speed { get; set; }
	public int attackType { get; set; }
	public int health { get { return _health; } set { _health = value; } }
	public int seeking { get; set; }

	public AIZombieStateMachine() {
		speed = 0.0f;
		seeking = 0;
		feeding = false;
		crawling = false;
		attackType = 0;
	}

	public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager,
	                                int hitDirection = 0) {
		if ((GameSceneManager.instance != null) && (GameSceneManager.instance.bloodParticles != null)) {
			ParticleSystem sys = GameSceneManager.instance.bloodParticles;
			sys.transform.position = position;
			var settings = sys.main;
			settings.simulationSpace = ParticleSystemSimulationSpace.World;
			sys.Emit(60);
		}
		health -= damage;
		float hitStrength = force.magnitude;
		bool shouldRagdoll = hitStrength > 1.0f;
		if (health <= 0) { shouldRagdoll = true; }

		if (shouldRagdoll) {
			if (_currentState) {
				_currentState.OnExitState();
				_currentState = null;
				_currentStateType = AIStateType.None;
			}

			if (_navAgent) { _navAgent.enabled = false; }
			if (_animator) { _animator.enabled = false; }
			if (_collider) { _collider.enabled = false; }
			inMeleeRange = false;

			foreach (var body in _bodyParts) if (body) { body.isKinematic = false; }

			if (hitStrength > 1.0f) { bodyPart.AddForce(force, ForceMode.Impulse); }
		}
	}

	// ---------------------------------------------------------
	// Name	:	Update
	// Desc	:	Refresh the animator with up-to-date values for
	//			its parameters
	// ---------------------------------------------------------
	protected override void Update() {
		base.Update();

		if (_animator != null) {
			_animator.SetFloat(_speedHash, speed);
			_animator.SetBool(_feedingHash, feeding);
			_animator.SetInteger(_seekingHash, seeking);
			_animator.SetInteger(_attackHash, attackType);
		}

		_satisfaction = Mathf.Max(0, _satisfaction - (((_depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(speed, 3)));
	}
}