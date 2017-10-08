using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public delegate void CurveControlledBobCallback();

public enum CurveControlledBobCallbackType {
	Horizontal,
	Vertical
}

public enum PlayerMoveStatus {
	NotMoving,
	Walking,
	Running,
	NotGrounded,
	Landing,
	Crouching
}

[Serializable]
public class CurveControlledBob {
	[SerializeField] private float _baseInterval = 1.0f;

	[SerializeField] private AnimationCurve _bobCurve = new AnimationCurve(new Keyframe(0f, 0f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, 0f),
		new Keyframe(1.5f, -1f),
		new Keyframe(2f, 0f));

	private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();
	[SerializeField] private float _horizontalMultiplier = 0.01f;
	[SerializeField] private float _verticalMultiplier = 0.02f;
	[SerializeField] private float _verticalToHorizontalSpeedRatio = 2.0f;
	private float _xPlayHead, _yPlayHead, _curveEndTime, _prevXPlayHead, _prevYPlayHead;

	public Vector3 GetVectorOffset(float speed) {
		_xPlayHead += (speed * Time.deltaTime) / _baseInterval;
		_yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalToHorizontalSpeedRatio;

		if (_xPlayHead > _curveEndTime) { _xPlayHead -= _curveEndTime; }
		if (_yPlayHead > _curveEndTime) { _yPlayHead -= _curveEndTime; }

		for (int i = 0; i < _events.Count; ++i) {
			CurveControlledBobEvent ev = _events[i];
			if (ev != null) {
				if (ev.Type == CurveControlledBobCallbackType.Vertical) {
					if (((_prevYPlayHead < ev.Time) && (_yPlayHead >= ev.Time)) ||
					    ((_prevYPlayHead > _yPlayHead) && ((ev.Time > _prevYPlayHead) || (ev.Time <= _yPlayHead)))) { ev.Function(); }
				}
				else {
					if (((_prevXPlayHead < ev.Time) && (_xPlayHead >= ev.Time)) ||
					    ((_prevXPlayHead > _xPlayHead) && ((ev.Time > _prevXPlayHead) || (ev.Time <= _xPlayHead)))) { ev.Function(); }
				}
			}
		}

		float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
		float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;
		_prevXPlayHead = _xPlayHead;
		_prevYPlayHead = _yPlayHead;

		return new Vector3(xPos, yPos, 0f);
	}

	public void Initialize() {
		_curveEndTime = _bobCurve[_bobCurve.length - 1].time;
		_xPlayHead = 0.0f;
		_yPlayHead = 0.0f;
		_prevXPlayHead = _prevYPlayHead = 0.0f;
	}

	public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type) {
		CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
		ccbeEvent.Time = time;
		ccbeEvent.Function = function;
		ccbeEvent.Type = type;
		_events.Add(ccbeEvent);
		_events.Sort(delegate(CurveControlledBobEvent t1, CurveControlledBobEvent t2) { return t1.Time.CompareTo(t2.Time); });
	}
}

[Serializable]
public class CurveControlledBobEvent {
	public CurveControlledBobCallback Function;
	public float Time;
	public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {
	//===============
	public List<AudioSource> audioSources = new List<AudioSource>();

	private int _audioToUse;
	//==============

	private Camera _camera;
	private CharacterController _characterController;

	private float _controllerHeight;

	[SerializeField] private float _crouchSpeed = 1.0f;

	//Timers
	private float _fallingTimer;

	[SerializeField] private GameObject _flashLight = null;
	[SerializeField] private float _gravityMultiplier = 2.5f;
	[SerializeField] private CurveControlledBob _headBob = new CurveControlledBob();

	private Vector2 _inputVector = Vector2.zero;
	private bool _isCrouching;
	private bool _isJumping;
	private bool _isWalking = true;
	private bool _jumpButtonPressed;
	[SerializeField] private float _jumpSpeed = 7.5f;
	private Vector3 _localSpaceCameraPos = Vector3.zero;
	[SerializeField] private MouseLook _mouseLook;
	private Vector3 _moveDirection = Vector3.zero;
	private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;
	private bool _previouslyGrounded;
	[SerializeField] private float _runSpeed = 4.5f;
	[SerializeField] private float _runStepLengthen = 0.75f;
	[SerializeField] private float _stickToGroundForce = 5.0f;
	[SerializeField] private float _walkSpeed = 2.0f;
	public PlayerMoveStatus movementStatus { get { return _movementStatus; } }

	public float runSpeed { get { return _runSpeed; } }

	public float walkSpeed { get { return _walkSpeed; } }

	protected void FixedUpdate() {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		bool waswalking = _isWalking;
		_isWalking = !Input.GetKey(KeyCode.LeftShift);
		float speed = _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : _runSpeed;
		_inputVector = new Vector2(horizontal, vertical);
		if (_inputVector.sqrMagnitude > 1) { _inputVector.Normalize(); }
		Vector3 desiredMove = (transform.forward * _inputVector.y) + (transform.right * _inputVector.x);
		RaycastHit hitInfo;
		if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1)) {
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
		}
		_moveDirection.x = desiredMove.x * speed;
		_moveDirection.z = desiredMove.z * speed;

		if (_characterController.isGrounded) {
			_moveDirection.y = -_stickToGroundForce;
			if (_jumpButtonPressed) {
				_moveDirection.y = _jumpSpeed;
				_jumpButtonPressed = false;
				_isJumping = true;
				// TODO: Play jumping Sound
			}
		}
		else { _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime; }
		_characterController.Move(_moveDirection * Time.fixedDeltaTime);

		Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);
		if (speedXZ.magnitude > 0.01f) {
			_camera.transform.localPosition = _localSpaceCameraPos +
			                                  _headBob.GetVectorOffset(speedXZ.magnitude *
			                                                           (_isCrouching || _isWalking ? 1.0f : _runStepLengthen));
		}
		else { _camera.transform.localPosition = _localSpaceCameraPos; }
	}

	protected void Start() {
		_characterController = GetComponent<CharacterController>();
		_controllerHeight = _characterController.height;
		_camera = Camera.main;
		_localSpaceCameraPos = _camera.transform.localPosition;
		_movementStatus = PlayerMoveStatus.NotMoving;
		_fallingTimer = 0.0f;
		_mouseLook.Init(transform, _camera.transform);
		_headBob.Initialize();
		_headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);
		if (_flashLight) { _flashLight.SetActive(false); }
	}

	protected void Update() {
		if (_characterController.isGrounded) { _fallingTimer = 0.0f; }
		else { _fallingTimer += Time.deltaTime; }

		if (Time.timeScale > Mathf.Epsilon) { _mouseLook.LookRotation(transform, _camera.transform); }

		if (Input.GetButtonDown("Flashlight")) { if (_flashLight) { _flashLight.SetActive(!_flashLight.activeSelf); } }
		if (!_jumpButtonPressed && !_isCrouching) { _jumpButtonPressed = Input.GetButtonDown("Jump"); }

		if (Input.GetButtonDown("Crouch")) {
			_isCrouching = !_isCrouching;
			_characterController.height = _isCrouching ? _controllerHeight / 2.0f : _controllerHeight;
		}
		if (!_previouslyGrounded && _characterController.isGrounded) {
			if (_fallingTimer > 0.5f) {
				// TODO: Add Landing Sound Effect
			}
			_moveDirection.y = 0.0f;
			_isJumping = false;
			_movementStatus = PlayerMoveStatus.Landing;
		}
		else if (!_characterController.isGrounded) { _movementStatus = PlayerMoveStatus.NotGrounded; }
		else if (_characterController.velocity.sqrMagnitude < 0.01f) { _movementStatus = PlayerMoveStatus.NotMoving; }
		else { _movementStatus = _isWalking ? PlayerMoveStatus.Walking : PlayerMoveStatus.Running; }
		_previouslyGrounded = _characterController.isGrounded;
	}

	private void PlayFootStepSound() {
		if (_isCrouching) { return; }

		audioSources[_audioToUse].Play();
		_audioToUse = _audioToUse == 0 ? 1 : 0;
	}
}