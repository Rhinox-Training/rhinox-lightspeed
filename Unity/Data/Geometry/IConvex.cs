using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public interface IConvex
    {
        public IList<Vector3> GetAxes();
        
        public IList<Vector3> GetVertices();

    }
}