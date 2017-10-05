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
To set up way points go to the 'GameObject' menu and select 'Create Empty'. This creates an empty game object, rename it to 'Waypoints'. It is advisable to immediately position this object at 0,0,0. This can be done in the inspector.

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


#### Mechanim

Mechanim provides a crucial means of mapping (humanoid) animations to models irregardless of the naming convention of the hierarchy. This allows any animation to be mapped to any model. Both an animation and a model are given an 'Avatar' by Unity. The animation and character hierarchies are then mapped to their respective avatars. Since unity avatars are constant unity can pair them to connect any animation with any model. This feature is called 'Humanoid Re-targeting'.

**To make a character model/animation available to mechanim**
1. Select the .fbx file of the character model.
2. Choose the 'Rig' tab in the inspector.
3. Change 'Animation Type' to 'Humanoid'.
4. Change 'Avatar Definition' to 'Create From This Model'.
5. Hit Apply.
6. Should be a small check mark near the 'Configure' button. Signifies that Unity was able to find all of the bones in the heirarchy it expected.
7. Hit 'Configure' if it fails.

**If animation is not working correctly**
Avatar creation requires the skeleton to be in a T-Pose. If not, the animation could look strange. There are two fixes:

1. On the 'Rig' tab, change 'Avatar Definition' to 'Create from Other Avatar'. Find the avatar of the model for which the animation was created.
2. On the 'Rig' tab, hit 'Configure'. Manually adjust the skeleton into a T-Pose.

#### Animation State Machines
To create an animation state machine (ASM) an animator controller must be created and assigned to a model's animator component.

Animations can be included in the ASM by dragging them into the animator controller window. By double clicking, the inspector for that animation can be retreived. Selecting the 'Loop Time' animation will loop the animation when played. This is adviseable when controlling via parameters. Transitions can be made by rightclicking the source node and connecting to a destination node. Deselcting 'Has Exit Time' allows for a transition to take place at any time. It does not have to wait for the animation to complete.

Layers can be used to blend animations together (i.e. punching while walking). All layers except 'Base' are assigned a weight of 0. This can be changed by clicking on the layer 'cog'. Setting the weight to 1 will dominate while less than will blend. Often it is desireable to have a layer wait for a condition before executing. This can be acheived with an empty state after the entry state.

To have a layer only affect part of the model avatar masks are needed. Usng the 'Humanoid' drop down the parts of the avatar we do not wish to be affected can be turned off. 

**Blend Tree**
Allows for a more intelligent means of blending animations (i.e. run, walk, sprint. Based on speed). Blend tree's can have multiple dimensions. For example, speed is a dimension but health can be another. By turning off 'Automate Thresholds' and using 'Compute Thresholds' the animator will look at the root motion of the animations and compute the values of the animations accordingly. Instead of the blend being from 0 to 1 the values will interpolate between the walk speed (1.56) to the run speed (5.66). Now we can plug in the nav agent desired speed directly.

#### AI State Machine
On an actor in the scene, add a capsule collider and position it to roughly fit around the model. Then add Rigidbody component with the 'Is Kinematic' option selected. This disables the physics on this entity. Go to the inspector and in the top right open the 'Layer' drop down. Add a layer called 'AI Entity'. Select the actor and change it's layer to the new 'AI Entity' layer (hit 'no' on the pop-up regarding it's children). Create another trigger called 'AI Entity Trigger'. Go to Edit > Project Settings > Physics. In the layer interaction matrix, deselect every row in the 'AI Entity' column and every row in the 'AI Entity Trigger' column except 'AI Entity'. Now the two new layers exclusivly interact with one another. 

Create an empty game object called 'Omni Zombie' and set it to the 'AI Entity' layer. Make the actor a child of this object. Make another empty game object a child of 'Omni Zombie'. Call this second child 'Target Trigger' and set it to the 'AI Entity Trigger'. Add a spherical collider component to this trigger object. This trigger, in game, will be moved to the zombie's destination. The sphere will act as a trigger, notifying the zombie it has arrived at it's target.

----------
## Notes
#### Definitions
- [Voxel](http://whatis.techtarget.com/definition/voxel): A voxel is a unit of graphic information that defines a point in three-dimensional space. A cube of space or a polygon on a 2D mesh.  
- [Off-Mesh Links](https://docs.unity3d.com/Manual/class-OffMeshLink.html): Generated shortcuts which allow the traversal over broken voxels.
- [AI.NavMeshAgent](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html) Documentation
- In Place Animation: Animations which are independent of position. Can often result in a phenomenon known as 'skating' where a character seems to float or skate. Resolved by using animations with 'root motion'.
- [Mixamo](https://www.mixamo.com/#/): Greate resource for models, animations.... etc
- **Hashes**: The unity system uses hash values to identify objects and parameters. When retreiving entities with a string it is better to precompute the hash value then search for that hash. 
- Anything Physics related should be updated in the 'FixedUpdate' method as it is called once per physics step rather than once per frame.

#### Add a trailing camera (3rd person)
 Find the head of the desired model and create an empty child of it. Call it 'Head Cam Mount'. Use 'GameObject' menu shortcut 'Align with view' to move cam mount to desired position. Add a script 'SmoothCameraMount'. Add script to main camera and make the head cam mount the mount object.Change the Update function to LateUpdate and add:

    public class SmoothCameraMount : MonoBehaviour {
    
        public Transform Mount = null;
        public float Speed = 5.0f;
    
    	// Use this for initialization
    	void Start () {
    		
    	}
    
        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, Mount.position, Time.deltaTime * Speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Mount.rotation, Time.deltaTime * Speed);
        }
    }

    