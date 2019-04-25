using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Drones.Utils;
using System;

namespace Drones.Routing
{
    public struct MockObstacle
    {
        public Vector3 position;
        public Vector3 size;
        public Vector3 orientation;
        public float diag;
        public Vector3 dz;
        public Vector3 dx;
        public float mu;
        public Vector3[] normals;
        public Vector3[] verts;
    }

    public static class AirTraffic
    {
        static List<List<MockObstacle>> Buildings;
        static List<MockObstacle> NoFlys = new List<MockObstacle>();
        const float maxAlt = 250;
        const float minAlt = 150;
        const int altDiv = 10; // Altitude interval
        const int buildingDiv = 30; // Building bucket height interval
        const int R_a = 200; // Corridor width
        const int R_d = 3; // exagerated
        const float epsilon = 0.001f;
        static readonly int[] hubAlt = { 500, 510 }; // hub altitudes, 500 for northbound drones, 510 for south
        static float droneCount;
        static float[] _altitudes;
        static int[] _assigned;
        static float[] Altitudes
        {
            get
            {
                if (_altitudes == null)
                {
                    _altitudes = new float[(int)((maxAlt - minAlt) / altDiv)];
                    _assigned = new int[(int)((maxAlt - minAlt) / altDiv)];
                    for (int i = 0; i < _altitudes.Length; i++)
                    {
                        _altitudes[i] = minAlt + i * altDiv;
                        _assigned[i] = 0;
                    }
                }
                return _altitudes;
            }
        }
        static Vector3 _origin;
        static Vector3 _destination;

        public static int HashVector(Vector3 v)
        {
            return (v.x.ToString("0.000") + v.z.ToString("0.000")).GetHashCode();
        }

        private static void GetNormalsAndVerts(ref MockObstacle obs)
        {
            obs.normals = new Vector3[4];
            obs.verts = new Vector3[4];
            obs.normals[0] = obs.dz.normalized;
            obs.normals[1] = obs.dx.normalized;
            obs.normals[2] = -obs.normals[0];
            obs.normals[3] = -obs.normals[1];

            obs.verts[0] = obs.position + obs.dz + obs.dx; // ij
            obs.verts[1] = obs.position - obs.dz + obs.dx; // jk
            obs.verts[2] = obs.position - obs.dz - obs.dx; // kl
            obs.verts[3] = obs.position + obs.dz - obs.dx; // li
        }

        public static void GetBuildings(MockObstacle[] o)
        {
            int added = 0;
            int i = 0;
            Buildings = new List<List<MockObstacle>>();
            while (added < o.Length)
            {
                Buildings.Add(new List<MockObstacle>());
                // Split buildings into buckets of 30m interval in height, e.g. 0-30m, 30-60m, 60-90m, etc.
                for (int j = 0; j < o.Length; j++)
                {
                    if (o[j].size.y < (i + 1) * buildingDiv && o[j].size.y >= i * buildingDiv)
                    {
                        o[j].position.y = 0;
                        o[j].dx = RotationY(o[j].orientation.y) * Vector3.right * o[j].size.x / 2;
                        o[j].dz = RotationY(o[j].orientation.y) * Vector3.forward * o[j].size.z / 2;
                        o[j].diag = new Vector2(o[j].size.x, o[j].size.z).magnitude;

                        GetNormalsAndVerts(ref o[j]);
                        Buildings[i].Add(o[j]);
                        added++;
                    }
                }
                i++;
            }
        } //Initialized presimulation

        private static void SetAltitude(MockObstacle obs, float alt)
        {
            obs.position.y = alt;
            for (int i = 0; i < obs.normals.Length; i++)
            {
                obs.normals[i].y = alt;
                obs.verts[i].y = alt;
            }
        }

        public static void UpdateGameState(int drones, int[] completed, List<MockObstacle> noflys)
        {
            droneCount = drones; // total number of drones in service
            for (int j = 0; j < noflys.Count; j++)
            {
                var obs = noflys[j];
                obs.position.y = 0;
                obs.dx = RotationY(obs.orientation.y) * Vector3.right * obs.size.x / 2;
                obs.dz = RotationY(obs.orientation.y) * Vector3.forward * obs.size.z / 2;
                obs.diag = new Vector2(obs.size.x, obs.size.z).magnitude;

                GetNormalsAndVerts(ref obs);
                noflys[j] = obs;
            }
            NoFlys = noflys;
            if (Altitudes.Length > 4) { }
            for (int i = 0; i < _assigned.Length; i++)
            {
                _assigned[i] -= completed[i]; // Number of jobs completed at the current altitude
                // alternative would be to set a timer have it reduce periodically
            }

        }
        // Generates rotation matrix with theta degrees 4x4 matrix because unity doesn't have 3x3
        // 3x3 should be the same matrix without the 4th column and 4th row
        public static Matrix4x4 RotationY(float theta) 
        {
            theta *= Mathf.PI / 180;

            return new Matrix4x4(new Vector4(Mathf.Cos(theta), 0, -Mathf.Sin(theta)),
                new Vector4(0, 1, 0), new Vector4(Mathf.Sin(theta), 0, Mathf.Cos(theta)), new Vector4(0, 0, 0, 1));
        }

        private static int ChooseAltitude(Vector3 origin, Vector3 dest)
        {
            float max = 0;

            int start = ((dest - origin).z > 0) ? 0 : 1; // North bound => even; South bound => odd

            int maxindex = _assigned.Length - 1;
            for (int i = start; i < _assigned.Length; i+=2)
            {
                // maximise altitude, minimize traffic, + 1 to prevent singularity
                float tmp = Altitudes[i] / maxAlt / (_assigned[i] / droneCount + 1);
                if (tmp > max)
                {
                    max = tmp;
                    maxindex = i;
                }
            }

            _assigned[maxindex]++;
            return maxindex;
        }

        // Get a direction vector perpendicular to dir 90 clockwise about y-axis looking from top to bottom
        private static Vector3 GetPerp(Vector3 dir)
        {
            Vector4 tmp1 = dir;
            tmp1.w = 0;
            Vector4 tmp2 = RotationY(90) * tmp1;
            tmp2.w = 0;
            return tmp2;
        }

        // The public interface to get the list of waypoints
        public static List<Vector3> Route(Vector3 origin, Vector3 dest, bool returnToHub)
        {
            float alt = returnToHub ? hubAlt[(dest - origin).z > 0 ? 0 : 1] : Altitudes[ChooseAltitude(origin, dest)];
            _origin = origin; 
            _destination = dest; // Cached in global/static var for later use
            _origin.y = 0;
            _destination.y = 0;
            try
            {
                var waypoints = Navigate(_origin, _destination, alt);
                // Decide on the altitudes
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Vector3 v = waypoints[i];
                    v.y = alt;
                    waypoints[i] = v;
                }
                return waypoints;
            }
            catch (StackOverflowException)
            {
                Debug.Log(frame);
                frame = 0;
                return null;
            }
            
        }
        // Get a sorted list/heap of buildings in a corridor between start and end
        private static MinHeap<MockObstacle> BlockingBuildings(Vector3 start, Vector3 end, float alt)
        {
            Vector3 direction = end - start;

            // Sorted by normalized projected distance
            MinHeap<MockObstacle> obstacles = new MinHeap<MockObstacle>((a, b) =>
            {
                if (a.mu <= b.mu) { return -1; }
                return 1;
            });

            // R_a is the corridor half-width
            Vector3 perp = GetPerp(direction).normalized * R_a; 

            int startIndex = (int)(alt / buildingDiv); // i.e. the building list index where we should start

            for (int i = startIndex; i < Buildings.Count; i++)
            {
                for (int j = 0; j < Buildings[i].Count; j++)
                {
                    var obs = Buildings[i][j];
                    if (obs.size.y > alt - altDiv / 2)
                    {
                        // normalized projected distance
                        float mu = Vector3.Dot(obs.position - start, direction) / direction.sqrMagnitude;
                        // normalized perpendicular distance
                        float nu = Vector3.Dot(start - obs.position, perp) / perp.sqrMagnitude;
                        if (nu <= 1 && nu >= -1 && mu <= 1 + R_a / direction.magnitude && mu >= 0)
                        {
                            obs.mu = mu;
                            obstacles.Add(obs);
                        }
                    }

                }
            }

            return obstacles;
        }

        // swap function
        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private static int FindIntersect(MockObstacle obs, Vector3 start, Vector3 end, out int[] indices)
        {
            var dir = end - start;
            var _dir = dir.normalized;
            Vector3 path(float m) => start + m * dir; // 0 < mu < 1
            int NumberOfIntersects = 0;
            float[] mu = new float[4];
            indices = new int[] { -1, -1};
            for (int j = 0; j < obs.normals.Length; j++)
            {
                // if heading not parallel to surface
                if (Mathf.Abs(Vector3.Dot(_dir, obs.normals[j])) > epsilon)
                {
                    // Solve ray-plane intersection 
                    // f(mu).n = P0.n 
                    // where . is dot product, n is plane normal, P0 is a point on the plane, f(mu) is ray equation
                    mu[j] = Vector3.Dot(obs.verts[j] - start, obs.normals[j]) / Vector3.Dot(dir, obs.normals[j]);
                    if (Vector3.Distance(path(mu[j]), obs.position) < obs.diag / 2 && mu[j] > 0 && mu[j] <= 1)
                    {
                        indices[NumberOfIntersects++] = j;
                    }
                }
            }

            if (NumberOfIntersects > 1 && mu[indices[1]] < mu[indices[0]])
            {
                Swap(ref indices[0], ref indices[1]); // make sure the first index refers to the closer intersect
            }

            return NumberOfIntersects;
        }

        private static Vector3 FindOtherWaypoint(MockObstacle obs, Vector3 start, Vector3 not)
        {
            foreach (var vert in obs.verts)
            {
                var point = vert + R_d * (vert - obs.position).normalized;
                if ((point - start).magnitude > epsilon && (point - not).magnitude > epsilon && (point - not).magnitude < obs.diag)
                {
                    return point;
                }
            }
            return start;
        }

        private static bool IsContained(MockObstacle obs, Vector3 p)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Vector3.Dot(p - obs.verts[i], obs.normals[i]) > 0) return false;
            }
            return true;
        }

        private static Vector3 FindWaypoint(MockObstacle obs, Vector3 start, Vector3 end, int[] indices)
        {
            var _dir = (end - start).normalized;
            Vector3 waypoint;

            if (indices[1] == -1)
            {
                // If only one intersetion detected sets the way point near the vertex clockwise from the 
                // intersection point
                int num = FindIntersect(obs, start, end + obs.diag * _dir, out int[] indi);
                if (num > 0) return FindWaypoint(obs, start, end + obs.diag * _dir, indi);
            }

            Vector3 a;
            Vector3 b;
            if (Mathf.Abs(indices[1] - indices[0]) == 1 || Mathf.Abs(indices[1] - indices[0]) == 3)
            {
                // indices previously swapped to ensure 1 is bigger than 0
                // adjacent faces interseciton
                int j;
                if (Mathf.Abs(indices[1] - indices[0]) == 1) j = indices[1] < indices[0] ? indices[1] : indices[0];
                else j = 3;
                a = obs.verts[j] + R_d * (obs.verts[j] - obs.position).normalized;
                b = obs.verts[(j + 1) % 4] + R_d * (obs.verts[(j + 1) % 4] - obs.position).normalized;

                if ((a - start).magnitude > epsilon)
                {
                    // Gets the waypoint with the smallest deviation angle from the path
                    waypoint = a;
                }
                else
                {
                    // I think its possible for opposite face intersection to obtain the same point again 
                    // but I might be wrong, this is to prevent it
                    waypoint = ((a - start).magnitude > epsilon) ? a : b;
                }
            }
            else
            {
                // opposite faces interseciton
                a = obs.verts[indices[0]] + R_d * (obs.verts[indices[0]] - obs.position).normalized;
                b = obs.verts[(indices[1] + 1) % 4] + R_d * (obs.verts[(indices[1] + 1) % 4] - obs.position).normalized;
                if ((a - start).magnitude > epsilon && (b - start).magnitude > epsilon)
                {
                    // Gets the waypoint with the smallest deviation angle from the path
                    waypoint = Mathf.Abs(Vector3.Dot((a - start).normalized, _dir)) >
                        Mathf.Abs(Vector3.Dot((b - start).normalized, _dir)) ? a : b;
                }
                else
                {
                    // I think its possible for opposite face intersection to obtain the same point again 
                    // but I might be wrong, this is to prevent it
                    waypoint = ((a - start).magnitude > epsilon) ? a : b;
                }
            }
            foreach (var nf in NoFlys)
            {
                if (IsContained(nf, waypoint)) waypoint = FindOtherWaypoint(obs, start, waypoint);
            }
            return waypoint;
        }

        private static List<Vector3> Navigate(Vector3 start, Vector3 end, float alt)
        {
            frame++;
            if (frame > 5 && frame < 13) Debug.Log(frame + " : " + start + " " + end);
            List<Vector3> waypoints = new List<Vector3>
            {
                start
            };
            var dir = end - start;
            if (dir.magnitude < epsilon) { return waypoints; } // If start = end return start
            // Finds all the buildings sorted by distance from the startpoint in a 200m wide corridor
            MinHeap<MockObstacle> buildings = BlockingBuildings(start, end, alt);
            MinHeap<Vector3> possibilities = new MinHeap<Vector3>((a, b) =>
            {
                // These are normalized projected distance, i.e. how far along the path the waypoint is located
                float mua = Vector3.Dot(a - start, dir) / dir.sqrMagnitude; 
                float mub = Vector3.Dot(b - start, dir) / dir.sqrMagnitude; 
                if (mua <= mub) return -1;
                return 1;
            });

            // To store and flag any deadend waypoints to be redirected, I believe Python has a set() which is equivalent
            // waypoints are hashed, see above for function
            HashSet<int> errorPoints = new HashSet<int>(); 

            bool intersected = false;

            foreach (var obs in NoFlys)
            {
                //For each no fly zone find the number intersects and index of vertices/normals
                if (FindIntersect(obs, start, end, out int[] i) > 0)
                {
                    intersected = true;
                    // Find the corresponding waypoint for the given vertices/normal
                    Vector3 v = FindWaypoint(obs, start, end, i);
                    possibilities.Add(v);
                    if (i[1] == -1) errorPoints.Add(HashVector(v));
                }
            }
            // 5 is arbitrary but should loop through a few in case some buildings overlap
            for (int k = 0; !buildings.IsEmpty() && k < 5; k++) 
            {
                var obs = buildings.Remove();
                int num = FindIntersect(obs, start, end, out int[] j);
                if (num > 0)
                {
                    intersected = true;
                    Vector3 v = FindWaypoint(obs, start, end, j);

                    possibilities.Add(v);
                    if (j[1] == -1) errorPoints.Add(HashVector(v));
                }
            }
            if (intersected)
            {
                var next = possibilities.Remove();

                possibilities.Clear();
                buildings.Clear();
                var list = Navigate(start, next, alt); // pass arguments by value!
                for (int i = 1; i < list.Count; i++)
                {
                    waypoints.Add(list[i]);
                }
                if (errorPoints.Count > 0 && errorPoints.Contains(HashVector(next)))
                {
                    end = _destination;
                }
                list = Navigate(list[list.Count - 1], end, alt); // pass arguments by value!
                for (int i = 1; i < list.Count; i++)
                {
                    waypoints.Add(list[i]);
                }
            }
            else
            {
                waypoints.Add(end);
            }
            return waypoints;

        }
        public static long frame;
    }

}