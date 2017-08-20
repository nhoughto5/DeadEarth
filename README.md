# **Dead Earth**
##### A Udemy Course Diary

This repository contains the code base develped while following the lessons which can be found [here](https://www.udemy.com/build-your-own-first-person-shooter-survival-game-in-unity). This course provided an excellent look into technologies and strategies used to build modern video games. Dead Earth is a first person shooter built with [Unity 5](https://unity3d.com/) and C#.

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

##### Notes and Definitions 
- [Voxel](http://whatis.techtarget.com/definition/voxel): A voxel is a unit of graphic information that defines a point in three-dimensional space. A cube of space or a polygon on a 2D mesh.  
- [Off-Mesh Links](https://docs.unity3d.com/Manual/class-OffMeshLink.html): Generated shortcuts which allow the traversal over broken voxels.
