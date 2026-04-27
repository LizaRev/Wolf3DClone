using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Wolf3DClone.Core;

namespace Wolf3DClone.Render
{
    public class Renderer
    {
        private GraphicsDevice _gd;
        private SpriteBatch _sb;
        private Texture2D _pixel;

        public Renderer(GraphicsDevice gd)
        {
            _gd = gd;
            _sb = new SpriteBatch(gd);

            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(Player player, Map map, Raycaster ray,
            Texture2D wall, Texture2D door, Texture2D finish, Texture2D floor)
        {
            _gd.Clear(Color.Black);
            _sb.Begin(samplerState: SamplerState.PointClamp);

            int sw = _gd.Viewport.Width;
            int sh = _gd.Viewport.Height;

            // 🌌 ceiling
            _sb.Draw(_pixel, new Rectangle(0, 0, sw, sh / 2), new Color(20, 20, 20));

            // =========================
            // 🌍 FLOOR RENDER
            // =========================
            for (int y = sh / 2; y < sh; y++)
            {
                float p = y - sh / 2f;
                float posZ = 0.5f * sh;
                float rowDistance = posZ / p;

                float stepX = rowDistance *
                    (float)(Math.Cos(player.Rotation + ray.FOV / 2) -
                            Math.Cos(player.Rotation - ray.FOV / 2)) / sw;

                float stepY = rowDistance *
                    (float)(Math.Sin(player.Rotation + ray.FOV / 2) -
                            Math.Sin(player.Rotation - ray.FOV / 2)) / sw;

                float floorX = player.Position.X +
                    rowDistance * (float)Math.Cos(player.Rotation - ray.FOV / 2);

                float floorY = player.Position.Y +
                    rowDistance * (float)Math.Sin(player.Rotation - ray.FOV / 2);

                for (int x = 0; x < sw; x++)
                {
                    float fx = floorX - (float)Math.Floor(floorX);
                    float fy = floorY - (float)Math.Floor(floorY);

                    int tx = (int)(floor.Width * fx) & (floor.Width - 1);
                    int ty = (int)(floor.Height * fy) & (floor.Height - 1);

                    floorX += stepX;
                    floorY += stepY;

                    float brightness = MathHelper.Clamp(1f - (rowDistance / 10f), 0.2f, 1f);

                    _sb.Draw(
                        floor,
                        new Rectangle(x, y, 1, 1),
                        new Rectangle(tx, ty, 1, 1),
                        new Color(Vector3.One * brightness)
                    );
                }
            }

            // =========================
            // 🧱 WALLS + 🚪 DOORS
            // =========================
            for (int x = 0; x < sw; x++)
            {
                float angle = player.Rotation - ray.FOV / 2 + ray.FOV * x / sw;
                var hit = ray.Cast(player.Position, angle, map);

                float dist = hit.Distance * (float)Math.Cos(angle - player.Rotation);
                int wallHeight = (int)(sh / (dist + 0.0001f));

                Texture2D tex = wall;

                if (hit.WallType == 2)
                    tex = door;
                else if (hit.WallType == 4)
                    tex = finish;

                // 🚪 door animation (slide effect)
                float doorOffset = 0f;

                if (hit.WallType == 2)
                {
                    int dx = (int)hit.WorldX;
                    int dy = (int)hit.WorldY;

                    doorOffset = map.DoorOpen[dy, dx] * tex.Width;
                }

                bool vertical =
                    Math.Abs(hit.WorldY - (int)hit.WorldY - 0.5f) >
                    Math.Abs(hit.WorldX - (int)hit.WorldX - 0.5f);

                float texCoord =
                    vertical
                        ? (hit.WorldX - (float)Math.Floor(hit.WorldX))
                        : (hit.WorldY - (float)Math.Floor(hit.WorldY));

                int sourceX = (int)(texCoord * tex.Width + doorOffset);
                sourceX = MathHelper.Clamp(sourceX, 0, tex.Width - 1);

                float brightness = MathHelper.Clamp(1f - (dist / 12f), 0.3f, 1f);

                _sb.Draw(
                    tex,
                    new Rectangle(x, (sh - wallHeight) / 2, 1, wallHeight),
                    new Rectangle(sourceX, 0, 1, tex.Height),
                    new Color(Vector3.One * brightness)
                );
            }

            _sb.End();
        }
    }
}