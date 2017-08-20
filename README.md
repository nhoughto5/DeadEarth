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

