🎮 Wolf3DClone

Wolf3DClone is a retro-style first-person shooter game developed in C# using the MonoGame Framework. The project was inspired by classic games like Wolfenstein 3D and recreates the old-school 3D atmosphere using the Raycasting technique.
In the game, the player explores a maze, opens doors, fights enemies, and tries to reach the finish area. Even though the world is technically 2D, Raycasting creates a realistic pseudo-3D effect by casting rays from the player’s position and calculating wall distances.

🛠 Technologies Used
1)C#
2)MonoGame
3)Microsoft.Xna.Framework
4)Raycasting
4)Texture2D & SpriteBatch
5)SoundEffect

📦 Main Classes

1)🗺 Map
Stores the level layout, walls, doors, and finish area.
Also handles door opening and collision checking.

2)👤 Player
Controls player movement, rotation, shooting, and health.

3)🔫 Projectile
Represents bullets and handles movement and collisions.

4)👾 Enemy
Implements simple enemy AI:
follows the player;
shoots projectiles;
takes damage and can die.

5)🎨 Renderer
Responsible for drawing:
walls;
floor;
enemies;
bullets;
menus;
health bar.

6)Raycaster
The core graphics system that creates the 3D visual effect

7)🎮 Game1
The main game controller that manages:
game states;
input;
textures;
sounds;
updates and rendering.

The game uses simple and intuitive controls. The W key moves the player forward, while S moves backward. The A and D keys rotate the camera left and right. The Space key is used to open and close doors. The player can shoot by pressing Left Ctrl or using the left mouse button. Pressing Escape returns the player to the main menu, and Enter is used to select menu options.
