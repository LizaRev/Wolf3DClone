using Microsoft.Xna.Framework;
using System;
using Wolf3DClone.Core;

namespace Wolf3DClone.World
{
    public class Enemy
    {
        public Vector2 Position;
        public float Scale = 0.6f; 
        private float _shootTimer = 0f;
        private float _shootDelay = 1.5f;
        private float _moveSpeed = 0.015f;

        public Enemy(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void Update(GameTime gameTime, Player player, Map map)
        {
            float dist = Vector2.Distance(Position, player.Position);

            // Слідування за гравцем
            if (dist < 8f && dist > 1.2f)
            {
                Vector2 dir = player.Position - Position;
                dir.Normalize();

                // Перевірка по X (використовуємо твій метод map.Get)
                float nextX = Position.X + dir.X * _moveSpeed;
                if (map.Get((int)nextX, (int)Position.Y) == 0)
                {
                    Position.X = nextX;
                }

                // Перевірка по Y
                float nextY = Position.Y + dir.Y * _moveSpeed;
                if (map.Get((int)Position.X, (int)nextY) == 0)
                {
                    Position.Y = nextY;
                }
            }

            // Таймер стрільби
            _shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_shootTimer >= _shootDelay)
            {
                _shootTimer = 0f;
                Console.WriteLine("ENEMY SHOOTS!");
                if (dist < 1.5f) Console.WriteLine("PLAYER HIT!");
            }
        }
    }
}