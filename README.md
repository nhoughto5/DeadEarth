# **Dead Earth**
##### A Udemy Course Diary

This repository contains the code base develped while followiung the which can be found [here](https://www.udemy.com/build-your-own-first-person-shooter-survival-game-in-unity). This course provided an excellent look into technologies and strategies used to build modern video games. Dead Earth is a first person shooter built with [Unity 5](https://unity3d.com/) and C#.

## Course Discussion
The following does not follow the lesson plan stages directly but somewhat aligns with the progression of the course material.

#### Introduction
The course begins by introducing the idea behind the game as well as some basics about Unity. 
#### Navigation and Path Finding
The next seven videos all focus on the development of a system by which the artificial intelligence (AI) will be able to navigate non-player characters (NPC) around the environment.

Video 4 of the series introduces and thoroughly describes the [A* algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm). This is the method used by unity to compute not only possible traverals, but the most optimized route for an agent to take to get from A to B. 

The idea of a [navigation mesh](https://en.wikipedia.org/wiki/Navigation_mesh) is introduced. In essence it is a fine grid which describes the areas of the map which can be traveresed by an agent. Each grid is either accessible or not. Unity provides a really nice means of automatically computing this grid for a given map. Video 5 of the series describes how to get Unity to generate this map as well as how to tweek the setting to optimize for the game's needs. The generaor's settings are discussed:

- **Agent Radius:** How close an agent can get to a wall/object.
- **Agent Height:** If a hanging object is lower than the agent height the agent can not traverse under it.
- **Max Slope:** The max angle at which an agent can traverse. Beyond this is too steep.
- **Step Height:** Polygons in the mesh which are disjointed can not be traversed. This parameter joins polygons which are at a height lower than the value. This allows for the traversal of features such as stairs.
- **Drop Height:** Similar idea to the step height.
- **Jump Distance:** The max horizontal distance two disjointed polygons can be traversed. Allows an agent to jump over small gaps.
- **Voxelization:** Similar to the notion of filling pixels on the screen however 3D. Finds 3D cubes which are traversable by an agent via interpolatino from the meshes of the map. 
- **Height Mesh:** Includes more accurate data about the height of the map. Used to prevent agent floating.

##### Waypoints
To set up way points go to the 'GameObject' menu and select 'Create Empty'. This creates an empty game object, rename it to 'Waypoints'. It is adviseable to immediately position this object at 0,0,0. This can be done in the inspector.

To create the first waypoint, add a child game object by selecting 'Create Empty Child' from the 'Game Object' menu when the 'Waypoints' object is selected. Rename to 'Waypoint 1'. 

To add a graphic for the system to render for the way point, find a simple flag png and add to project. With the child object selected, in the inspector pane click the cube icon in the top left corner of the menu. Hit 'Other' in the menu that appears and find your png. To create more, right click and use 'Duplicate'.

To create a waypoint network, create a new game object using 'Create Empty'. Create a C# script called 'AIWaypointsNetwork.cs' and add it to the network object.

Edit the script to contain a list of tranform objects:

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class AIWaypointNetwork : MonoBehaviour {
        public List<Transform> Waypoints = new List<Transform>();
    }

##### Waypoint Paths
In order to get unity to render our waypoint paths we must create an editor script which modifies the way the unity editor renders components. An editor script is a C# script that MUST be within a folder named 'Editor'. It does not matter where this folder is in the heirarchy, infact you can have multiple. Unity treats each as one single folder. 

An editor script class extends the [Editor](https://docs.unity3d.com/ScriptReference/Editor.html) class. The details of the complete script can be seen [here](https://github.com/nhoughto5/DeadEarth/blob/master/Assets/Dead%20Earth/Editor/AIWaypointNetworkEditor.cs).

A cylinder is then added to the scene to represent a basic user agent. This is done via the 'GameObject' -> '3D Object' menu. A nav-mesh component is then added to create movement parameters.

A new class, NavAgentExample.cs is then made. This class controls selection and of new way points and initiates movement. This class relies on the '[hasPath](https://docs.unity3d.com/412/Documentation/ScriptReference/NavMeshAgent-hasPath.html)' method. This method is a-synchronous. In other words, when a destination is set it takes some time for the path to be computed. The hasPath method returns true when complete. This method can be used in conjunction with the 'pathPending' method to build robust logic. A check can be added against the nav agent's parameter [pathStatus](https://docs.unity3d.com/530/Documentation/ScriptReference/NavMeshPathStatus.html) to determine if the path is complete, partial or invalid.

Partial paths occur when a waypoint is not reachable. The agent will get as close as possible then declare that it has completed that path, and then move on.

Off-mesh links can be generated in the nav-mesh by setting a jump distance greater than 0. Any gaps less than this will become traversable. [There is a bug in Unity](https://issuetracker.unity3d.com/issues/navmeshagent-dot-haspath-is-false-when-agent-is-crossing-an-offmeshlink) where the 'hasPath' Boolean is set to false immediately after traversing an off-mesh link. This can break correct logic. It can be circumvented by using the [remainingDistance](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-remainingDistance.html) method in conjunction with the stopping distance of the agent:

    _navAgent.remainingDistance <= _navAgent.stoppingDistance

**To create a manual off-mesh link:**
1. Create two empty game objects (not children), give names (ex: entrance, exit).
2. Place over the gap you wish the link.
3. On one of the game objects add the 'Off-mesh link' component.
4. Drag that objects transform component (also in the inspector) into the start/end parameter of the 'Off-mesh link' component.
5. Drag the other game object into the remaining start/end parameter.

**To Customize the traversal of off-mesh links:**
1. In the agent, turn off 'Auto-traverse off mesh links'.
2. In the agent's update detect [isOnOffMeshLink](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-isOnOffMeshLink.html).
3. Call a [CoRoutine](https://docs.unity3d.com/Manual/Coroutines.html). (A CoRoutine is a method which can run over multiple update calls).
4. Create an 'AnimationCurve' objct as a member of the update script.
5. Add an evaluation of the AnimationCurve to a lerp of the start->end of the traversal (see example below).
6. In the Unity Editor, on the nav agent's update script modify the 'AnimationCurve' object to a desired cuve (ex. parabola)

Ex:

    _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;


**NavMesh Obstacles**
Similar to a collider object but used for the navigation system. Even though objects may have a collider on them, an agent on a navigation path will traverse through them without a NavMesh Obstacle (the collider is linked to the physics system, not the navigation system). 

Though a NavMesh Obstacle will stop a nav-agent from passing through an object it will not cause the agent to recompute it's path. This is because the object is not part of the nav mesh. With Unity 5+ the nav mesh obstacle has an option called 'carve'. This option causes the object to carve out a silhouette of itself from the nav mesh.


##### Notes and Definitions 
- [Voxel](http://whatis.techtarget.com/definition/voxel): A voxel is a unit of graphic information that defines a point in three-dimensional space. A cube of space or a polygon on a 2D mesh.  
- [Off-Mesh Links](https://docs.unity3d.com/Manual/class-OffMeshLink.html): Generated shortcuts which allow the traversal over broken voxels.
- [AI.NavMeshAgent](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html) Documentation
