using UnityEngine;

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine {
	[SerializeField, Range(0.0f, 1.0f)] private float _aggression = 0.5f;

	private int _attackHash = Animator.StringToHash("Attack");

	[SerializeField] private float _depletionRate = 0.1f;

	// Private
	private int _feedingHash = Animator.StringToHash("Feeding");

	// Inspector Assigned
	[SerializeField, Range(10.0f, 360.0f)] private float _fov = 50.0f;

	[SerializeField, Range(0, 100)] private int _health = 100;
	[SerializeField, Range(0.0f, 1.0f)] private float _hearing = 1.0f;
	[SerializeField, Range(0.0f, 1.0f)] private float _intelligence = 0.5f;
	[SerializeField] private float _replenishRate = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private float _satisfaction = 1.0f;

	// Hashes
	private int _seekingHash = Animator.StringToHash("Seeking");

	[SerializeField, Range(0.0f, 1.0f)] private float _sight = 0.5f;
	private int _speedHash = Animator.StringToHash("Speed");

	// Public Properties
	public float aggression { get { return _aggression; } set { _aggression = value; } }

	public int attackType { get; set; }
	public bool crawling { get; private set; }
	public bool feeding { get; set; }
	public float fov { get { return _fov; } }
	public int health { get { return _health; } set { _health = value; } }
	public float hearing { get { return _hearing; } }
	public float intelligence { get { return _intelligence; } }
	public float replenishRate { get { return _replenishRate; } }
	public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
	public int seeking { get; set; }
	public float sight { get { return _sight; } }
	public float speed { get; set; }

	public AIZombieStateMachine() {
		speed = 0.0f;
		seeking = 0;
		feeding = false;
		crawling = false;
		attackType = 0;
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