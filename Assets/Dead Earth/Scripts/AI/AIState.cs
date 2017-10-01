using UnityEngine;

public abstract class AIState : MonoBehaviour {
    protected AIStateMachine _stateMachine;

    public static void ConvertSphereColliderToWorldSpace(SphereCollider col, out Vector3 pos, out float radius) {
        pos = Vector3.zero;
        radius = 0.0f;
        if (col == null)
            return;

        pos = col.transform.position;
        pos.x += col.center.x * col.transform.lossyScale.x;
        pos.y += col.center.y * col.transform.lossyScale.y;
        pos.z += col.center.z * col.transform.lossyScale.z;

        radius = Mathf.Max(col.radius * col.transform.lossyScale.x, col.radius * col.transform.lossyScale.y);
        radius = Mathf.Max(radius, col.radius * col.transform.lossyScale.z);
    }

    public static float FindSignedAngle(Vector3 from, Vector3 to) {
        if (from == to)
            return 0.0f;

        float angle = Vector3.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);

        angle *= Mathf.Sign(cross.y);

        return angle;
    }

    public abstract AIStateType GetStateType();

    public virtual void OnAnimatorIKUpdated() {
    }

    public virtual void OnAnimatorUpdated() {
        if (_stateMachine.useRootPosition) {
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;
        }

        if (_stateMachine.useRootRotation) {
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
        }
    }

    public virtual void OnDestinationReached(bool isReached) {
    }

    // Default Handlers
    public virtual void OnEnterState() { }

    public virtual void OnExitState() {
    }

    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) {
    }

    public abstract AIStateType OnUpdate();

    public virtual void SetStateMachine(AIStateMachine machine) {
        _stateMachine = machine;
    }
}