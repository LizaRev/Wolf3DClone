using Microsoft.Xna.Framework.Graphics;

namespace Wolf3DClone.World
{
    public class Wall
    {
        public int Type { get; set; }
        public Texture2D Texture { get; set; }

        public Wall(int type, Texture2D texture)
        {
            Type = type;
            Texture = texture;
        }
    }
}