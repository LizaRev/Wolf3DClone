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
        public float Health = 100f; // Здоров'я ворога

        private float _shootTimer = 0f;
        private float _shootDelay = 2.0f;
        private float _moveSpeed = 0.015f;

        public List<Projectile> Bullets = new List<Projectile>();

        public Enemy(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void Update(GameTime gameTime, Player player, Map map)
        {
            float dist = Vector2.Distance(Position, player.Position);

            if (dist < 8f && dist > 1.4f)
            {
                Vector2 dir = player.Position - Position;
                dir.Normalize();

                float padding = 0.3f; 
                Vector2 nextPos = Position + dir * _moveSpeed;

                float checkX = (dir.X > 0) ? nextPos.X + padding : nextPos.X - padding;
                int tileX = map.Get((int)checkX, (int)Position.Y);
                if (tileX == 0 || tileX == 2) Position.X = nextPos.X;

                float checkY = (dir.Y > 0) ? nextPos.Y + padding : nextPos.Y - padding;
                int tileY = map.Get((int)Position.X, (int)checkY);
                if (tileY == 0 || tileY == 2) Position.Y = nextPos.Y;
            }

            _shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_shootTimer >= _shootDelay && dist < 10f)
            {
                _shootTimer = 0f;
                Vector2 shootDir = player.Position - Position;
                Bullets.Add(new Projectile(Position, shootDir));
            }

            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(map);
                if (!Bullets[i].IsActive) Bullets.RemoveAt(i);
            }
        }
    }
}