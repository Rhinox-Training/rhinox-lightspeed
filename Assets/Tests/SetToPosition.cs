using UnityEngine;
using Rhinox.Lightspeed;

namespace Tests
{
    public class SetToPosition : MonoBehaviour
    {
        public Vector3 Pos;
        public Vector3 Euler;

        [ContextMenu("Set")]
        public void SetPosition()
        {
            var quat = Quaternion.Euler(Euler);
            gameObject.CenterObjectBoundsOnPosition(Pos, quat);
        }
    }
}