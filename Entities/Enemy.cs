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
        private float _shootTimer = 0f;
        private float _shootDelay = 2.0f; // Стріляє раз на 2 секунди
        private float _moveSpeed = 0.015f;

        // Список іскор (куль), які випустив саме цей ворог
        public List<Projectile> Bullets = new List<Projectile>();

        public Enemy(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void Update(GameTime gameTime, Player player, Map map)
        {
            float dist = Vector2.Distance(Position, player.Position);

            // --- 1. РУХ З ПЕРЕВІРКОЮ СТІН ---
            // Ворог іде, якщо бачить гравця (dist < 8) і не стоїть впритул (dist > 1.4)
            if (dist < 8f && dist > 1.4f)
            {
                Vector2 dir = player.Position - Position;
                dir.Normalize();

                // Колізія: додаємо відступ (padding), щоб ворог не входив у стіну плечем
                float padding = 0.3f; 
                Vector2 nextPos = Position + dir * _moveSpeed;

                // Перевірка по горизонталі (X)
                // Визначаємо, яку сторону ворога перевіряти залежно від напрямку руху
                float checkX = (dir.X > 0) ? nextPos.X + padding : nextPos.X - padding;
                if (map.Get((int)checkX, (int)Position.Y) == 0)
                {
                    Position.X = nextPos.X;
                }

                // Перевірка по вертикалі (Y)
                float checkY = (dir.Y > 0) ? nextPos.Y + padding : nextPos.Y - padding;
                if (map.Get((int)Position.X, (int)checkY) == 0)
                {
                    Position.Y = nextPos.Y;
                }
            }

            // --- 2. ЛОГІКА СТРІЛЬБИ ---
            _shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_shootTimer >= _shootDelay && dist < 10f)
            {
                _shootTimer = 0f;
                // Створюємо іскру, що летить від ворога до гравця
                Vector2 shootDir = player.Position - Position;
                Bullets.Add(new Projectile(Position, shootDir));
            }

            // Оновлюємо іскри цього ворога
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(map);
                // Якщо іскра влучила в стіну, вона стає IsActive = false і ми її видаляємо
                if (!Bullets[i].IsActive) 
                {
                    Bullets.RemoveAt(i);
                }
            }
        }
    }
}