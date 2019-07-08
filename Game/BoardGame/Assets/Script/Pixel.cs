using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoardGame
{
    public class Pixel
    {
        int _x;
        int _y;
        Color _previousColor;
        Color _newColor;

        public Pixel(int x, int y, Color previousColor, Color newColor)
        {
            X = x;
            Y = y;
            PreviousColor = previousColor;
            NewColor = newColor;
        }

        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public Color PreviousColor { get => _previousColor; set => _previousColor = value; }
        public Color NewColor { get => _newColor; set => _newColor = value; }
    }
}
