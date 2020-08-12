using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;
        public Vector3Double(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
    }
}