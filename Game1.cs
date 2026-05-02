using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using Wolf3DClone.Core;
using Wolf3DClone.Render;
using Wolf3DClone.World;

namespace Wolf3DClone
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Renderer _renderer;
        private Player _player;
        private Map _map;
        private Raycaster _raycaster;

        private Texture2D _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _boltTex;
        private SoundEffect _stepSound, _doorSound;
        private SoundEffectInstance _stepInstance, _doorInstance;

        private KeyboardState _oldState;
        private MouseState _oldMouse;
        private List<Enemy> _enemies;
        private List<Projectile> _playerBullets = new List<Projectile>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            _player = new Player();
            _map = new Map();
            _raycaster = new Raycaster();
            _enemies = new List<Enemy> 
            { 
                new Enemy(2.5f, 1.5f), new Enemy(1.5f, 7.5f), 
                new Enemy(7.5f, 3.5f), new Enemy(13.5f, 9.5f), new Enemy(8.5f, 13.5f) 
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _renderer = new Renderer(GraphicsDevice);
            _wallTex = LoadTexture("Content/wall.png");
            _doorTex = LoadTexture("Content/door.png");
            _finishTex = LoadTexture("Content/finish.png");
            _floorTex = LoadTexture("Content/floor.png");
            _enemyTex = LoadTexture("Content/enemy.png");
            _boltTex = LoadTexture("Content/bolt.png");

            CleanTransparency(_enemyTex);
            CleanTransparency(_boltTex);

            _stepSound = SoundEffect.FromStream(File.OpenRead("Content/step.wav"));
            _stepInstance = _stepSound.CreateInstance();
            _stepInstance.IsLooped = true;
            _doorSound = SoundEffect.FromStream(File.OpenRead("Content/door.wav"));
            _doorInstance = _doorSound.CreateInstance();
        }

        private Texture2D LoadTexture(string path) => Texture2D.FromStream(GraphicsDevice, File.OpenRead(path));

        private void CleanTransparency(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
                if (data[i].A < 255 || (data[i].R > 240 && data[i].G > 240 && data[i].B > 240))
                    data[i] = Color.Transparent;
            texture.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            var k = Keyboard.GetState();
            var m = Mouse.GetState();

            _player.Update(_map, k.IsKeyDown(Keys.W), k.IsKeyDown(Keys.S), 0.05f);
            
            bool isMoving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S);
            if (isMoving) { if (_stepInstance.State != SoundState.Playing) _stepInstance.Play(); }
            else _stepInstance.Stop();

            // Стрільба гравця
            if ((m.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released) ||
                (k.IsKeyDown(Keys.LeftControl) && !_oldState.IsKeyDown(Keys.LeftControl)))
            {
                _playerBullets.Add(_player.Shoot());
            }

            // Оновлення ворогів та їхніх куль
            foreach (var e in _enemies)
            {
                e.Update(gameTime, _player, _map);
                
                // Перевірка: чи влучив ворог у гравця
                for (int i = e.Bullets.Count - 1; i >= 0; i--)
                {
                    if (Vector2.Distance(e.Bullets[i].Position, _player.Position) < 0.4f)
                    {
                        _player.Health -= 10f; // Мінус 10 HP
                        e.Bullets.RemoveAt(i);
                        
                        if (_player.Health <= 0) // Смерть гравця
                        {
                            _player.Health = 100f;
                            _player.Position = new Vector2(1.5f, 1.5f);
                        }
                    }
                }
            }

            // Оновлення твоїх куль та влучань у ворогів
            for (int i = _playerBullets.Count - 1; i >= 0; i--)
            {
                _playerBullets[i].Update(_map);
                for (int j = _enemies.Count - 1; j >= 0; j--)
                {
                    if (Vector2.Distance(_playerBullets[i].Position, _enemies[j].Position) < 0.5f)
                    {
                        _enemies[j].Health -= 10f;
                        _playerBullets[i].IsActive = false;
                        if (_enemies[j].Health <= 0) _enemies.RemoveAt(j);
                        break;
                    }
                }
                if (!_playerBullets[i].IsActive) _playerBullets.RemoveAt(i);
            }

            if (k.IsKeyDown(Keys.A)) _player.Rotation -= 0.04f;
            if (k.IsKeyDown(Keys.D)) _player.Rotation += 0.04f;
            
            if (k.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space))
            {
                _map.ToggleDoor(_player.Position.X, _player.Position.Y, _player.Rotation);
                _doorInstance.Play();
            }

            _oldState = k;
            _oldMouse = m;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderer.Draw(_player, _map, _raycaster, _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _boltTex, _enemies, _playerBullets, gameTime);
            base.Draw(gameTime);
        }
    }
}