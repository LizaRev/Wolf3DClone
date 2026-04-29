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
        private Texture2D _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex;
        private SoundEffect _stepSound, _doorSound;
        private SoundEffectInstance _stepInstance, _doorInstance;
        private float _stepTimer;
        private KeyboardState _oldState;
        private List<Enemy> _enemies;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            _player = new Player();
            _map = new Map();
            _raycaster = new Raycaster();
            _enemies = new List<Enemy> { new Enemy(5.5f, 5.5f) };
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
            CleanTransparency(_enemyTex);

            _stepSound = SoundEffect.FromStream(File.OpenRead("Content/step.wav"));
            _stepInstance = _stepSound.CreateInstance();
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
            _player.Update(_map, k.IsKeyDown(Keys.W), k.IsKeyDown(Keys.S), 0.05f);
            
            foreach (var e in _enemies) 
            {
                // ПЕРЕДАЕМ КАРТУ ДЛЯ ПРОВЕРКИ КОЛЛИЗИЙ
                e.Update(gameTime, _player, _map);
            }

            if (k.IsKeyDown(Keys.A)) _player.Rotation -= 0.04f;
            if (k.IsKeyDown(Keys.D)) _player.Rotation += 0.04f;
            if (k.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space))
            {
                _map.ToggleDoor(_player.Position.X, _player.Position.Y, _player.Rotation);
                _doorInstance.Play();
            }
            _oldState = k;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // ПЕРЕДАЕМ gameTime В РЕНДЕР
            _renderer.Draw(_player, _map, _raycaster, _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _enemies, gameTime);
            base.Draw(gameTime);
        }
    }
}