using System;
using System.Reflection;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [Serializable]
    public class SerializableFieldInfo : SerializableMemberInfo
    {
        [SerializeField]     
        private SerializableType _fieldType;

        [SerializeField]     
        private MemberTypes _memberTypes;

        [SerializeField]     
        private string _fieldName;

        [SerializeField]     
        private bool _isPublic;
        
        [SerializeField]     
        private bool _isStatic;

        public override MemberTypes MemberType => _memberTypes;
        public override string Name => _fieldName;


        public SerializableFieldInfo(FieldInfo f) : base(f.DeclaringType)
        {
            _fieldType = new SerializableType(f.FieldType);
            _fieldName = f.Name;
            _memberTypes = f.MemberType;
            _isPublic = f.IsPublic;
            _isStatic = f.IsStatic;
        }
        
        public Type DeclaringType => _declaringType.Type;
        public Type FieldType => _fieldType.Type;
        public bool IsPublic => _isPublic;
        public bool IsStatic => _isStatic;

        private FieldInfo _fieldInfo;
        public FieldInfo FieldInfo
        {
            get
            {
                if (_fieldInfo == null)
                    GetFieldInfo();

                return _fieldInfo;
            }
        }
        
        private void GetFieldInfo()
        {
            BindingFlags bf = BindingFlags.Default;
            bf |= (IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);
            bf |= (IsStatic ? BindingFlags.Static : BindingFlags.Instance);
            
            _fieldInfo = DeclaringType.GetField(Name, bf);
        }

        public static implicit operator FieldInfo(SerializableFieldInfo x) => x.FieldInfo;
        
        public override MemberInfo GetMemberInfo() => FieldInfo;

        public override bool Equals(object obj)
        {
            SerializableFieldInfo temp;
            if (obj is FieldInfo fieldInfo)
                temp = new SerializableFieldInfo(fieldInfo);
            else
                temp = obj as SerializableFieldInfo;
            return (object) temp != null && Equals(temp);
        }
        
        public bool Equals(FieldInfo _Object)
        {
            return _Object.Equals(FieldInfo);
        }

        public bool Equals(SerializableFieldInfo _Object)
        {
            return _Object.FieldInfo.Equals(FieldInfo);
        }

        public override int GetHashCode()
        {
            return FieldInfo.GetHashCode();
        }

        public static bool operator ==(SerializableFieldInfo a, SerializableFieldInfo b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;
        
            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null)
                return false;
        
            return a.Equals(b);
        }
        
        public static bool operator !=(SerializableFieldInfo a, SerializableFieldInfo b)
        {
            return !(a == b);
        }
    }
}