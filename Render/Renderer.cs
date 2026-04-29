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
            Texture2D enemyTex, List<Enemy> enemies, GameTime gameTime)
        {
            int sw = _gd.Viewport.Width;
            int sh = _gd.Viewport.Height;
            float[] zBuffer = new float[sw];

            _gd.Clear(Color.Black);
            _sb.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            // 🌌 СТЕЛЯ
            _sb.Draw(_pixel, new Rectangle(0, 0, sw, sh / 2), new Color(230, 220, 200));

            // 🌍 ПІДЛОГА
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
                    float fx = floorX - (float)Math.Floor(floorX);
                    float fy = floorY - (float)Math.Floor(floorY);
                    int tx = (int)(floor.Width * fx) & (floor.Width - 1);
                    int ty = (int)(floor.Height * fy) & (floor.Height - 1);
                    floorX += stepX;
                    floorY += stepY;

                    float brightness = MathHelper.Clamp(1f - (rowDistance / 10f), 0.2f, 1f);
                    _sb.Draw(floor, new Rectangle(x, y, 1, 1), new Rectangle(tx, ty, 1, 1), new Color(Vector3.One * brightness));
                }
            }

            // 🧱 СТІНИ (Записуємо в Z-буфер)
            for (int x = 0; x < sw; x++)
            {
                float angle = player.Rotation - ray.FOV / 2 + ray.FOV * x / sw;
                var hit = ray.Cast(player.Position, angle, map);
                float dist = hit.Distance * (float)Math.Cos(angle - player.Rotation);
                zBuffer[x] = dist;

                int wallHeight = (int)(sh / (dist + 0.0001f));
                Texture2D tex = (hit.WallType == 2) ? door : (hit.WallType == 4 ? finish : wall);

                float texCoord = (Math.Abs(hit.WorldY - (int)hit.WorldY - 0.5f) > Math.Abs(hit.WorldX - (int)hit.WorldX - 0.5f))
                    ? (hit.WorldX - (float)Math.Floor(hit.WorldX))
                    : (hit.WorldY - (float)Math.Floor(hit.WorldY));

                int sourceX = (int)(texCoord * tex.Width);
                float brightness = MathHelper.Clamp(1f - (dist / 12f), 0.3f, 1f);

                _sb.Draw(tex, new Rectangle(x, (sh - wallHeight) / 2, 1, wallHeight), new Rectangle(sourceX, 0, 1, tex.Height), new Color(Vector3.One * brightness));
            }

            // 👾 ВОРОГИ
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            foreach (var enemy in enemies)
            {
                Vector2 dir = enemy.Position - player.Position;
                float dist = dir.Length();
                float angleToEnemy = (float)Math.Atan2(dir.Y, dir.X);
                float relativeAngle = angleToEnemy - player.Rotation;

                while (relativeAngle < -MathHelper.Pi) relativeAngle += MathHelper.TwoPi;
                while (relativeAngle > MathHelper.Pi) relativeAngle -= MathHelper.TwoPi;

                // Якщо ворог поза полем зору — не малюємо
                if (Math.Abs(relativeAngle) > ray.FOV) continue;

                // РОЗМІР З УРАХУВАННЯМ Scale
                float size = (sh / (dist + 0.0001f)) * enemy.Scale;
                float screenX = (0.5f + relativeAngle / ray.FOV) * sw;
                
                // Левітація (плавне погойдування)
                int bobbing = (int)(Math.Sin(time * 3f) * (size * 0.15f)); 
                int drawY = (int)(sh / 2 - size / 2) + bobbing;

                int startX = (int)(screenX - size / 2);
                int endX = (int)(screenX + size / 2);

                for (int x = startX; x < endX; x++)
                {
                    // Перевірка Z-буфера: ворог не малюється крізь стіни
                    if (x >= 0 && x < sw && dist < zBuffer[x])
                    {
                        // ВИПРАВЛЕНО: Співвідношення пікселів екрану до пікселів текстури
                        int sourceX = (int)((x - startX) * enemyTex.Width / size);
                        
                        // Додаткова перевірка, щоб не вийти за межі ширини картинки
                        if (sourceX >= 0 && sourceX < enemyTex.Width)
                        {
                            float brightness = MathHelper.Clamp(1f - (dist / 10f), 0.3f, 1f);
                            _sb.Draw(enemyTex, 
                                new Rectangle(x, drawY, 1, (int)size), 
                                new Rectangle(sourceX, 0, 1, enemyTex.Height), 
                                new Color(Vector3.One * brightness));
                        }
                    }
                }
            }
            _sb.End();
        }
    }
}