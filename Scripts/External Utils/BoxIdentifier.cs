using Drones.Interface;
using System;

namespace Drones.Utils
{
    public class BoxIdentifier
    {

        IClosestPoint unity;
        private readonly Point _Centre;
        private Point[] _Vertices;
        public Point Corner { get; private set; }
        public Point End { get; private set; }
        public Point Start { get; private set; }
        public float Width { get; private set; }
        public float Length { get; private set; }
        public bool TooSmall;
        private readonly float _Epsilon = (float)Math.Cos(Math.PI / 2 - Math.PI / 36);

        private float Abs(float x)
        {
            return (x < 0) ? -x : x;
        }

        public BoxIdentifier(IClosestPoint point)
        {
            unity = point;
            _Centre = new Point(point.GetCentre());
            try 
            {
                GetVertices();
                if (!CheckVolume()) { GetDimensions(); }
            } 
            catch
            {
                GetAltVertices();
                if (!CheckVolume()) { GetDimensions(); }
            }

        }

        private Point GetVertex(Point outside)
        {
            Point vertex = new Point(unity.GetClosestPoint(outside.ToArray()));
            return vertex;
        }

        private bool CheckVolume()
        {
            float volume = Point.Distance(_Vertices[0], _Vertices[1]);
            volume *= Point.Distance(_Vertices[1], _Vertices[2]);

            Point high = _Centre.Clone();
            high.y += 1000f;
            Point low = _Centre.Clone();
            low.y -= 1000f;
            high = new Point(unity.GetClosestPoint(high.ToArray()));
            low = new Point(unity.GetClosestPoint(low.ToArray()));
            volume *= high.y - low.y;
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
            _Vertices = new Point[4];
            Point outside = _Centre.Clone();
            outside.x += 1000f;
            _Vertices[0] = GetVertex(outside);

            outside.x -= 1000f;
            outside.z -= 1000f;
            _Vertices[1] = GetVertex(outside);

            outside.z += 1000f;
            outside.x -= 1000f;
            _Vertices[2] = GetVertex(outside);

            outside.x += 1000f;
            outside.z += 1000f;
            _Vertices[3] = GetVertex(outside);

            _Vertices = SetYZero(_Vertices);

            GetReferencePoint();

            return _Vertices;
        }

        private Point[] GetAltVertices()
        {
            _Vertices = new Point[4];
            Point outside = _Centre.Clone();
            outside.x += 1000f;
            outside.z += 1000f;
            _Vertices[0] = GetVertex(outside);

            outside.z -= 2 * 1000f;
            _Vertices[1] = GetVertex(outside);

            outside.x -= 2 * 1000f;
            _Vertices[2] = GetVertex(outside);

            outside.z += 2 * 1000f;
            _Vertices[3] = GetVertex(outside);

            _Vertices = SetYZero(_Vertices);

            GetReferencePoint();

            return _Vertices;
        }


        private void GetReferencePoint()
        {
            bool assigned = false;
            float min = float.MaxValue;
            for (int i = 0; i < _Vertices.Length; i++)
            {
                int j = (i + 1) % _Vertices.Length;
                int k = (i + 2) % _Vertices.Length;
                int l = (i + 3) % _Vertices.Length;
                Point ji = Point.Normalize(_Vertices[j], _Vertices[i]);
                Point jk = Point.Normalize(_Vertices[j], _Vertices[k]);
                Point ij = Point.Normalize(_Vertices[i], _Vertices[j]);
                Point il = Point.Normalize(_Vertices[i], _Vertices[l]);
                Point kj = Point.Normalize(_Vertices[k], _Vertices[j]);
                Point kl = Point.Normalize(_Vertices[k], _Vertices[l]);
                float nextDot = Point.Dot(_Vertices[k], kj, kl);
                float dot = Point.Dot(_Vertices[j], ji, jk);
                float prevDot = Point.Dot(_Vertices[i], ij, il);
                if (dot > 0 && min > dot && (prevDot > -_Epsilon || nextDot > - _Epsilon))
                {
                    assigned = true;
                    Start = _Vertices[j];
                    Corner = _Vertices[k];
                    min = dot;
                }
            }
            if (!assigned)
            {
                Start = _Vertices[0];
                Corner = _Vertices[1];
            }
        }

        private Point GetFurthest(Point o)
        {
            MaxHeap<Point> sorter = new MaxHeap<Point>((Point a, Point b) =>
            {
                return (Point.Distance(o, a) > Point.Distance(o, b)) ? 1 : -1;
            });

            foreach (Point point in _Vertices)
            {
                sorter.Add(point);
            }

            return sorter.Remove();
        }

        private float MinDistance()
        {
            float min = float.MaxValue;
            for (int i = 0; i < _Vertices.Length - 1; i++)
            {
                for (int j = i + 1; j < _Vertices.Length; j++)
                {
                    float a = Point.Distance(_Vertices[i], _Vertices[j]);
                    if (min > a) { min = a; }
                }
            }
            return min;
        }

        private void GetDimensions()
        {
            Point one = Point.Normalize(Start, Corner);

            int step = ExponentialSearch(Start, Corner, one);

            if (step > 0)
            {
                Corner = BisectionSearch(Start, Corner, one, unity.Exp(step - 1), unity.Exp(step), _Epsilon);
            }

            Length = Point.Distance(Start, Corner);
            End = Start - Corner;
            End = new Point(-End.z, End.y, End.x) + Corner;
            one = Point.Normalize(Corner, End);
            End = Corner + (one - Corner) * MinDistance();
            step = ExponentialSearch(Corner, End, one);

            if (step > 0)
            {
                End = BisectionSearch(Corner, End, one, unity.Exp(step - 1), unity.Exp(step), _Epsilon);
            }
            Width = Point.Distance(Corner, End);
            /* refPoint -> lineEnd -> lineEnd2 ALWAYS clockwise; vertices[i++] ALWAYS clockwise */
        }

        private bool CanSearch(Point origin, Point end)
        {
            for (int i = 0; i < _Vertices.Length; i++)
            {
                float dot = Point.Dot(end, origin, _Vertices[i]);
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
            for (int i = 0; i < _Vertices.Length; i++)
            {
                float dot = Point.Dot(newEnd, normalize, Point.Normalize(newEnd, _Vertices[i]));
                if (dot < 0) 
                {
                    return ExponentialSearch(origin, end, one, step + 1); 
                }
            }
            return step;
        }
        int steps;
        private Point BisectionSearch(Point origin, Point end, Point one, float min, float max, float epsilon)
        {
            float mid = (min + max) / 2;
            Point newEnd = end + mid * (one - origin);
            Point normalize = Point.Normalize(newEnd, origin);
            float minDot = float.MaxValue;

            for (int i = 0; i < _Vertices.Length; i++)
            {
                float dot = Point.Dot(newEnd, normalize, Point.Normalize(newEnd, _Vertices[i]));
                if (dot < -epsilon)
                {
                    steps++;

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
