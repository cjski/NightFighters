# NightFighters
Top down arcade game where humans try and hunt down and convert the monsters attacking their homes back to humans. The last monster standing turns into a big boss and starts a final fight with the remaining humans.

## Demo Videos 
- Pre-Alpha AI Gameplay: https://www.youtube.com/watch?v=ciPgSKAKDvo&feature=youtu.be 

## Features
- ### AI
  - Human and monster types
  - Can both escape from players and chase them around walls and obstacles
- ### Custom Collision Detection
  - Prevents choppy movement
- ### Unique Characters and abilities
  - Dashes, knockbacks, projectiles for damage and slows, throwable lights that can be picked up
  - Robust player controller code to easily apply timed slows and dashes
- ### Robust character selection screen
  - Players can join, exit, and rejoin at will
  - Any controller type can become the first player
  - Prevents 2 players from picking a human at the start (Only one is allowed)
  - AI characters fill up the remaining positions
- ### Random Map Generation
  - Ensures every tile can be connected to every other tile
  - Quickly calculates distances to every other tile for fast in-game pathfinding
  
## Planned Features
- AI able to complete secondary objectives (Turning off lights, using abilities)
- Boss Monster fights
- Gamepad/Controller support

## Tools
Unity is the game engine being using to develop the code, which is all written in C#. The artwork is being done with Piskel.
