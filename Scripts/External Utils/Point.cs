using System;

namespace Drones.Utils
{
    public class Point
    {
        public float x;
        public float y;
        public float z;
        private readonly int index;
        private static int count;

        public Point()
        {
            x = 0;
            y = 0;
            z = 0;
            index = count;
            count++;
        }

        public Point(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            index = count;
            count++;
        }

        public Point(float[] array)
        {
            if (array.Length != 3)
            {
                throw new ArgumentException("Array size must be 3!");
            }
            x = array[0];
            y = array[1];
            z = array[2];
            index = count;
            count++;
        }

        public float[] ToArray()
        {
            float[] val = { x, y, z };
            return val;
        }

        public Point Clone()
        {
            return new Point(ToArray());
        }

        public static Point operator +(Point b, Point c)
        {
            Point a = new Point(b.x + c.x, b.y + c.y, b.z + c.z);
  
            return a;
        }

        public static Point operator -(Point b, Point c)
        {
            Point a = new Point(b.x - c.x, b.y - c.y, b.z - c.z);

            return a;
        }

        public static Point operator *(Point b, float multiplier) 
        {
            Point a = new Point(b.x * multiplier, b.y * multiplier, b.z * multiplier);
            return a;
        }

        public static Point operator *(float multiplier, Point b)
        {
            Point a = new Point(b.x * multiplier, b.y * multiplier, b.z * multiplier);
            return a;
        }

        public static Point operator /(Point b, float divisor)
        {
            Point a = new Point(b.x / divisor, b.y / divisor, b.z / divisor);
            return a;
        }

        public static bool operator ==(Point b, Point c)
        {
            if (b is null && !(c is null) || !(b is null) && c is null) return false;
            if (b is null && c is null) return true;

            return -float.Epsilon < Distance(b, c) && float.Epsilon > Distance(b, c);
        }

        public static bool operator !=(Point b, Point c)
        {
            return !(b == c);
        }

        public static float Dot(Point o, Point a, Point b)
        {
            float dot = 0;
            Point l1 = a - o;
            Point l2 = b - o;
            dot += l1.x * l2.x;
            dot += l1.y * l2.y;
            dot += l1.z * l2.z;
            return dot;
        }

        public static Point Normalize(Point o, Point a)
        {
            return o + (a - o) / Distance(o, a);
        }

        public static float Distance(Point o, Point a)
        {
            Point line = a - o;
            return (float)Math.Sqrt(line.x * line.x + line.y * line.y + line.z * line.z);
        }

        public override bool Equals(object obj)
        {
            return obj is Point && obj.GetHashCode() == GetHashCode() ||
                (this is null && obj is Point && obj is null);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'
        }

        public override string ToString()
        {
            string str = "(" + x.ToString() + ", ";
            str += y.ToString() + ", ";
            str += z.ToString() + ")";

            return str;
        }
    }
}

