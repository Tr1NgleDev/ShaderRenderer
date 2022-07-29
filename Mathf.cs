using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace ShaderRenderer
{
    public static class Mathf
    {
        public static float Cos(float rad)
        {
            return (float)Math.Cos(rad);
        }
        public static float Sin(float rad)
        {
            return (float)Math.Sin(rad);
        }
        public static float CosD(float degree)
        {
            return (float)Math.Cos(MathHelper.DegreesToRadians(degree));
        }

        public static int Floor(float val)
        {
            return (int)Math.Floor(val);
        }

        public static float SinD(float degree)
        {
            return (float)Math.Sin(MathHelper.DegreesToRadians(degree));
        }
        public static float DistanceVector3(Vector3 a, Vector3 b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) +
                Math.Pow(b.Y - a.Y, 2) +
                Math.Pow(b.Z - a.Z, 2) * 1.0);
        }
        public static float Round(float decimalVal, int digits = 0, bool toEven = false, bool doMinusIfNotEven = false)
        {
            float cVal = (float)Math.Round(decimalVal, digits);
            if (toEven)
            {
                if (cVal / 2f != (int)(cVal / 2))
                    if (doMinusIfNotEven)
                        cVal--;
                    else
                        cVal++;
            }
            return cVal;
        }
        public static Vector3 Round(Vector3 decimalVec, int digits = 0, bool toEven = false, bool doMinusIfNotEven = false)
        {
            return new Vector3(Round(decimalVec.X, digits, toEven, doMinusIfNotEven), Round(decimalVec.Y, digits, toEven, doMinusIfNotEven), Round(decimalVec.Z, digits, toEven, doMinusIfNotEven));
        }
        public static Vector3 SplitVector3(Vector3 vec, float value)
        {
            return new Vector3(vec.X % value, vec.Y % value, vec.Z % value);
        }
    }
}
