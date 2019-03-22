using Drones.Interface;
using System;

namespace Drones.Utils
{
    public class BoxIdentifier
    {

        IClosestPoint unity;
        readonly Point centre;
        private Point[] vertices;
        public Point Corner { get; private set; }
        public Point End { get; private set; }
        public Point Start { get; private set; }
        public float Width { get; private set; }
        public float Length { get; private set; }
        public bool TooSmall;

        private float Abs(float x)
        {
            return (x < 0) ? -x : x;
        }

        public BoxIdentifier(IClosestPoint point)
        {
            unity = point;
            centre = new Point(point.GetCentre());
            GetVertices();
            if (!CheckVolume()) { GetDimensions(); }
        }
        private Point GetVertex(Point outside)
        {
            Point vertex = new Point(unity.GetClosestPoint(outside.ToArray()));
            return vertex;
        }

        private bool CheckVolume()
        {
            float volume = Point.Distance(vertices[0], vertices[1]);
            volume *= Point.Distance(vertices[1], vertices[2]);
            Point high = centre.Clone();
            high.y += 200;
            Point low = centre.Clone();
            low.y -= 200;
            high = new Point(unity.GetClosestPoint(high.ToArray()));
            low = new Point(unity.GetClosestPoint(low.ToArray()));
            volume *= Point.Distance(low, high);
            TooSmall = volume < 10;

            return TooSmall;
        }

        private Point[] SetYZero(Point[] points)
        {
            foreach (Point point in points)
            {
                point.y = 0;
            }
            return points;
        }
        private void Print(Point[] points, string note)
        {
            foreach (Point point in points)
            {
                unity.Message(point.y.ToString() + " " + note);
            }
        }

        private Point[] GetVertices()
        {
            vertices = new Point[4];
            Point outside = centre.Clone();
            outside.x += Constants.unityTileSize;
            vertices[0] = GetVertex(outside);

            outside.x -= Constants.unityTileSize;
            outside.z -= Constants.unityTileSize;
            vertices[1] = GetVertex(outside);

            outside.z += Constants.unityTileSize;
            outside.x -= Constants.unityTileSize;
            vertices[2] = GetVertex(outside);

            outside.x += Constants.unityTileSize;
            outside.z += Constants.unityTileSize;
            vertices[3] = GetVertex(outside);

            vertices = SetYZero(vertices);

            GetReferencePoint();

            return vertices;
        }
        
        private void GetReferencePoint()
        {
            bool assigned = false;
            float min = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                int k = (i + 2) % vertices.Length;
                int l = (i + 3) % vertices.Length;
                float dot = Point.Dot(vertices[j], vertices[i], vertices[k]);
                float prevDot = Point.Dot(vertices[i], vertices[j], vertices[l]);
                if (dot > 0 && min > dot && prevDot > -Constants.EPSILON)
                {
                    assigned = true;
                    Start = vertices[j];
                    Corner = vertices[k];
                    min = dot;
                }
            }
            if (!assigned)
            {
                Start = vertices[0];
                Corner = vertices[1];
            }
        }

        private Point GetFurthest(Point o)
        {
            MaxHeap<Point> sorter = new MaxHeap<Point>((Point a, Point b) =>
            {
                return (Point.Distance(o, a) > Point.Distance(o, b)) ? 1 : -1;
            });

            foreach (Point point in vertices)
            {
                sorter.Add(point);
            }

            return sorter.Remove();
        }

        private float MinDistance()
        {
            float min = float.MaxValue;
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                for (int j = i + 1; j < vertices.Length; j++)
                {
                    float a = Point.Distance(vertices[i], vertices[j]);
                    if (min > a) { min = a; }
                }
            }
            return min;
        }

        private void GetDimensions()
        {
            float epsilon = (float) Math.Cos(Math.PI / 2 - Math.PI / 36);
            Point one = Point.Normalize(Start, Corner);

            int step = ExponentialSearch(Start, Corner, one);
            if (step > 0)
            {
                Corner = BisectionSearch(Start, Corner, one, unity.Exp(step - 1), unity.Exp(step), epsilon);
            }
            Length = Point.Distance(Start, Corner);
            End = Start - Corner;
            End = new Point(-End.z, End.y, End.x) + Corner;
            one = Point.Normalize(Corner, End);
            End = Corner + (one - Corner) * MinDistance();
            step = ExponentialSearch(Corner, End, one);

            if (step > 0)
            {
                End = BisectionSearch(Corner, End, one, unity.Exp(step - 1), unity.Exp(step), epsilon);
            }
            Width = Point.Distance(Corner, End);
            /* refPoint -> lineEnd -> lineEnd2 ALWAYS clockwise; vertices[i++] ALWAYS clockwise */
        }

        private bool CanSearch(Point origin, Point end)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                float dot = Point.Dot(end, origin, vertices[i]);
                if (dot < 0)
                {
                    return true;
                }
            }
            return false;
        }

        private int ExponentialSearch(Point origin, Point end, Point one, int step = 0)
        {
            Point newEnd = end + unity.Exp(step) * (one - origin);
            Point normalize = Point.Normalize(newEnd, origin);
            for (int i = 0; i < vertices.Length; i++)
            {
                float dot = Point.Dot(newEnd, normalize, Point.Normalize(newEnd, vertices[i]));
                if (dot < 0) 
                {
                    return ExponentialSearch(origin, end, one, step + 1); 
                }
            }
            return step;
        }

        private Point BisectionSearch(Point origin, Point end, Point one, float min, float max, float epsilon)
        {
            float mid = (min + max) / 2;
            Point newEnd = end + mid * (one - origin);
            Point normalize = Point.Normalize(newEnd, origin);
            float minDot = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float dot = Point.Dot(newEnd, normalize, Point.Normalize(newEnd, vertices[i]));
                if (dot < -epsilon) 
                {
                    return BisectionSearch(origin, end, one, mid, max, epsilon);
                }
                if (minDot > dot) { minDot = dot; }
            }

            if (minDot > epsilon)
            {
                return BisectionSearch(origin, end, one, min, mid, epsilon);
            }
            return newEnd;
        }




    }
}
