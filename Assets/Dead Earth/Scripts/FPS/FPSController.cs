using UnityEngine;

public enum PlayerMoveStatus { NotMoving, Walking, Running, NotGrounded, Landing }

[System.Serializable]
public class CurveControlledBob {
    [SerializeField] private float _baseInterval = 1.0f;
    [SerializeField] private AnimationCurve _bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f), new Keyframe(1.5f, -1f), new Keyframe(2f, 0f));
    [SerializeField] private float _horizontalMultiplier = 0.01f;
    [SerializeField] private float _verticalMultiplier = 0.02f;
    [SerializeField] private float _verticalToHorizontalSpeedRatio = 2.0f;
    private float _xPlayHead, _yPlayHead, _curveEndTime;

    public Vector3 GetVectorOffset(float speed) {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalToHorizontalSpeedRatio;

        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;
        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;

        float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        return new Vector3(xPos, yPos, 0f);
    }

    public void Initialize() {
        _curveEndTime = _bobCurve[_bobCurve.length - 1].time;
        _xPlayHead = 0.0f;
        _yPlayHead = 0.0f;
    }
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {
    [SerializeField] private float _runSpeed = 4.5f;
    [SerializeField] private float _walkSpeed = 1.0f;
    [SerializeField] private float _jumpSpeed = 7.5f;
    [SerializeField] private float _stickToGroundForce = 5.0f;
    [SerializeField] private float _gravityMultiplier = 2.5f;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;
    [SerializeField] private CurveControlledBob _headBob = new CurveControlledBob();
    [SerializeField] private float _runStepLengthen = 0.75f;

    private Camera _camera = null;
    private bool _jumpButtonPressed = false;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _previouslyGrounded = false;
    private bool _isWalking = true;
    private bool _isJumping = false;
    private Vector3 _localSpaceCameraPos = Vector3.zero;

    //Timers
    private float _fallingTimer = 0.0f;

    private CharacterController _characterController = null;
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;

    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }

    protected void FixedUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool waswalking = _isWalking;
        _isWalking = !Input.GetKey(KeyCode.LeftShift);
        float speed = _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, vertical);
        if (_inputVector.sqrMagnitude > 1) {
            _inputVector.Normalize();
        }
        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;
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
        } else {
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);

        if (_characterController.velocity.magnitude > 0.01f) {
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(_characterController.velocity.magnitude * (_isWalking ? 1.0f : _runStepLengthen));
        } else {
            _camera.transform.localPosition = _localSpaceCameraPos;
        }
    }

    protected void Start() {
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;
        _localSpaceCameraPos = _camera.transform.localPosition;
        _movementStatus = PlayerMoveStatus.NotMoving;
        _fallingTimer = 0.0f;
        _mouseLook.Init(transform, _camera.transform);
        _headBob.Initialize();
    }

    protected void Update() {
        if (_characterController.isGrounded) {
            _fallingTimer = 0.0f;
        } else {
            _fallingTimer += Time.deltaTime;
        }

        if (Time.timeScale > Mathf.Epsilon) {
            _mouseLook.LookRotation(transform, _camera.transform);
        }

        if (!_jumpButtonPressed) {
            _jumpButtonPressed = Input.GetButtonDown("Jump");
        }

        if (!_previouslyGrounded && _characterController.isGrounded) {
            if (_fallingTimer > 0.5f) {
                // TODO: Add Landing Sound Effect
            }
            _moveDirection.y = 0.0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        } else if (!_characterController.isGrounded) {
            _movementStatus = PlayerMoveStatus.NotGrounded;
        } else if (_characterController.velocity.sqrMagnitude < 0.01f) {
            _movementStatus = PlayerMoveStatus.NotMoving;
        } else {
            _movementStatus = PlayerMoveStatus.Running;
        }
        _previouslyGrounded = _characterController.isGrounded;
    }
}