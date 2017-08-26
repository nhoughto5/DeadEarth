using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour
{

    private NavMeshAgent _navAgent = null;
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool hasPath = false, pathPending = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();
    public bool mixedMode = true;


    private Animator _animator = null;
    private float smoothAngle = 0.0f;
    private const float DEGREES_PER_SECOND = 80.0f;
    // Use this for initialization
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updateRotation = false;
        _animator = GetComponent<Animator>();
        if (WaypointNetwork == null) return;
        SetNextDestination(false);
    }

    // Update is called once per frame
    void Update()
    {
        hasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;
        PathStatus = _navAgent.pathStatus;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;
        smoothAngle = Mathf.MoveTowardsAngle(smoothAngle, angle, DEGREES_PER_SECOND * Time.deltaTime);
        _animator.SetFloat("Angle", smoothAngle);
        float speed = localDesiredVelocity.z;
        _animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);


        if (_navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if(!mixedMode || (mixedMode && Mathf.Abs(angle) < 80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }
        }


        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !pathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }
        else if (_navAgent.isPathStale)
        {
            // Recompute
            SetNextDestination(false);
        }
    }

    private void OnAnimatorMove()
    {
        //transform.rotation = _animator.rootRotation;
        if (mixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
        {
            transform.rotation = _animator.rootRotation;
        }
        _navAgent.velocity = (Time.deltaTime != 0)? _animator.deltaPosition / Time.deltaTime : Vector3.zero;
    }

    // False triggers a re-computation of the current target
    void SetNextDestination(bool increment)
    {
        if (!WaypointNetwork) return;
        int incStep = increment ? 1 : 0;
        Transform nextWayPointTransform = null;

        int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        nextWayPointTransform = WaypointNetwork.Waypoints[nextWaypoint];
        if (nextWayPointTransform != null)
        {
            CurrentIndex = nextWaypoint;
            _navAgent.destination = nextWayPointTransform.position;
            return;
        }

        CurrentIndex++;
    }

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);

        float time = 0.0f;
        while (time <= duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;

            //Allow a single frame to be rendered.
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}
