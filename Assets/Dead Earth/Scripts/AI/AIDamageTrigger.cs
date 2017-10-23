using UnityEngine;

public class AIDamageTrigger : MonoBehaviour {
	[SerializeField] private string _parameter = "";
	[SerializeField] private int _bloodParticlesBustAmount = 10;
	[SerializeField] private float _damageAmount = 0.10f;
	private Animator _animator;
	private int _parameterHash = -1;
	private AIStateMachine _stateMachine;
	private GameSceneManager _gameSceneManager;

	private void OnTriggerStay(Collider col) {
		if (!_animator) { return; }

		if (col.gameObject.CompareTag("Player") && (_animator.GetFloat(_parameterHash) > 0.9f)) {
			if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles) {
				ParticleSystem system = GameSceneManager.instance.bloodParticles;
				system.transform.position = transform.position;
				system.transform.rotation = Camera.main.transform.rotation;
				ParticleSystem.MainModule main = system.main;
				main.simulationSpace = ParticleSystemSimulationSpace.World;
				system.Emit(_bloodParticlesBustAmount);
			}

			if (_gameSceneManager != null) {
				PlayerInfo info = _gameSceneManager.GetPlayerInfo(col.GetInstanceID());
				if ((info != null) && (info.characterManager != null)) { info.characterManager.TakeDamage(_damageAmount); }
			}
		}
	}

	private void Start() {
		_stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
		if (_stateMachine != null) { _animator = _stateMachine.animator; }
		_parameterHash = Animator.StringToHash(_parameter);
		_gameSceneManager = GameSceneManager.instance;
	}
}