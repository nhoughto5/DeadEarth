using UnityEngine;

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine {

    // Private
    private bool _feeding = false;
    private bool _crawling = false;
    [SerializeField] [Range(0.0f, 1.0f)] private float _aggression = 0.5f;
    [SerializeField] private float _depletionRate = 0.1f;
    private int _seeking = 0;
    private int _attackType = 0;

    // Inspector Assigned
    [SerializeField] [Range(10.0f, 360.0f)] private float _fov = 50.0f;
    [SerializeField] [Range(0, 100)] private int _health = 100;
    [SerializeField] [Range(0.0f, 1.0f)] private float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] private float _intelligence = 0.5f;
    [SerializeField] private float _replenishRate = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] private float _satisfaction = 1.0f;

    // Hashes
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _speedHash = Animator.StringToHash("Speed");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");
    [SerializeField] [Range(0.0f, 1.0f)] private float _sight = 0.5f;
    private float _speed = 0.0f;

    // Public Properties
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public bool crawling { get { return _crawling; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public float fov { get { return _fov; } }
    public int health { get { return _health; } set { _health = value; } }
    public float hearing { get { return _hearing; } }
    public float intelligence { get { return _intelligence; } }
    public float replenishRate { get { return _replenishRate; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float sight { get { return _sight; } }
    public float speed {get { return _speed; }set { _speed = value; }}

    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	Refresh the animator with up-to-date values for
    //			its parameters
    // ---------------------------------------------------------
    protected override void Update() {
        base.Update();

        if (_animator != null) {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
        }
    }
}