using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState
{

    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AIZombieStateMachine _zombieStateMachine = null;

    void Awake()
    {
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        _bodyPartLayer = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator");
        _visualLayerMask = LayerMask.NameToLayer("AI Body Part");
    }

    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if (stateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(stateMachine);
            _zombieStateMachine = (AIZombieStateMachine)stateMachine;
        }
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (_zombieStateMachine == null) return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType curType = _zombieStateMachine.VisualThreat.type;
            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(_zombieStateMachine.sensorPosition, other.transform.position);
                if (curType != AITargetType.Visual_Player ||
                    (curType == AITargetType.Visual_Player && distance < _zombieStateMachine.VisualThreat.distance))
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                    {
                        // Yep...it's close and in our FOV and we have line of sight so store as the current most dangerous threat
                        _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position,
                            distance);
                    }
                }
            }
            else if (other.CompareTag("Flashlight") && curType != AITargetType.Visual_Player)
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;
                float distanceToThreat = Vector3.Distance(_zombieStateMachine.sensorPosition,
                    flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToThreat / zSize;
                if (aggrFactor < _zombieStateMachine.sight && aggrFactor <= _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }
            }
            else if (other.CompareTag("AI Sound Emitter"))
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) return;
                Vector3 agentSensorPosition = _zombieStateMachine.sensorPosition;
                Vector3 soundPos;
                float soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);
                float distanceToThreat = (soundPos - agentSensorPosition).magnitude;
                float distanceFactor = (distanceToThreat / soundRadius);

                distanceFactor += distanceFactor * (1.0f - _zombieStateMachine.hearing);

                // Too far away
                if (distanceFactor > 1.0f) return;
                if (distanceToThreat < _zombieStateMachine.AudioThreat.distance)
                {
                    _zombieStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                }
            }
            else if (other.CompareTag("AI Food") && curType != AITargetType.Visual_Player && curType != AITargetType.Visual_Light 
                && _zombieStateMachine.satisfaction <= 0.9f && _zombieStateMachine.AudioThreat.type == AITargetType.None)
            {
                float distanceToThreat = Vector3.Distance(other.transform.position, _zombieStateMachine.sensorPosition);
                if (distanceToThreat < _zombieStateMachine.VisualThreat.distance)
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, _visualLayerMask))
                    {
                        _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();
        if (_zombieStateMachine == null) return false;


        Vector3 head = _zombieStateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > _zombieStateMachine.fov * 0.5f)
        {
            return false;
        }
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized,
            _zombieStateMachine.sensorRadius * _zombieStateMachine.sight, layerMask);

        float closestColliderDitance = float.MaxValue;
        Collider closestCollider = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.distance < closestColliderDitance)
            {
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    // Check it is not our body part
                    if (_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        closestColliderDitance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    closestColliderDitance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }
        //Check if have line of sight
        if (closestCollider && closestCollider.gameObject == other.gameObject)
        {
            return true;
        }
        return false;
    }
}
