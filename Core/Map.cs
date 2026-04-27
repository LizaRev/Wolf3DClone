using System;

namespace Wolf3DClone.Core
{
    public class Map
    {
        public int Width => 15;
        public int Height => 15;

        public float[,] DoorOpen = new float[15, 15];

        public int[,] FinalGrid = new int[15, 15]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}, // 0
            {1,0,0,0,1,0,0,0,0,0,0,0,0,0,1}, // 1
            {1,0,0,0,2,0,1,1,1,1,1,1,1,0,1}, // 2
            {1,0,0,0,1,0,1,0,0,0,0,0,1,0,1}, // 3
            {1,1,2,1,1,0,1,0,1,1,1,0,1,0,1}, // 4
            {1,0,0,0,1,0,1,0,1,0,0,0,1,0,1}, // 5
            {1,1,1,1,1,0,1,0,1,0,1,1,1,0,1}, // 6
            {1,0,0,0,0,0,0,0,1,0,1,0,0,0,1}, // 7
            {1,1,1,1,1,1,1,2,1,0,1,0,1,1,1}, // 8
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1}, // 9
            {1,0,1,1,1,1,1,1,1,1,1,1,1,0,1}, // 10
            {1,0,1,0,0,0,0,0,0,1,1,1,1,0,1}, // 11
            {1,0,1,0,1,1,1,1,0,4,4,4,1,0,1}, // 12 -- Верхня стіна кімнати
            {1,0,0,0,1,0,0,1,0,4,0,4,1,0,1}, // 13 -- Середина (4 0 4)
            {1,1,1,1,1,1,1,1,0,2,4,4,1,1,1}  // 14 -- Вхід через двері (2) в червоне
        };

        public int Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return 1;
            return FinalGrid[y, x];
        }

        public void ToggleDoor(float px, float py, float angle)
        {
            int x = (int)(px + Math.Cos(angle) * 0.7f);
            int y = (int)(py + Math.Sin(angle) * 0.7f);
            if (Get(x, y) != 2) return;
            DoorOpen[y, x] = (DoorOpen[y, x] >= 1f) ? 0f : 1f;
        }
    }
}