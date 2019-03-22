using System;
using UnityEngine;
using Drones.Utils;

namespace Drones.Utils.Extensions
{
    public static class Vector3Extension
    {
        // Start is called before the first frame update
        public static float[] ToArray(this Vector3 vector)
        {
            float[] value = { vector.x, vector.y, vector.z };
            return value;
        }
        public static Point ToPoint(Vector3 source)
        {
            return new Point(source.x, source.y, source.z);
        }
    }
}

