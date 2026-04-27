using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Wolf3DClone.Core;
using Wolf3DClone.Render;

namespace Wolf3DClone
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Renderer _renderer;
        private Player _player;
        private Map _map;
        private Raycaster _raycaster;

        private Texture2D _wallTex, _doorTex, _finishTex, _floorTex;

        private SoundEffect _stepSound;
        private SoundEffectInstance _stepInstance;
        private float _stepTimer;

        private SoundEffect _doorSound;
        private SoundEffectInstance _doorInstance;

        private KeyboardState _oldState;

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _renderer = new Renderer(GraphicsDevice);

            _wallTex = Texture2D.FromStream(GraphicsDevice, System.IO.File.OpenRead("Content/wall.png"));
            _doorTex = Texture2D.FromStream(GraphicsDevice, System.IO.File.OpenRead("Content/door.png"));
            _finishTex = Texture2D.FromStream(GraphicsDevice, System.IO.File.OpenRead("Content/finish.png"));
            _floorTex = Texture2D.FromStream(GraphicsDevice, System.IO.File.OpenRead("Content/floor.png"));

            using (var s = System.IO.File.OpenRead("Content/step.wav"))
            {
                _stepSound = SoundEffect.FromStream(s);
                _stepInstance = _stepSound.CreateInstance();
            }

            using (var s = System.IO.File.OpenRead("Content/door.wav"))
            {
                _doorSound = SoundEffect.FromStream(s);
                _doorInstance = _doorSound.CreateInstance();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var k = Keyboard.GetState();

            bool moving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.S);

            _player.Update(_map, k.IsKeyDown(Keys.W), k.IsKeyDown(Keys.S), 0.05f);

            // 🚶 кроки
            if (moving)
            {
                _stepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_stepTimer > 0.45f)
                {
                    if (_stepInstance.State != SoundState.Playing)
                        _stepInstance.Play();

                    _stepTimer = 0;
                }
            }
            else
            {
                _stepTimer = 0;

                if (_stepInstance.State == SoundState.Playing)
                    _stepInstance.Stop();
            }

            // 🔄 поворот
            if (k.IsKeyDown(Keys.A)) _player.Rotation -= 0.04f;
            if (k.IsKeyDown(Keys.D)) _player.Rotation += 0.04f;

            // 🚪 двері
            if (k.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space))
            {
                _map.ToggleDoor(_player.Position.X, _player.Position.Y, _player.Rotation);

                if (_doorInstance.State == SoundState.Playing)
                    _doorInstance.Stop();

                _doorInstance.Play();
            }

            _oldState = k;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderer.Draw(_player, _map, _raycaster, _wallTex, _doorTex, _finishTex, _floorTex);
            base.Draw(gameTime);
        }
    }
}