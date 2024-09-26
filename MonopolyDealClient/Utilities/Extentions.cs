using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace MonopolyDeal
{ 
    public static class Extentions
    {
        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }
}
