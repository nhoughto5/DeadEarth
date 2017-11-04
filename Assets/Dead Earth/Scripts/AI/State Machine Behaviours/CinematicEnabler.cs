using UnityEngine;

public class CinematicEnabler : AIStateMachineLink {
	public bool OnEnter = false;
	public bool OnExit = false;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex) {
		if (_stateMachine) { _stateMachine.cinematicEnabled = OnEnter; }
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex) {
		if (_stateMachine) { _stateMachine.cinematicEnabled = OnExit; }
	}
}