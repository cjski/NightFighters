# NightFighters
Top down arcade game where humans try and hunt down and convert the monsters attacking their homes back to humans. The last monster standing turns into a big boss and starts a final fight with the remaining humans.

## Demo Videos 
- Core Gameplay and AI (Now with sprites!): https://www.youtube.com/watch?v=CbgdxXsreBM
- Pre-Alpha AI Gameplay: https://www.youtube.com/watch?v=ciPgSKAKDvo&feature=youtu.be 

## Features
- ### AI
  - Human, monster, and boss types with different behaviours
  - Can both escape from players and chase them around walls and obstacles
  - Can make decisions on where to go, do secondary goals like turn off lights, and use abilities at the right times
- ### Custom Collision Detection
  - Prevents choppy movement, allows for pushing guys out of the way
- ### Unique Characters and abilities
  - Dashes, knockbacks, projectiles for damage and slows, throwable lights that can be picked up
  - Robust player controller code to easily apply timed slows and dashes
  - Boss monster fights in the end game
- ### Character selection screen
  - Players can join, exit, and rejoin at will
  - Any controller type can become the first player
  - Prevents 2 players from picking a human at the start (Only one is allowed)
  - AI characters fill up the remaining positions
  - The game can be played with only AI and will work just fine!
- ### Random Map Generation
  - Ensures every tile can be connected to every other tile
  - Quickly calculates distances to every other tile for fast in-game pathfinding
- ### Local Multiplayer
  - Support for 1-4 players on any combination of keyboard, controller, or mouse

## AI Information
- ### Pathfinding
  - The pathfinding is done through a combination of steering and flow fields
  - When a map is generated, a navigation map is also generated, that uses A* to find the distances from every tile to every other tile, in essence generating a flow field so at any tile an AI knows where to go to get somewhere else
  - AI have a series of vectors applied to them that influence their movement (Push vectors, goal direction, movement away from walls/other players)
  - These get summed to find the final heading an AI should move in
  - If the AI can't see their goal, they can use the navigation data to find the tile they should path to that will let them get there the fastest
  - They path towards that tile and continue moving until they can see their goal and get there in a straight line
  
## Tools
Unity is the game engine being using to develop the code, which is all written in C#. The artwork is being done with Piskel.
