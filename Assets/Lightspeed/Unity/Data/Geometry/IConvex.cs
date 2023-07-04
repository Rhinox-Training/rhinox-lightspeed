using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public interface IConvex
    {
        IList<Vector3> GetAxes();
        
        IList<Vector3> GetVertices();

    }
}