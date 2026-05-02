using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using Wolf3DClone.Core;
using Wolf3DClone.Render;
using Wolf3DClone.World;

namespace Wolf3DClone
{
    public enum GameState { Menu, Playing }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Renderer _renderer;
        private Player _player;
        private Map _map;
        private Raycaster _raycaster;

        // Текстури
        private Texture2D _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _boltTex;
        private Texture2D _menuBgTex, _startBtnTex, _exitBtnTex, _titleTex;
        
        // Звукові ефекти
        private SoundEffect _stepSound, _doorSound;
        private SoundEffectInstance _stepInstance, _doorInstance;

        private KeyboardState _oldState;
        private MouseState _oldMouse;
        private List<Enemy> _enemies;
        private List<Projectile> _playerBullets = new List<Projectile>();

        private GameState _currentState = GameState.Menu;
        private int _selected = 0;

        // Розміри кнопок (твої широкі кнопки по центру)
        private Rectangle _titleRect = new Rectangle(200, 40, 400, 120);
        private Rectangle _startRect = new Rectangle(200, 260, 400, 110);
        private Rectangle _exitRect = new Rectangle(200, 400, 400, 110);

        private float _lastHitTimer = 0f;
        private const float RegenDelay = 3.0f;
        private const float RegenRate = 5.0f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _player = new Player();
            _map = new Map();
            _raycaster = new Raycaster();
            _enemies = new List<Enemy> { 
                new Enemy(4.5f, 1.5f), new Enemy(1.5f, 7.5f), 
                new Enemy(13.5f, 1.5f), new Enemy(13.5f, 13.5f) 
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _renderer = new Renderer(GraphicsDevice);
            
            // Завантаження графіки
            _wallTex = LoadTexture("Content/wall.png");
            _doorTex = LoadTexture("Content/door.png");
            _finishTex = LoadTexture("Content/finish.png");
            _floorTex = LoadTexture("Content/floor.png");
            _enemyTex = LoadTexture("Content/enemy.png");
            _boltTex = LoadTexture("Content/bolt.png");
            _menuBgTex = LoadTexture("Content/menu_bg.png");
            _startBtnTex = LoadTexture("Content/start_btn.png");
            _exitBtnTex = LoadTexture("Content/exit_btn.png");
            _titleTex = LoadTexture("Content/title.png");

            CleanTransparency(_enemyTex);
            CleanTransparency(_boltTex);

            // Завантаження звуків кроків та дверей (wav зазвичай не викликає SIGABRT)
            try {
                _stepSound = LoadSound("Content/step.wav");
                if (_stepSound != null) {
                    _stepInstance = _stepSound.CreateInstance();
                    _stepInstance.IsLooped = true;
                }
                
                _doorSound = LoadSound("Content/door.wav");
                if (_doorSound != null) _doorInstance = _doorSound.CreateInstance();
            } catch {
                // Якщо навіть wav не вантажиться, гра піде без звуку, але не вилетить
            }
        }

        private Texture2D LoadTexture(string path)
        {
            if (File.Exists(path)) return Texture2D.FromStream(GraphicsDevice, File.OpenRead(path));
            Texture2D t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new[] { Color.HotPink });
            return t;
        }

        private SoundEffect LoadSound(string path)
        {
            if (File.Exists(path)) return SoundEffect.FromStream(File.OpenRead(path));
            return null;
        }

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
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentState == GameState.Menu)
            {
                IsMouseVisible = true;
                if (k.IsKeyDown(Keys.Up) && _oldState.IsKeyUp(Keys.Up)) _selected = 0;
                if (k.IsKeyDown(Keys.Down) && _oldState.IsKeyUp(Keys.Down)) _selected = 1;

                bool clicked = (m.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released);
                if ((clicked && _startRect.Contains(m.Position)) || (k.IsKeyDown(Keys.Enter) && _selected == 0))
                {
                    _currentState = GameState.Playing;
                }
                if ((clicked && _exitRect.Contains(m.Position)) || (k.IsKeyDown(Keys.Enter) && _selected == 1))
                    Exit();
            }
            else
            {
                IsMouseVisible = false;
                if (k.IsKeyDown(Keys.Escape)) _currentState = GameState.Menu;

                _player.Update(_map, k.IsKeyDown(Keys.W), k.IsKeyDown(Keys.S), 0.05f);
                
                if (k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S)) {
                    if (_stepInstance?.State != SoundState.Playing) _stepInstance?.Play();
                } else _stepInstance?.Stop();

                if ((m.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released) ||
                    (k.IsKeyDown(Keys.LeftControl) && !_oldState.IsKeyDown(Keys.LeftControl)))
                    _playerBullets.Add(_player.Shoot());

                foreach (var e in _enemies)
                {
                    e.Update(gameTime, _player, _map);
                    for (int i = e.Bullets.Count - 1; i >= 0; i--)
                    {
                        if (Vector2.Distance(e.Bullets[i].Position, _player.Position) < 0.4f)
                        {
                            _player.Health -= 10f;
                            _lastHitTimer = 0f;
                            e.Bullets.RemoveAt(i);
                            if (_player.Health <= 0) { _player.Health = 100f; _player.Position = new Vector2(1.5f, 1.5f); }
                        }
                    }
                }

                _lastHitTimer += dt;
                if (_lastHitTimer >= RegenDelay && _player.Health < 100f)
                {
                    _player.Health += RegenRate * dt;
                    if (_player.Health > 100f) _player.Health = 100f;
                }

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
                    _doorInstance?.Play();
                }
            }

            _oldState = k; _oldMouse = m;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_currentState == GameState.Menu)
                _renderer.DrawMenu(_menuBgTex, _titleTex, _titleRect, _startBtnTex, _startRect, _exitBtnTex, _exitRect, _selected);
            else
                _renderer.Draw(_player, _map, _raycaster, _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _boltTex, _enemies, _playerBullets, gameTime);
            base.Draw(gameTime);
        }
    }
}