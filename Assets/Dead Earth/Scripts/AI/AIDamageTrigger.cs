using UnityEngine;

public class AIDamageTrigger : MonoBehaviour {
	private Animator _animator;
	[SerializeField] private string _parameter = "";
	[SerializeField] private int _bloodParticlesBustAmount = 10;

	private int _parameterHash = -1;
	private AIStateMachine _stateMachine;

	private void OnTriggerStay(Collider col) {
		if (!_animator) { return; }

		if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f) {
			if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles) {
				ParticleSystem system = GameSceneManager.instance.bloodParticles;
				system.transform.position = transform.position;
				system.transform.rotation = Camera.main.transform.rotation;
				ParticleSystem.MainModule main = system.main;
				main.simulationSpace = ParticleSystemSimulationSpace.World;
				system.Emit(_bloodParticlesBustAmount);
			}
			Debug.Log("Player Being Damaged");
		}
	}

	private void Start() {
		_stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
		if (_stateMachine != null) { _animator = _stateMachine.animator; }
		_parameterHash = Animator.StringToHash(_parameter);
	}
}