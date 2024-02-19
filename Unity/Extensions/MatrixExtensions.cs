using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class MatrixExtensions
    {
        public static Matrix4x4 GetMatrixRelativeTo(this Matrix4x4 worldMatrix, Matrix4x4 parentMatrix)
        {
            return parentMatrix.inverse * worldMatrix;
        }
        
        public static Matrix4x4 GetMatrixRelativeTo(this Component component, Matrix4x4 parentMatrix)
        {
            return parentMatrix.inverse * GetWorldMatrix(component);
        }
        
        public static Matrix4x4 GetMatrixRelativeTo(this GameObject gameObject, Matrix4x4 parentMatrix)
        {
            return parentMatrix.inverse * GetWorldMatrix(gameObject);
        }
		
        public static Matrix4x4 GetWorldMatrix(this Component component)
        {
            return component.transform.localToWorldMatrix;
        }
        
        public static Matrix4x4 GetWorldMatrix(this GameObject gameObject)
        {
            return gameObject.transform.localToWorldMatrix;
        }
        
        public static Matrix4x4 GetLocalTransform(this Component component)
        {
            var t = component.transform;
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
        
        public static Matrix4x4 GetLocalTransform(this GameObject gameObject)
        {
            var t = gameObject.transform;
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }

    public static partial class Utility
    {
        public static Vector3 GetMatrixPosition(Matrix4x4 m)
        {
            return new Vector3(m[0, 3], m[1, 3], m[2, 3]);
        }
 
        public static Vector3 GetMatrixScale(Matrix4x4 m)
        {
            return new Vector3
                (m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }
 
        public static Quaternion GetMatrixRotation(Matrix4x4 m)
        {
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m.m00 + m.m11 + m.m22)) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m.m00 - m.m11 - m.m22)) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m.m00 + m.m11 - m.m22)) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m.m00 - m.m11 + m.m22)) / 2;
            q.x = _copysign(q.x, m.m21 - m.m12);
            q.y = _copysign(q.y, m.m02 - m.m20);
            q.z = _copysign(q.z, m.m10 - m.m01);
            return q;
        }
        
        private static float _copysign(float sizeval, float signval)
        {
            return Mathf.Sign(signval) == 1 ? Mathf.Abs(sizeval) : -Mathf.Abs(sizeval);
        }
    }
}