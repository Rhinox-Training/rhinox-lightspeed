using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [Serializable]
    public class SerializablePropertyInfo : SerializableMemberInfo
    {
        [SerializeField]     
        private SerializableType _fieldType;

        [SerializeField]     
        private MemberTypes _memberTypes;

        [SerializeField]     
        private string _fieldName;

        [SerializeField]     
        private bool _canRead;
        
        [SerializeField]     
        private bool _canWrite;
        
        [SerializeField]     
        private bool _isPublic;
        
        [SerializeField]     
        private bool _isStatic;

        public override MemberTypes MemberType => _memberTypes;
        public override string Name => _fieldName;
        
        public SerializablePropertyInfo(PropertyInfo p) : base(p.DeclaringType)
        {
            _fieldType = new SerializableType(p.PropertyType);
            _fieldName = p.Name;
            _memberTypes = p.MemberType;
            _canRead = p.CanRead;
            _canWrite = p.CanWrite;
            _isPublic = p.IsPublic();
            _isStatic = p.IsStatic();
        }
        
        public Type DeclaringType => _declaringType.Type;
        public Type PropertyType => _fieldType.Type;
        public bool IsPublic => _isPublic;
        public bool IsStatic => _isStatic;

        private PropertyInfo _propertyInfo;
        public PropertyInfo PropertyInfo
        {
            get
            {
                if (_propertyInfo == null)
                    GetPropertyInfo();

                return _propertyInfo;
            }
        }
        
        private void GetPropertyInfo()
        {
            BindingFlags bf = BindingFlags.Default;
            bf |= (IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);
            bf |= (IsStatic ? BindingFlags.Static : BindingFlags.Instance);
            
            _propertyInfo = DeclaringType.GetProperty(Name, bf);
        }

        public static implicit operator PropertyInfo(SerializablePropertyInfo x) => x.PropertyInfo;
        
        public override MemberInfo GetMemberInfo() => PropertyInfo;

        public override bool Equals(object obj)
        {
            SerializablePropertyInfo temp;
            if (obj is PropertyInfo propertyInfo)
                temp = new SerializablePropertyInfo(propertyInfo);
            else
                temp = obj as SerializablePropertyInfo;
            return (object) temp != null && Equals(temp);
        }
        
        public bool Equals(PropertyInfo _Object)
        {
            return _Object.Equals(PropertyInfo);
        }

        public bool Equals(SerializablePropertyInfo _Object)
        {
            return _Object.PropertyInfo.Equals(PropertyInfo);
        }

        public override int GetHashCode()
        {
            return PropertyInfo.GetHashCode();
        }

        public static bool operator ==(SerializablePropertyInfo a, SerializablePropertyInfo b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;
        
            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null)
                return false;
        
            return a.Equals(b);
        }
        
        public static bool operator !=(SerializablePropertyInfo a, SerializablePropertyInfo b)
        {
            return !(a == b);
        }
    }
}