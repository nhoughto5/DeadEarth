using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor {

    void OnSceneGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;
        for (int i = 0; i < network.Waypoints.Count; i++)
        {
            if (network.Waypoints[i] != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = new Color(0.73F, 0.84F, 0.815F, 1);
                Handles.Label(network.Waypoints[i].position, i.ToString() + ": " + network.Waypoints[i].name, style);
            }
        }
        Vector3[] linePoints = new Vector3[network.Waypoints.Count + 1];
        for (int i = 0; i <= network.Waypoints.Count; i++)
        {
            int index = i != network.Waypoints.Count ? i : 0;
            if (network.Waypoints[index] != null)
            {
                linePoints[i] = network.Waypoints[index].position;
            }
            else
            {
                linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
        }

        Handles.color = Color.cyan;
        Handles.DrawPolyLine(linePoints);
    }
}
