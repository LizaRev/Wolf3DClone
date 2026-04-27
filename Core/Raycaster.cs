using Microsoft.Xna.Framework;
using System;

namespace Wolf3DClone.Core
{
    public class Raycaster
    {
        public float FOV = MathHelper.ToRadians(60);

        public (float Distance, int WallType, float WorldX, float WorldY)
        Cast(Vector2 pos, float angle, Map map)
        {
            float dist = 0f;

            while (dist < 20f)
            {
                float x = pos.X + (float)Math.Cos(angle) * dist;
                float y = pos.Y + (float)Math.Sin(angle) * dist;

                int cell = map.Get((int)x, (int)y);

                // 🚪 двері з колізією
                if (cell == 2)
                {
                    float open = map.DoorOpen[(int)y, (int)x];

                    if (open < 0.7f)
                        return (dist, cell, x, y);
                }
                else if (cell != 0)
                {
                    return (dist, cell, x, y);
                }

                dist += 0.01f;
            }

            return (20f, 1, 0, 0);
        }
    }
}