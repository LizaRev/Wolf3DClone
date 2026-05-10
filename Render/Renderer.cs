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

        public void DrawMenu(Texture2D bg, Texture2D title, Rectangle tRect, Texture2D start, Rectangle sRect, Texture2D exit, Rectangle eRect, int selected)
        {
            _gd.Clear(Color.Black);
            _sb.Begin();
            
            _sb.Draw(bg, new Rectangle(0, 0, _gd.Viewport.Width, _gd.Viewport.Height), Color.White);
            
            _sb.Draw(title, tRect, Color.White);
            
            _sb.Draw(start, sRect, (selected == 0) ? Color.Gold : Color.White);
            _sb.Draw(exit, eRect, (selected == 1) ? Color.Gold : Color.White);
            
            _sb.End();
        }

        public void DrawWinScreen(Texture2D photo)
        {
            _gd.Clear(Color.Black);
            _sb.Begin();
            _sb.Draw(photo, new Rectangle(0, 0, _gd.Viewport.Width, _gd.Viewport.Height), Color.White);
            _sb.End();
        }

        public void Draw(Player player, Map map, Raycaster ray,
            Texture2D wall, Texture2D door, Texture2D finish, Texture2D floor,
            Texture2D enemyTex, Texture2D boltTex, List<Enemy> enemies, List<Projectile> playerBullets, GameTime gameTime)
        {
            int sw = _gd.Viewport.Width;
            int sh = _gd.Viewport.Height;
            float[] zBuffer = new float[sw];

            _gd.Clear(Color.Black);
            _sb.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            _sb.Draw(_pixel, new Rectangle(0, 0, sw, sh / 2), Color.Wheat);

            for (int y = sh / 2; y < sh; y++)
            {
                float rowDist = (0.5f * sh) / (y - sh / 2f + 0.0001f);
                
                float fX = player.Position.X + rowDist * (float)Math.Cos(player.Rotation - ray.FOV / 2);
                float fY = player.Position.Y + rowDist * (float)Math.Sin(player.Rotation - ray.FOV / 2);
                
                float stepX = rowDist * (float)(Math.Cos(player.Rotation + ray.FOV / 2) - Math.Cos(player.Rotation - ray.FOV / 2)) / sw;
                float stepY = rowDist * (float)(Math.Sin(player.Rotation + ray.FOV / 2) - Math.Sin(player.Rotation - ray.FOV / 2)) / sw;

                for (int x = 0; x < sw; x++)
                {
                    int tx = (int)(floor.Width * (fX % 1)) & (floor.Width - 1);
                    int ty = (int)(floor.Height * (fY % 1)) & (floor.Height - 1);
                    
                    float br = MathHelper.Clamp(1f - (rowDist / 10f), 0.2f, 1f);
                    _sb.Draw(floor, new Rectangle(x, y, 1, 1), new Rectangle(tx, ty, 1, 1), new Color(Vector3.One * br));
                    
                    fX += stepX; 
                    fY += stepY;
                }
            }

            for (int x = 0; x < sw; x++)
            {
                float angle = player.Rotation - ray.FOV / 2 + ray.FOV * x / sw;
                var hit = ray.Cast(player.Position, angle, map);
                float dist = hit.Distance * (float)Math.Cos(angle - player.Rotation);
                zBuffer[x] = dist;

                int h = (int)(sh / (dist + 0.0001f));
                Texture2D tex = (hit.WallType == 2) ? door : (hit.WallType == 4 ? finish : wall);
                
                float tc = (Math.Abs(hit.WorldY % 1 - 0.5f) > Math.Abs(hit.WorldX % 1 - 0.5f)) ? hit.WorldX % 1 : hit.WorldY % 1;
                float br = MathHelper.Clamp(1f - (dist / 12f), 0.3f, 1f);
                
                _sb.Draw(tex, new Rectangle(x, (sh - h) / 2, 1, h), 
                        new Rectangle((int)(tc * tex.Width), 0, 1, tex.Height), 
                        new Color(Vector3.One * br));
            }

            foreach (var e in enemies)
            {
                int bob = (int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5) * 10);
                DrawSprite(e.Position, enemyTex, e.Scale, player, ray, sw, sh, zBuffer, bob, Color.White, false, e.Health);
                
                foreach (var b in e.Bullets) 
                    DrawSprite(b.Position, boltTex, 0.1f, player, ray, sw, sh, zBuffer, 0, Color.Yellow, true);
            }

            foreach (var b in playerBullets)
                DrawSprite(b.Position, boltTex, 0.1f, player, ray, sw, sh, zBuffer, 0, Color.Cyan, true);

            int uiW = 200; int uiH = 25; int m = 20;
            Rectangle barBG = new Rectangle(sw - uiW - m, m, uiW, uiH);
            _sb.Draw(_pixel, barBG, Color.Black * 0.5f);
            
            float hRatio = MathHelper.Clamp(player.Health / 100f, 0, 1);
            Color hCol = player.Health > 50 ? Color.Lime : (player.Health > 25 ? Color.Yellow : Color.Red);
            _sb.Draw(_pixel, new Rectangle(barBG.X + 2, barBG.Y + 2, (int)((uiW - 4) * hRatio), uiH - 4), hCol);
            
            _sb.Draw(_pixel, new Rectangle(barBG.X, barBG.Y, uiW, 1), Color.White);
            _sb.Draw(_pixel, new Rectangle(barBG.X, barBG.Y + uiH, uiW, 1), Color.White);
            _sb.Draw(_pixel, new Rectangle(barBG.X, barBG.Y, 1, uiH), Color.White);
            _sb.Draw(_pixel, new Rectangle(barBG.X + uiW, barBG.Y, 1, uiH + 1), Color.White);

            _sb.End();
        }

        private void DrawSprite(Vector2 pos, Texture2D tex, float sc, Player p, Raycaster r, int sw, int sh, float[] zb, int bob, Color col, bool bright, float health = -1)
        {
            Vector2 dir = pos - p.Position;
            float dist = dir.Length();
            float ang = (float)Math.Atan2(dir.Y, dir.X) - p.Rotation;
            
            while (ang < -MathHelper.Pi) ang += MathHelper.TwoPi;
            while (ang > MathHelper.Pi) ang -= MathHelper.TwoPi;
            
            if (Math.Abs(ang) > r.FOV) return;

            float sz = (sh / (dist + 0.0001f)) * sc;
            float sX = (0.5f + ang / r.FOV) * sw;
            int dY = (int)(sh / 2 - sz / 2) + bob;

            for (int x = (int)(sX - sz / 2); x < (int)(sX + sz / 2); x++)
            {
                if (x >= 0 && x < sw && dist < zb[x])
                {
                    int tx = (int)((x - (sX - sz / 2)) * tex.Width / sz);
                    if (tx >= 0 && tx < tex.Width)
                    {
                        _sb.Draw(tex, new Rectangle(x, dY, 1, (int)sz), new Rectangle(tx, 0, 1, tex.Height), col);
                        
                        if (health >= 0 && x >= (int)(sX - sz / 4) && x < (int)(sX + sz / 4))
                        {
                            _sb.Draw(_pixel, new Rectangle(x, dY - 15, 1, 5), Color.Red);
                            if ((x - (sX - sz / 4)) / (sz / 2) < health / 100f)
                                _sb.Draw(_pixel, new Rectangle(x, dY - 15, 1, 5), Color.Lime);
                        }
                    }
                }
            }
        }
    }
}