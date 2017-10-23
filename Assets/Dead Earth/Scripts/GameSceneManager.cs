using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour {
	public static GameSceneManager instance {
		get {
			if (_instance == null) { _instance = (GameSceneManager) FindObjectOfType(typeof(GameSceneManager)); }
			return _instance;
		}
	}

	public ParticleSystem bloodParticles { get { return _bloodParticles; } }

	private static GameSceneManager _instance;
	[SerializeField] private ParticleSystem _bloodParticles = null;
	private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();
	private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

	public AIStateMachine GetAIStateMachine(int key) {
		AIStateMachine machine = null;
		if (_stateMachines.TryGetValue(key, out machine)) { return machine; }

		return null;
	}

	public PlayerInfo GetPlayerInfo(int key) {
		PlayerInfo player = null;
		if (_playerInfos.TryGetValue(key, out player)) { return player; }

		return null;
	}

	public void RegisterAIStateMachine(int key, AIStateMachine stateMachine) {
		if (!_stateMachines.ContainsKey(key)) { _stateMachines[key] = stateMachine; }
	}

	public void RegisterPlayerInfo(int key, PlayerInfo playerInfo) {
		if (!_playerInfos.ContainsKey(key)) { _playerInfos[key] = playerInfo; }
	}
}

public class PlayerInfo {
	public Camera camera = null;
	public CharacterManager characterManager = null;
	public Collider collider = null;
	public CapsuleCollider meleeTrigger = null;
}