using Microsoft.Xna.Framework;
using System;

namespace Wolf3DClone.Core
{
    public class Player
    {
        public Vector2 Position;
        public float Rotation;

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

            int x = (int)next.X;
            int y = (int)next.Y;

            int cell = map.Get(x, y);

            if (cell == 1)
                return;

            if (cell == 2)
            {
                float open = map.DoorOpen[y, x];

                if (open < 0.6f)
                    return;
            }

            Position = next;
        }
    }
}