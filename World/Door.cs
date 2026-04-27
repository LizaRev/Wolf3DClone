using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wolf3DClone.World
{
    public class Door
    {
        public Vector2 Position { get; set; }
        public bool IsOpen { get; set; }
        public float OpenAmount { get; set; } // 0.0 до 1.0
        public Texture2D Texture { get; set; }

        public Door(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            IsOpen = false;
            OpenAmount = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (IsOpen && OpenAmount < 1.0f)
                OpenAmount += 0.05f;
            else if (!IsOpen && OpenAmount > 0.0f)
                OpenAmount -= 0.05f;
        }
    }
}