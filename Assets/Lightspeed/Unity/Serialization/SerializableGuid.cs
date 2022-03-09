using System;
using System.IO;
using Rhinox.Lightspeed;
using UnityEditor;

namespace Rhinox.Lightspeed
{
    public interface IIdentifiable
    {
        SerializableGuid ID { get; }
    }
    
    [Serializable]
    public class SerializableGuid : IEquatable<SerializableGuid>
    {
        public byte[] SerializedBytes;
        
        public string GuidAsString => SerializedBytes.IsNullOrEmpty() ? "<EMPTY>" : AsSystemGuid().ToString();
        public Guid SystemGuid => AsSystemGuid();
        
        public static SerializableGuid Empty => new SerializableGuid()
        {
            _guidRef = Guid.Empty,
            SerializedBytes = Guid.Empty.ToByteArray()
        };

        private System.Guid _guidRef;

        private SerializableGuid() { }
        
        public SerializableGuid(Guid guid)
        {
            _guidRef = guid;
            SerializedBytes = _guidRef.ToByteArray();
        }

        public static SerializableGuid CreateNew() => new SerializableGuid(Guid.NewGuid());

        private Guid AsSystemGuid()
        {
            if (_guidRef != Guid.Empty)
                return _guidRef;
            
            if (SerializedBytes.IsNullOrEmpty())
                return Guid.Empty;
                
            if (SerializedBytes.Length != 16)
            {
                throw new FormatException(
                    $"Cannot convert to System.Guid, SerializedBytes is of incorrect length (length: {SerializedBytes.Length} - expected: 16)");
            }
                
            _guidRef = new Guid(SerializedBytes);

            return _guidRef;
        }
        
        public static implicit operator Guid(SerializableGuid @this) => @this.AsSystemGuid();
        
        public static bool operator ==(SerializableGuid a, SerializableGuid b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;
        
            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null)
                return false;
        
            return a.Equals(b);
        }
        
        public static bool operator !=(SerializableGuid a, SerializableGuid b)
        {
            return !(a == b);
        }
        
        public static bool operator ==(SerializableGuid a, Guid b)
        {
            Guid a_casted = a?.AsSystemGuid() ?? Guid.Empty;
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a_casted, b))
                return true;
        
            return a_casted.Equals(b);
        }
        
        public static bool operator !=(SerializableGuid a, Guid b)
        {
            return !(a == b);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableGuid) obj);
        }
        
        public bool Equals(SerializableGuid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SystemGuid, other.SystemGuid);
        }

        public override int GetHashCode()
        {
            
            return SystemGuid.GetHashCode();
        }

        public override string ToString()
        {
            return SystemGuid.ToString();
        }
    }
    
    public static partial class Extensions
    {
        public static bool IsNullOrEmpty(this SerializableGuid guid)
        {
            if (guid == null) return true;
            if (guid.SystemGuid == Guid.Empty)
                return true;
            return false;
        }
    }
}