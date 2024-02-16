using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEngine;

public class TestCenterOnBounds : MonoBehaviour
{
    public Vector3 Target;


    private void OnDrawGizmosSelected()
    {
        var matrix = Gizmos.matrix;

        var bounds = gameObject.GetObjectLocalBounds();
        // Gizmos.matrix = matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.matrix = matrix;  
        
        Vector3 currentCenter = bounds.center;
        var globalCenter = transform.TransformPoint(currentCenter);
        
        var offset = gameObject.GetOffsetToCenterOnBounds(Target);
        
        Gizmos.DrawWireSphere(Target, .1f);
        Gizmos.DrawWireSphere(globalCenter, .1f);
        Gizmos.DrawLine(globalCenter, globalCenter + offset);
    }
}
