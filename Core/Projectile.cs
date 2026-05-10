using Microsoft.Xna.Framework;
using Wolf3DClone.Core;

namespace Wolf3DClone.World
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed = 0.12f;
        public bool IsActive = true;

        public Projectile(Vector2 pos, Vector2 dir)
        {
            Position = pos;
            Direction = dir;
            if (Direction != Vector2.Zero) Direction.Normalize();
        }

        public void Update(Map map)
        {
            Position += Direction * Speed;

            if (map.Get((int)Position.X, (int)Position.Y) != 0)
            {
                IsActive = false;
            }
        }
    }
}