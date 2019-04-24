using System;

namespace Drones.Serializable
{
    [Serializable]
    public class StaticObstacle
    {
        public SVector3 position;
        public SVector3 size;
        public SVector3 orientation;
        public float diag;
        public SVector3 dz;
        public SVector3 dx;
        public float mu;
        public SVector3[] normals;
        public SVector3[] verts;
    }
}

