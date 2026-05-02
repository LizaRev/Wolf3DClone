using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Wolf3DClone.Core;

namespace Wolf3DClone.World
{
    public class Enemy
    {
        public Vector2 Position;
        public float Scale = 0.6f; 
        public float Health = 100f; 

        private float _shootTimer = 0f;
        // ЗМІНЕНО: тепер затримка 0.8 сек замість 2.0
        private float _shootDelay = 0.8f; 
        // ЗМІНЕНО: швидкість 0.02f замість 0.015f (став трохи швидшим)
        private float _moveSpeed = 0.02f; 

        public List<Projectile> Bullets = new List<Projectile>();

        public Enemy(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void Update(GameTime gameTime, Player player, Map map)
        {
            float dist = Vector2.Distance(Position, player.Position);

            // Рух до гравця
            if (dist < 10f && dist > 1.2f)
            {
                Vector2 dir = Vector2.Normalize(player.Position - Position);
                
                // Колізія по X
                Vector2 nextX = Position + new Vector2(dir.X * _moveSpeed, 0);
                if (map.Get((int)(nextX.X + (dir.X > 0 ? 0.3f : -0.3f)), (int)Position.Y) <= 2) 
                    Position.X = nextX.X;

                // Колізія по Y
                Vector2 nextY = Position + new Vector2(0, dir.Y * _moveSpeed);
                if (map.Get((int)Position.X, (int)(nextY.Y + (dir.Y > 0 ? 0.3f : -0.3f))) <= 2) 
                    Position.Y = nextY.Y;
            }

            // ШВИДКА СТРІЛЬБА
            _shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_shootTimer >= _shootDelay && dist < 12f)
            {
                _shootTimer = 0f;
                // Стріляє точно в гравця
                Vector2 shootDir = player.Position - Position;
                Bullets.Add(new Projectile(Position, shootDir));
            }

            // Оновлення куль
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(map);
                if (!Bullets[i].IsActive) Bullets.RemoveAt(i);
            }
        }
    }
}