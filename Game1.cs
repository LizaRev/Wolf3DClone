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

        // Текстури
        private Texture2D _wallTex, _doorTex, _finishTex, _floorTex, _enemyTex, _boltTex;

        // Звуки
        private SoundEffect _stepSound, _doorSound;
        private SoundEffectInstance _stepInstance, _doorInstance;

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
            _player = new Player(); // Початкова позиція гравця (1.5, 1.5)
            _map = new Map();
            _raycaster = new Raycaster();

            // ТУТ 5 ВОРОГІВ: ОДИН ПРЯМО НА ПОЧАТКУ + 4 ПО КАРТІ
            _enemies = new List<Enemy> 
            { 
                new Enemy(2.5f, 1.5f),   // ЗАСІДКА ПРЯМО ПЕРЕД ГРАВЦЕМ
                new Enemy(1.5f, 7.5f),   // Коридор зліва
                new Enemy(7.5f, 3.5f),   // Лабіринт зверху
                new Enemy(13.5f, 9.5f),  // Довгий коридор справа
                new Enemy(8.5f, 13.5f)   // Охорона входу в червону кімнату
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
            _boltTex = LoadTexture("Content/bolt.png"); // Текстура іскри

            // Очищення фону спрайтів
            CleanTransparency(_enemyTex);
            CleanTransparency(_boltTex);

            // Завантаження звуку
            _stepSound = SoundEffect.FromStream(File.OpenRead("Content/step.wav"));
            _stepInstance = _stepSound.CreateInstance();
            _stepInstance.IsLooped = true; // Робимо кроки циклічними

            _doorSound = SoundEffect.FromStream(File.OpenRead("Content/door.wav"));
            _doorInstance = _doorSound.CreateInstance();
        }

        private Texture2D LoadTexture(string path) => Texture2D.FromStream(GraphicsDevice, File.OpenRead(path));

        private void CleanTransparency(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
                // Прибираємо білий або прозорий фон
                if (data[i].A < 255 || (data[i].R > 240 && data[i].G > 240 && data[i].B > 240))
                    data[i] = Color.Transparent;
            texture.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            var k = Keyboard.GetState();

            // Перевірка, чи гравець натискає кнопки ходьби
            bool isMoving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S);

            // Рух гравця
            _player.Update(_map, k.IsKeyDown(Keys.W), k.IsKeyDown(Keys.S), 0.05f);

            // КЕРУВАННЯ ЗВУКОМ КРОКІВ
            if (isMoving)
            {
                if (_stepInstance.State != SoundState.Playing)
                    _stepInstance.Play();
            }
            else
            {
                if (_stepInstance.State == SoundState.Playing)
                    _stepInstance.Stop();
            }

            // Оновлення ворогів (їхній рух та стрільба)
            foreach (var e in _enemies) 
            {
                e.Update(gameTime, _player, _map);
            }

            // Повороти голови
            if (k.IsKeyDown(Keys.A)) _player.Rotation -= 0.04f;
            if (k.IsKeyDown(Keys.D)) _player.Rotation += 0.04f;

            // Відкриття дверей на Space
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
            // Рендер всього світу
            _renderer.Draw(_player, _map, _raycaster, 
                _wallTex, _doorTex, _finishTex, _floorTex, 
                _enemyTex, _boltTex, _enemies, gameTime);

            base.Draw(gameTime);
        }
    }
}