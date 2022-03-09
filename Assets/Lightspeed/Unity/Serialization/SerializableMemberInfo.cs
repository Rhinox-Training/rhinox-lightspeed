using System;
using System.Reflection;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [Serializable]
    public abstract class SerializableMemberInfo
    {
        [SerializeField]        
        protected SerializableType _declaringType;
        
        public abstract string Name { get; }
        public abstract MemberTypes MemberType { get; }

        protected SerializableMemberInfo(Type declaringType)
        {
            _declaringType = new SerializableType(declaringType);
        }

        public abstract MemberInfo GetMemberInfo();
    }
}