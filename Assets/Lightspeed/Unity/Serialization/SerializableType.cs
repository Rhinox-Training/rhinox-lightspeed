using System;
using System.IO;
using UnityEngine;

// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)
namespace Rhinox.Utilities
{
    [Serializable]
    public class SerializableType
    {
        [SerializeField] private string _assemblyName;

        [SerializeField] private string _assemblyQualifiedName;

        [SerializeField] private string _name;

        private Type _type;

        public SerializableType(Type t)
        {
            _type = t;
            if (t == null) return;
            
            _name = t.Name;
            _assemblyQualifiedName = t.AssemblyQualifiedName;
            _assemblyName = t.Assembly.FullName;
        }

        public string Name => _name;

        public string AssemblyQualifiedName => _assemblyQualifiedName;

        public string AssemblyName => _assemblyName;

        public Type Type
        {
            get
            {
                if (_type == null)
                    GetSystemType();

                return _type;
            }
        }

        private void GetSystemType()
        {
            _type = Rhinox.Lightspeed.ReflectionUtility.FindTypeExtensively(ref _assemblyQualifiedName);
        }

        public static implicit operator Type(SerializableType x) => x.Type;

        public override string ToString()
        {
            var t = Type;
            return t == null ? "<None>" : t.ToString();
        }

        public override bool Equals(object obj)
        {
            var temp = obj as SerializableType;
            return (object) temp != null && Equals(temp);
        }

        public bool Equals(SerializableType t)
        {
            //return m_AssemblyQualifiedName.Equals(t.m_AssemblyQualifiedName);
            return t.Type == Type;
        }

        public override int GetHashCode()
        {
            if (Type != null) return Type.GetHashCode();
            
            return base.GetHashCode();
        }

        public static bool operator ==(SerializableType a, SerializableType b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }
    }
}