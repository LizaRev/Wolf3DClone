using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Wolf3DClone.Core;
using Wolf3DClone.World;

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
            Texture2D wall, Texture2D door, Texture2D finish, Texture2D floor,
            Texture2D enemyTex, Texture2D boltTex, List<Enemy> enemies, GameTime gameTime)
        {
            int sw = _gd.Viewport.Width;
            int sh = _gd.Viewport.Height;
            float[] zBuffer = new float[sw];

            _gd.Clear(Color.Black);
            _sb.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            // 🌌 СТЕЛЯ (Бежева)
            _sb.Draw(_pixel, new Rectangle(0, 0, sw, sh / 2), new Color(230, 220, 200));

            // 🌍 ПІДЛОГА (Текстурована)
            for (int y = sh / 2; y < sh; y++)
            {
                float p = y - sh / 2f;
                float posZ = 0.5f * sh;
                float rowDistance = posZ / (p + 0.0001f);

                float stepX = rowDistance * (float)(Math.Cos(player.Rotation + ray.FOV / 2) - Math.Cos(player.Rotation - ray.FOV / 2)) / sw;
                float stepY = rowDistance * (float)(Math.Sin(player.Rotation + ray.FOV / 2) - Math.Sin(player.Rotation - ray.FOV / 2)) / sw;

                float floorX = player.Position.X + rowDistance * (float)Math.Cos(player.Rotation - ray.FOV / 2);
                float floorY = player.Position.Y + rowDistance * (float)Math.Sin(player.Rotation - ray.FOV / 2);

                for (int x = 0; x < sw; x++)
                {
                    int tx = (int)(floor.Width * (floorX - (float)Math.Floor(floorX))) & (floor.Width - 1);
                    int ty = (int)(floor.Height * (floorY - (float)Math.Floor(floorY))) & (floor.Height - 1);
                    floorX += stepX;
                    floorY += stepY;

                    float br = MathHelper.Clamp(1f - (rowDistance / 10f), 0.2f, 1f);
                    _sb.Draw(floor, new Rectangle(x, y, 1, 1), new Rectangle(tx, ty, 1, 1), new Color(Vector3.One * br));
                }
            }

            // 🧱 СТІНИ
            for (int x = 0; x < sw; x++)
            {
                float angle = player.Rotation - ray.FOV / 2 + ray.FOV * x / sw;
                var hit = ray.Cast(player.Position, angle, map);
                float dist = hit.Distance * (float)Math.Cos(angle - player.Rotation);
                zBuffer[x] = dist; // Записуємо відстань для спрайтів

                int wallH = (int)(sh / (dist + 0.0001f));
                Texture2D tex = (hit.WallType == 2) ? door : (hit.WallType == 4 ? finish : wall);

                float tCoord = (Math.Abs(hit.WorldY - (int)hit.WorldY - 0.5f) > Math.Abs(hit.WorldX - (int)hit.WorldX - 0.5f))
                    ? hit.WorldX % 1 : hit.WorldY % 1;

                float br = MathHelper.Clamp(1f - (dist / 12f), 0.3f, 1f);
                _sb.Draw(tex, new Rectangle(x, (sh - wallH) / 2, 1, wallH), 
                    new Rectangle((int)(tCoord * tex.Width), 0, 1, tex.Height), new Color(Vector3.One * br));
            }

            // 👾 ВОРОГИ ТА ІСКРИ
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            foreach (var enemy in enemies)
            {
                // 1. Малюємо самого ворога
                DrawSprite(enemy.Position, enemyTex, enemy.Scale, player, ray, sw, sh, zBuffer, 
                    (int)(Math.Sin(time * 3f) * 15f), Color.White, false);

                // 2. Малюємо його іскри (якщо вони є в списку)
                foreach (var bullet in enemy.Bullets)
                {
                    DrawSprite(bullet.Position, boltTex, 0.15f, player, ray, sw, sh, zBuffer, 0, Color.Yellow, true);
                }
            }

            _sb.End();
        }

        // Універсальний метод для малювання спрайтів (ворогів та куль)
        private void DrawSprite(Vector2 pos, Texture2D tex, float scale, Player player, Raycaster ray, 
            int sw, int sh, float[] zBuffer, int bob, Color col, bool fullBright)
        {
            Vector2 dir = pos - player.Position;
            float dist = dir.Length();
            float angle = (float)Math.Atan2(dir.Y, dir.X) - player.Rotation;

            while (angle < -MathHelper.Pi) angle += MathHelper.TwoPi;
            while (angle > MathHelper.Pi) angle -= MathHelper.TwoPi;

            if (Math.Abs(angle) > ray.FOV) return;

            float size = (sh / (dist + 0.0001f)) * scale;
            float screenX = (0.5f + angle / ray.FOV) * sw;
            int drawY = (int)(sh / 2 - size / 2) + bob;

            int startX = (int)(screenX - size / 2);
            int endX = (int)(screenX + size / 2);

            for (int x = startX; x < endX; x++)
            {
                if (x >= 0 && x < sw && dist < zBuffer[x])
                {
                    int srcX = (int)((x - startX) * tex.Width / size);
                    if (srcX >= 0 && srcX < tex.Width)
                    {
                        float br = fullBright ? 1f : MathHelper.Clamp(1f - (dist / 10f), 0.4f, 1f);
                        _sb.Draw(tex, new Rectangle(x, drawY, 1, (int)size), 
                            new Rectangle(srcX, 0, 1, tex.Height), col * br);
                    }
                }
            }
        }
    }
}