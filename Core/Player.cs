using Microsoft.Xna.Framework;
using System;
using Wolf3DClone.World;

namespace Wolf3DClone.Core
{
    public class Player
    {
        public Vector2 Position;
        public float Rotation;
        public float Health = 100f; // Твоє здоров'я

        public Player()
        {
            Position = new Vector2(1.5f, 1.5f);
            Rotation = 0f;
        }

        public void Update(Map map, bool forward, bool back, float speed)
        {
            float dx = (float)Math.Cos(Rotation) * speed;
            float dy = (float)Math.Sin(Rotation) * speed;

            Vector2 next = Position;
            if (forward) next += new Vector2(dx, dy);
            if (back) next -= new Vector2(dx, dy);

            // Можна ходити крізь порожнечу (0) та двері (2)
            int cellType = map.Get((int)next.X, (int)next.Y);
            if (cellType == 0 || cellType == 2)
            {
                Position = next;
            }
        }

        public Projectile Shoot()
        {
            Vector2 dir = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            return new Projectile(Position, dir);
        }
    }
}