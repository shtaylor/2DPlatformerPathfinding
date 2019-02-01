# 2DPlatformerPathfinding
An attempt at generalized pathfinding for a 2D platformer style game.

See it in action: https://youtu.be/6pNPyXKm-Y4

I noticed I couldn't find any 2D platformer games with pathfinding implemented that were not grid-based or allowed for complex movements. I coded this in C# for Unity as a test to see if it was viable. Works pretty well, but there are still some issues that arise from time to time, and performance isn't great yet. This plugs into the awesome Ferr2D tool on the Unity Asset Store so that it can generate all the floor nodes automatically for you. I created prefabs for the various types of jumps as well, and so all that is needed to build the pathfinding is to place the jump prefabs around the level. The scripts connect everything up for you so you don't have to manually setup everything.
It works by transforming the level data into a graph, then performing Dijkstra's algorithm to determine the shortest path. After it has determined the path, the game determines the necessary messages to send the AI-controlled game characters. The game characters take the messages and turn that into the corresponding input to perform the actions.
