using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{

    private NavMeshAgent _navAgent = null;
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool hasPath = false, pathPending = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();

    private Animator _animator = null;
    private float _originalMaxSpeed = 0;
    // Use this for initialization
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        if (WaypointNetwork == null) return;
        if (_navAgent) _originalMaxSpeed = _navAgent.speed;
        SetNextDestination(false);
    }

    // Update is called once per frame
    void Update()
    {
        int turnOnSpot;
        hasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;
        PathStatus = _navAgent.pathStatus;
        Vector3 cross = Vector3.Cross(transform.forward, _navAgent.desiredVelocity.normalized);
        float horizontal = cross.y < 0 ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (_navAgent.desiredVelocity.magnitude < 0.5f && Vector3.Angle(transform.forward, _navAgent.desiredVelocity) > 10.0f)
        {
            _navAgent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            _navAgent.speed = _originalMaxSpeed;
            turnOnSpot = 0;
        }

        _animator.SetFloat("Vertical", _navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetInteger("TurnOnSpot", turnOnSpot);
        //if (_navAgent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(1.0f));
        //    return;
        //}

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
