using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZoneTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider col) {
		AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(col.GetInstanceID());
		if (machine) { machine.inMeleeRange = true; }
	}

	void OnTriggerExit(Collider col) {
		AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(col.GetInstanceID());
		if (machine) { machine.inMeleeRange = false; }
	}
}
