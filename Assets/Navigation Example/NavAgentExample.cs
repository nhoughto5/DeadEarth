using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{

    private NavMeshAgent _navAgent = null;
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool hasPath = false, pathPending = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();

    // Use this for initialization
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        if (WaypointNetwork == null) return;
        SetNextDestination(false);
    }

    // Update is called once per frame
    void Update()
    {
        hasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;
        PathStatus = _navAgent.pathStatus;

        if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
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
