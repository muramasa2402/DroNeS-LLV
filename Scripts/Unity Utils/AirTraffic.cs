using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Drones.Routing
{
    public struct StaticObstacle
    {
        public Vector3 position;
        public Vector3 size;
        public Vector3 orientation;
        public float diag;
        public Vector3 dz;
        public Vector3 dx;
        public Vector2 munu;
    }

    public class AirTraffic
    {
        static readonly List<List<StaticObstacle>> Buildings = new List<List<StaticObstacle>>();
        static readonly HashSet<StaticObstacle> NoFlys = new HashSet<StaticObstacle>();
        static readonly float maxAlt = 450;
        static readonly float minAlt = 150;
        static readonly int altDiv = 10;
        static readonly int buildingDiv = 30;
        static readonly int radiusOfConcern = 100;
        static readonly int droneRadius = 1;
        static readonly int hubAlt = 500;
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

        public void GetBuildings(StaticObstacle[] o)
        {
            int added = 0;
            int i = 0;
            while (added < o.Length)
            {
                Buildings.Add(new List<StaticObstacle>());
                for (int j = 0; j < o.Length; j++)
                {
                    if (o[j].size.y < (i + 1) * buildingDiv && o[j].size.y >= i * buildingDiv)
                    {
                        o[j].position.y = 0;
                        o[j].dx = RotationY(o[j].orientation.y) * Vector3.right * o[j].size.x / 2;
                        o[j].dz = RotationY(o[j].orientation.y) * Vector3.forward * o[j].size.z / 2;
                        o[j].diag = new Vector2(o[j].size.x, o[j].size.z).magnitude;
                        Buildings[i].Add(o[j]);
                        added++;
                    }
                }
                i++;
            }
        } //Initialized presimulation

        public void GetGameState(int drones, int[,] completed, List<StaticObstacle> noflys)
        {
            droneCount = drones;
            foreach (var nf in noflys)
            {
                NoFlys.Add(nf);
            }

            for (int i = 0; i < _assigned.Length; i++)
            {
                _assigned[completed[i,0]] -= completed[i,1];
            }

        }

        public int ChooseAltitude()
        {
            float max = 0;
            int maxindex = _assigned.Length - 1;
            for (int i = 0; i < _assigned.Length; i++)
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

        public Matrix4x4 RotationY(float theta)
        {
            theta /= 180 * Mathf.PI;

            return new Matrix4x4(new Vector4(Mathf.Cos(theta), 0, -Mathf.Sin(theta)),
                new Vector4(0, 1, 0), new Vector4(Mathf.Sin(theta), 0, Mathf.Cos(theta)), new Vector4(0, 0, 0, 1));
        }

        private Vector3 GetPerp(ref Vector3 o, ref Vector3 d)
        {
            Vector4 tmp1 = (d - o).normalized;
            tmp1.w = 0;
            Vector4 tmp2 = RotationY(90) * tmp1;
            tmp2.w = 0;
            tmp2 = tmp2.normalized;
            return tmp2;
        }

        private List<StaticObstacle> GetPossibleObstacles(Vector3 origin, Vector3 dest, bool backToHub = false)
        {
            List<StaticObstacle> obstacles = new List<StaticObstacle>();
            origin.y = 0;
            dest.y = 0;
            Vector3 hmu = dest - origin; // origin to dest this is NOT NORMALISED SO 0 < MU < 1
            Vector3 hnu = GetPerp(ref origin, ref dest); // THIS IS NORMALISED, SO NU IS ACTUAL DISTANCE

            float alt = backToHub ? hubAlt : Altitudes[ChooseAltitude()];

            int buildingbase = (int)(alt / buildingDiv); // i.e. the building list index where we should start

            for (int i = buildingbase; i < Buildings.Count; i++)
            {
                for (int j = 0; j < Buildings[i].Count; j++)
                {
                    var building = Buildings[i][j];
                    if (building.size.y < alt - altDiv / 2) continue; // Building is below altitude
                    Vector2 munu = new Vector2
                    {
                        x = Vector3.Dot(building.position - origin, hmu) / hmu.sqrMagnitude,
                        y = Vector3.Dot(origin - building.position, hnu) / hnu.sqrMagnitude
                    };

                    if (munu.y < radiusOfConcern && munu.x <= 1 && munu.x >= 0)
                    {
                        building.munu = munu;
                        obstacles.Add(building);
                    }

                }
            }

            obstacles.Sort((StaticObstacle a, StaticObstacle b) =>
            {
                if (a.munu.x <= b.munu.x) { return -1; }
                return 1;
            });
            return obstacles;
        }

        private void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private void GetNormalsAndVerts(StaticObstacle obj, out Vector3[] norms, out Vector3[] verts)
        {
            norms = new Vector3[4];
            verts = new Vector3[4];
            norms[0] = obj.dz.normalized;
            norms[1] = obj.dx.normalized;
            norms[2] = -norms[0];
            norms[3] = -norms[1];

            verts[0] = obj.position + obj.dz + obj.dx; // ij
            verts[1] = obj.position - obj.dz + obj.dx; // jk
            verts[2] = obj.position - obj.dz - obj.dx; // kl
            verts[3] = obj.position + obj.dz - obj.dx; // li
        }

        private List<Vector3> GetWaypoints(Vector3 origin, Vector3 dest, bool backToHub = false)
        {
            List<Vector3> waypoints = new List<Vector3>
            {
                origin
            };

            Vector3 h = dest - origin;
            Vector3 first;
            Vector3 last;
            Vector3 prevfirst = new Vector3();
            Vector3 prevlast = new Vector3();

            Vector3 f(float mu) => origin + mu * h;

            int addHistory = 0;

            foreach (var obj in GetPossibleObstacles(origin, dest, backToHub))
            {
                float[] mu = new float[4];
                int[] J = new int[2]; // store the surface index, 2 is expected number of intersects

                GetNormalsAndVerts(obj, out Vector3[] n, out Vector3[] v);

                int NumberOfIntersects = 0;

                for (int j = 0; j < mu.Length; j++)
                {
                    float tmp = Vector3.Dot(h.normalized, n[j]);
                    if (tmp > 0.001f) 
                    {
                        mu[j] = Vector3.Dot(v[j] - origin, n[j]) / Vector3.Dot(h, n[j]);
                        if ((f(mu[j]) - obj.position).magnitude < obj.diag)
                        {
                            J[NumberOfIntersects++] = j;
                        }
                    }

                    if (NumberOfIntersects == 2 || (NumberOfIntersects == 0 && j == 2)) { break; }
                }

                if (NumberOfIntersects != 0)
                {
                    if (mu[J[1]] < mu[J[0]]) Swap(ref J[0], ref J[1]); // Make sure first intersection is the closer one

                    first = f(mu[J[0]]) - droneRadius * h.normalized;
                    last = f(mu[J[1]]) + droneRadius * h.normalized;

                    if (waypoints.Count > 1 && Vector3.Dot(first - prevlast, h) < 0) // Makes sure that drone doesn't reverse
                    {
                        if (Vector3.Distance(last, first) > Vector3.Distance(prevfirst, prevlast))
                        {
                            // the previous building is contained within the current building
                            waypoints.RemoveRange(waypoints.Count - addHistory, addHistory);
                        }
                        else
                        {
                            continue; // the current building is contained within the previous building
                        }
                    }

                    waypoints.Add(first);
                    if (J[1] - J[0] < 2)
                    {
                        //adjacent face
                        waypoints.Add(v[J[0]] + droneRadius * (n[J[0]] + n[J[1]]));
                        addHistory = 3;
                    }
                    else
                    {
                        //opposite face
                        waypoints.Add(v[J[0]] + droneRadius * (n[J[0]] + n[J[1] - 1]));
                        waypoints.Add(v[J[0]] + droneRadius * (n[J[1] - 1] + n[J[1]]));
                        addHistory = 4;
                    }
                    waypoints.Add(last);
                    prevfirst = first;
                    prevlast = last;
                }
            }

            if (Vector3.Dot(dest - waypoints[waypoints.Count - 1], h) > 0) waypoints.Add(dest);

            return waypoints;
        }

        public List<Vector3> GoAroundNoFlys(Vector3 origin, Vector3 dest, bool backToHub = false)
        {
            List<Vector3> waypoints = new List<Vector3>
            {
                origin
            };
            Vector3 h = dest - origin;
            Vector3 f(float mu) => origin + mu * h;

            foreach (var obj in NoFlys)
            {
                float[] mu = new float[4];
                int[] J = new int[2]; // Expected number of intersects
                GetNormalsAndVerts(obj, out Vector3[] n, out Vector3[] v);
                int NumberOfIntersects = 0;

                for (int j = 0; j < mu.Length; j++)
                {
                    float tmp = Vector3.Dot(h.normalized, n[j]);
                    if (tmp > 0.001f)
                    {
                        mu[j] = Vector3.Dot(v[j] - origin, n[j]) / Vector3.Dot(h, n[j]);
                        if ((f(mu[j]) - obj.position).magnitude < obj.diag)
                        {
                            J[NumberOfIntersects++] = j;
                        }
                    }

                    if (NumberOfIntersects == 2 || (NumberOfIntersects == 0 && j == 2)) { break; }
                }

                if (NumberOfIntersects != 0)
                {
                    if (mu[J[1]] < mu[J[0]]) // Make sure first intersection is the closer one
                    {
                        int tmp = J[0];
                        J[0] = J[1];
                        J[1] = tmp;
                    }
                    var o = origin;
                    var d = f(mu[J[0]]) - radiusOfConcern * h.normalized;
                    var partial = GetWaypoints(o, d, backToHub);
                    waypoints = waypoints.Concat(partial).ToList();
                    if (J[1] - J[0] < 2)
                    {
                        //adjacent face
                        o = waypoints[waypoints.Count - 1];
                        d = v[J[0]] + radiusOfConcern * (n[J[0]] + n[J[1]]);
                        partial = GetWaypoints(o, d, backToHub);
                        waypoints = waypoints.Concat(partial).ToList();
                    }
                    else
                    {
                        //opposite face
                        o = waypoints[waypoints.Count - 1];
                        d = v[J[0]] + radiusOfConcern * (n[J[0]] + n[J[1] - 1]);
                        partial = GetWaypoints(o, d, backToHub);
                        waypoints = waypoints.Concat(partial).ToList();
                        o = waypoints[waypoints.Count - 1];
                        d = v[J[0]] + radiusOfConcern * (n[J[1] - 1] + n[J[1]]);
                        partial = GetWaypoints(o, d, backToHub);
                        waypoints = waypoints.Concat(partial).ToList();

                    }
                    o = waypoints[waypoints.Count - 1];
                    d = f(mu[J[1]]) + radiusOfConcern * h.normalized;
                    partial = GetWaypoints(o, d, backToHub);
                    waypoints = waypoints.Concat(partial).ToList();
                }

            }

            return waypoints;

        }

    }

}