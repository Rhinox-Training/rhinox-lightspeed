using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public enum ShaderParameterType
    {
        None,
        Float,
        Color,
        Vector,
        Texture
    }
    
    public static class ShaderUtility
    {
        public static string[] GetShaderPropertyList(Shader shader, ShaderParameterType type = ShaderParameterType.None)
        {
#if UNITY_EDITOR
            return GetShaderPropertyList(shader, GetValidTypes(type));
#endif
            // TODO add warning about runtime use?
            return Array.Empty<string>();
        }
        
#if UNITY_EDITOR
        private static string[] GetShaderPropertyList(Shader shader, IList<ShaderUtil.ShaderPropertyType> filterTypes = null)
        {
            if (shader == null) return Array.Empty<string>();
            
            List<string> results = new List<string>();

            int count = ShaderUtil.GetPropertyCount(shader);
            results.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                bool isHidden = ShaderUtil.IsShaderPropertyHidden(shader, i);
                var propertyType = ShaderUtil.GetPropertyType(shader, i);
                bool isValidPropertyType = filterTypes == null || filterTypes.Contains(propertyType);
                if (!isHidden && isValidPropertyType)
                {
                    var name = ShaderUtil.GetPropertyName(shader, i);
                    results.Add(name);
                }
            }

            results.Sort();
            return results.ToArray();
        }

        public static ShaderUtil.ShaderPropertyType[] GetValidTypes(ShaderParameterType type)
        {
            switch (type)
            {
                case ShaderParameterType.Float:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Float,
                        ShaderUtil.ShaderPropertyType.Range
                    };
                case ShaderParameterType.Color:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Color,
                    };
                case ShaderParameterType.Vector:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Vector
                    };
                case ShaderParameterType.Texture:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.TexEnv
                    };
            }

            return null;
        }
#endif

    }
}
