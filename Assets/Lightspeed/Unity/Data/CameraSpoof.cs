using UnityEngine;

namespace Rhinox.Lightspeed
{
    public class CameraSpoof
    {
        private float _aspectRatio;
        private float _nearClipPlane;
        private float _farClipPlane;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        private float _fov;
        private Vector3 _position;

        private static readonly Vector3 _up = new Vector3(0, 1, 0);
        private static readonly Vector3 _forward = new Vector3(0, 0, 1);
        private static readonly Vector3 _right = new Vector3(1, 0, 0);

        public CameraSpoof()
        {
            _position = Vector3.zero;

            _aspectRatio = 16/9f;
            _nearClipPlane =3;
            _farClipPlane = 100;

            _fov = Mathf.Tan((Mathf.Deg2Rad * 90) / 2f);

            RecalculateViewMatrix();
            CalculateProjectionMatrix();
        }
        
        /// <summary>
        /// Initializes the camera spoof from an existing camera.
        /// </summary>
        /// <param name="cam">The existing camera.</param>
        public CameraSpoof(Camera cam)
        {
            _position = cam.transform.position;

            _aspectRatio = cam.aspect;
            _nearClipPlane = cam.nearClipPlane;
            _farClipPlane = cam.farClipPlane;

            _fov = Mathf.Tan((Mathf.Deg2Rad * cam.fieldOfView) / 2f);

            RecalculateViewMatrix();
            CalculateProjectionMatrix();
        }
        
        /// <summary>
        /// Copies the camera info from an existing camera.
        /// </summary>
        /// <param name="cam"></param>
        public void SetCameraInfo(Camera cam)
        {
            _position = cam.transform.position;

            _aspectRatio = cam.aspect;
            _nearClipPlane = cam.nearClipPlane;
            _farClipPlane = cam.farClipPlane;

            _fov = Mathf.Tan((Mathf.Deg2Rad * cam.fieldOfView) / 2f);

            RecalculateViewMatrix();
            CalculateProjectionMatrix();
        }

        //--------------------------------------------------------------------------------------------------------------
        // Translate the camera info
        //--------------------------------------------------------------------------------------------------------------
        public void TranslateCamera(Vector3 translation)
        {
            _position += translation;
            RecalculateViewMatrix();
        }


        //--------------------------------------------------------------------------------------------------------------
        // Get the camera matrices
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the current view matrix of the camera info.
        /// </summary>
        /// <returns>The view matrix.</returns>
        public Matrix4x4 GetViewMatrix()
        {
            return _viewMatrix;
        }

        /// <summary>
        /// Returns the current projection matrix of the camera info.
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public Matrix4x4 GetProjectionMatrix()
        {
            return _projectionMatrix;
        }

        //--------------------------------------------------------------------------------------------------------------
        // (Re)Calculate the matrices
        //--------------------------------------------------------------------------------------------------------------
        private void RecalculateViewMatrix()
        {
            var right = _right;
            var up = _up;
            var back = -_forward;
            var position = _position;

            _viewMatrix = new Matrix4x4(
                new Vector4(right.x, right.y, right.z, 0),
                new Vector4(up.x, up.y, up.z, 0),
                new Vector4(back.x, -back.y, back.z, 0),
                new Vector4(-position.x, -position.y, position.z, 1));
        }

        private void CalculateProjectionMatrix()
        {
            float clipPlaneDifference = _farClipPlane - _nearClipPlane;
            _projectionMatrix = new Matrix4x4(
                new Vector4(1f / (_aspectRatio * _fov), 0f, 0f, 0f),
                new Vector4(0f, 1f / _fov, 0f, 0f),
                new Vector4(0f, 0f, -(_farClipPlane + _nearClipPlane) / clipPlaneDifference, -1f),
                new Vector4(0f, 0f, -2f * _farClipPlane * _nearClipPlane / clipPlaneDifference, 0f));
        }

        //--------------------------------------------------------------------------------------------------------------
        // Camera functionality
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Transforms a position from world space to screen space.
        /// </summary>
        /// <param name="worldPosition">The position in world space.</param>
        /// <returns>The position in screen space.</returns>
        /// <remarks>
        /// The transformation from world space to screen space involves the following steps:<br/>
        /// 1. Convert the world position to normalized device coordinates using the WorldToViewportPoint method.<br/>
        /// 2. Use the normalized device coordinates to calculate the position on the screen in pixels.<br/>
        ///   - The x and y coordinates are multiplied by the width and height of the screen respectively.<br/>
        ///   - The y coordinate is inverted because the y-axis is flipped in screen space (0 at the top and maximum at the bottom).
        /// </remarks>
        public Vector3 WorldToScreenPoint(Vector3 worldPosition)
        {
            // Get the viewport position
            var normalizedDevicePosition = WorldToViewportPoint(worldPosition); 

            // Resolution of the game view
            var res = Camera.main.pixelRect;

            // Apply screen transform
            float screenX = normalizedDevicePosition.x * res.width;
            float screenY = normalizedDevicePosition.y * res.height;
            return new Vector3(screenX, screenY, normalizedDevicePosition.z);
        }

        /// <summary>
        /// Transforms a position from world space to viewport space.
        /// </summary>
        /// <param name="worldPosition">The position in world space.</param>
        /// <returns>The position in viewport space.</returns>
        /// <remarks>
        /// The transformation from world space to viewport space involves the following steps:<br />
        /// 1. Apply the view matrix: Transform the world position to view space.<br />
        /// 2. Apply the projection matrix: Transform the view space position to clip space.<br />
        /// 3. Perform perspective division: Divide the clip space coordinates by the 'w' component to obtain normalized device coordinates.<br />
        /// 4. Normalized device coordinates are in the range [-1,1]. They are remapped to [0,1] for viewport space.
        /// </remarks>
        public Vector3 WorldToViewportPoint(Vector3 worldPosition)
        {
            //1. Apply the view matrix
            Vector4 viewSpacePosition = _viewMatrix * new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, 1);

            //2. Apply the projection matrix
            Vector4 clipSpacePosition = _projectionMatrix * viewSpacePosition;

            //3. Perform perspective division
            Vector4 ndcPosition = clipSpacePosition / clipSpacePosition.w;

            //4. Values are in [-1,1], get them to [0,1]
            Vector3 aPos = new Vector3(ndcPosition.x, ndcPosition.y, clipSpacePosition.w);

            aPos.x = aPos.x * 0.5f + 0.5f;
            aPos.y = aPos.y * 0.5f + 0.5f;
            return aPos;
        }
    }
}