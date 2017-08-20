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

    }

    // False triggers a re-computation of the current target
    void SetNextDestination(bool increment)
    {
        if (!WaypointNetwork) return;
        int incStep = increment ? 1 : 0;
        Transform nextWayPointTransform = null;

        while (nextWayPointTransform == null)
        {
            int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
            nextWayPointTransform = WaypointNetwork.Waypoints[nextWaypoint];
            if (nextWayPointTransform != null)
            {
                CurrentIndex = nextWaypoint;
                _navAgent.destination = nextWayPointTransform.position;
                return;
            }
        }
        CurrentIndex++;
    }
}
