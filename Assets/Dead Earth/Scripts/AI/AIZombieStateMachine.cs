using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIBoneControlType {
	Animated,
	Ragdoll,
	RagdollToAnim
}

public class BodyPartSnapshot {
	public Transform transform;
	public Vector3 position;
	public Quaternion rotation, localRotation;
}

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine {
	private readonly int _attackHash = Animator.StringToHash("Attack");
	private readonly int _feedingHash = Animator.StringToHash("Feeding");
	private readonly int _seekingHash = Animator.StringToHash("Seeking");
	private readonly int _speedHash = Animator.StringToHash("Speed");
	private readonly int _crawlingHash = Animator.StringToHash("Crawling");
	private readonly int _hitTriggerHash = Animator.StringToHash("Hit");
	private readonly int _hitTypeHash = Animator.StringToHash("HitType");
	private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
	private readonly List<BodyPartSnapshot> _bodyPartSnapshots = new List<BodyPartSnapshot>();
	private float _raddollEndTime = float.MinValue;
	private Vector3 _ragdollHipPosition, _ragdollFeetPosition, _ragdollHeadPosition;
	private IEnumerator _reanimationCoroutine;
	private float _mecanimTransitionTime = 0.1f;

	[SerializeField] private readonly float _depletionRate = 0.1f;
	[SerializeField] private readonly float _replenishRate = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private float _aggression = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private readonly float _hearing = 1.0f;
	[SerializeField, Range(0.0f, 1.0f)] private readonly float _intelligence = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)] private float _satisfaction = 1.0f;
	[SerializeField, Range(0.0f, 1.0f)] private readonly float _sight = 0.5f;
	[SerializeField, Range(10.0f, 360.0f)] private readonly float _fov = 50.0f;
	[SerializeField, Range(0, 100)] private int _health = 100;
	[SerializeField, Range(0, 100)] private int _lowerBodyDamage;
	[SerializeField, Range(0, 100)] private int _upperBodyDamage;
	[SerializeField, Range(0.0f, 1.0f)] private int _upperBodyThreshold = 30;
	[SerializeField, Range(0.0f, 1.0f)] private int _limpThreshold = 30;
	[SerializeField, Range(0.0f, 1.0f)] private readonly int _crawlThreshold = 90;
	[SerializeField] private float _reanimationBlendTime = 1.5f;
	[SerializeField] private float _reanimationWaitTime = 3.0f;
	public float aggression { get { return _aggression; } set { _aggression = value; } }
	public int attackType { get; set; }

	public bool crawling { get; private set; }
	public bool feeding { get; set; }
	public float fov { get { return _fov; } }
	public int health { get { return _health; } set { _health = value; } }
	public float hearing { get { return _hearing; } }
	public float intelligence { get { return _intelligence; } }
	public bool isCrawling { get { return _lowerBodyDamage >= _crawlThreshold; } }
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

	public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager,
	                                int hitDirection = 0) {
		if ((GameSceneManager.instance != null) && (GameSceneManager.instance.bloodParticles != null)) {
			ParticleSystem sys = GameSceneManager.instance.bloodParticles;
			sys.transform.position = position;
			ParticleSystem.MainModule settings = sys.main;
			settings.simulationSpace = ParticleSystemSimulationSpace.World;
			sys.Emit(60);
		}
		float hitStrength = force.magnitude;

		if (_boneControlType == AIBoneControlType.Ragdoll) {
			if (bodyPart != null) {
				if (hitStrength > 1.0f) { bodyPart.AddForce(force, ForceMode.Force); }
				if (bodyPart.CompareTag("Head")) { _health = Mathf.Max(_health - damage, 0); }
				else if (bodyPart.CompareTag("Upper Body")) { _upperBodyDamage += damage; }
				else if (bodyPart.CompareTag("Lower Body")) { _lowerBodyDamage += damage; }
				UpdateAnimatorDamage();
				if (_health > 0) {
					if (_reanimationCoroutine != null) { StopCoroutine(_reanimationCoroutine); }
					_reanimationCoroutine = Reanimate();
					StartCoroutine(_reanimationCoroutine);
				}
			}
		}

		Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);
		Vector3 hitLocPos = transform.InverseTransformPoint(position); // Get local space position of hit

		bool shouldRagdoll = hitStrength > 1.0f;

		if (bodyPart != null) {
			if (bodyPart.CompareTag("Head")) {
				_health = Mathf.Max(_health - damage, 0);
				if (health == 0) { shouldRagdoll = true; }
			}
			else if (bodyPart.CompareTag("Upper Body")) {
				_upperBodyDamage += damage;
				UpdateAnimatorDamage();
			}
			else if (bodyPart.CompareTag("Lower Body")) {
				_lowerBodyDamage += damage;
				UpdateAnimatorDamage();
				shouldRagdoll = true;
			}
		}

		if ((_boneControlType != AIBoneControlType.Animated) || isCrawling || cinematicEnabled || (attackerLocPos.z < 0)) {
			shouldRagdoll = true;
		}
		//Melee weapon hit direction
		if (!shouldRagdoll) {
			float angle = 0.0f;
			if (hitDirection == 0) {
				Vector3 vecToHit = (position - transform.position).normalized;
				angle = AIState.FindSignedAngle(vecToHit, transform.forward);
			}

			// Decide which damage animation to play
			int hitType = 0;
			if (bodyPart.gameObject.CompareTag("Head")) {
				if ((angle < -10) || (hitDirection == -1)) { hitType = 1; }
				else if ((angle > 10) || (hitDirection == 1)) { hitType = 3; }
				else { hitType = 2; }
			}
			else if (bodyPart.gameObject.CompareTag("Upper Body")) {
				if ((angle < -20) || (hitDirection == -1)) { hitType = 4; }
				else if ((angle > 20) || (hitDirection == 1)) { hitType = 6; }
				else { hitType = 5; }
			}
			if (_animator) {
				_animator.SetInteger(_hitTypeHash, hitType);
				_animator.SetTrigger(_hitTriggerHash);
			}
		}
		else {
			if (_currentState) {
				_currentState.OnExitState();
				_currentState = null;
				_currentStateType = AIStateType.None;
			}

			if (_navAgent) { _navAgent.enabled = false; }
			if (_animator) { _animator.enabled = false; }
			if (_collider) { _collider.enabled = false; }
			inMeleeRange = false;

			foreach (Rigidbody body in _bodyParts) if (body) { body.isKinematic = false; }

			if (hitStrength > 1.0f) { bodyPart.AddForce(force, ForceMode.Impulse); }
			_boneControlType = AIBoneControlType.Ragdoll;
			if (_health > 0) {
				if (_reanimationCoroutine != null) { StopCoroutine(_reanimationCoroutine); }
				_reanimationCoroutine = Reanimate();
				StartCoroutine(_reanimationCoroutine);
			}
		}
	}

	protected override void Start() {
		base.Start();

		if (_rootBone != null) {
			Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
			foreach (Transform trans in transforms) {
				BodyPartSnapshot snapShot = new BodyPartSnapshot {
					transform = trans
				};
				_bodyPartSnapshots.Add(snapShot);
			}
		}

		UpdateAnimatorDamage();
	}

	protected void UpdateAnimatorDamage() {
		if (_animator != null) { _animator.SetBool(_crawlingHash, isCrawling); }
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

	private IEnumerator Reanimate() { }
}