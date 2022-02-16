#if !UNITY_2019_1_OR_NEWER
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace Rhinox.Utilities
{
    public static class NetworkExtensions
    {
        public static GameObject FindObjectWithID(this NetworkInstanceId objID)
        {
            var idObj = GameObject.FindObjectsOfType<NetworkIdentity>().FirstOrDefault(id => id.netId == objID);

            if (idObj == null) return null;

            return idObj.gameObject;
        }

        public static NetworkInstanceId GetNetID(this Transform obj)
        {
            return obj.GetComponent<NetworkIdentity>().netId;
        }

        public static NetworkInstanceId GetNetID(this GameObject obj)
        {
            return obj.GetComponent<NetworkIdentity>().netId;
        }

        public static NetworkInstanceId GetNetID(this MonoBehaviour obj)
        {
            return obj.GetComponent<NetworkIdentity>().netId;
        }
    }
}
#endif