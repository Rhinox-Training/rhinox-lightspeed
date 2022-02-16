using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEngine.Events;

namespace Rhinox.Utilities
{
    public static class UnityEventHelper
    {
        public class UnityEventListenerInfo
        {
            public UnityEngine.Object Target;
            public MethodInfo Method;
            public PersistentListenerMode Mode = PersistentListenerMode.EventDefined;
            public UnityEventCallState CallState = UnityEventCallState.RuntimeOnly;
            public object Argument;
        }

        private static bool _initialized;

        private static FieldInfo _persistentCallsField;
        private static FieldInfo _groupCallsField;
        
        private static FieldInfo _callTarget;
        private static FieldInfo _callMethod;
        private static FieldInfo _callMode;
        private static FieldInfo _callState;
        private static FieldInfo _callArgument;
        private static FieldInfo _objectTypeName;

        private static Dictionary<Type, FieldInfo> _argumentsFieldByType;
        
        public static List<UnityEventListenerInfo> GetPersistentListeners(this UnityEventBase e)
        {
            // We can do without reflection for the target and method but we need it to get the parameter
            FetchFields();
            // Type == PersistentCallGroup [Internal]
            var persistentCalls = _persistentCallsField.GetValue(e);
            var callsList = _groupCallsField.GetValue(persistentCalls) as IEnumerable<object>;

            var list = new List<UnityEventListenerInfo>();
            foreach (var call in callsList)
            {
                var target = (UnityEngine.Object) _callTarget.GetValue(call);
                var methodName = (string) _callMethod.GetValue(call);
                var argumentCache = _callArgument.GetValue(call);
                var mode = (PersistentListenerMode) _callMode.GetValue(call);
                var argument = GetArgument(e, argumentCache, mode, out Type argumentType);

                var allMethods = target.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == methodName)
                    .ToArray();

                MethodInfo info = null;
                foreach (var mi in allMethods)
                {
                    var methodParams = mi.GetParameters();
                    if (methodParams.Any() && argumentType == null) continue;
                    if (methodParams.Length == 0 && argumentType == null)
                    {
                        info = mi;
                        break;
                    }
                    if (methodParams.Length > 1) continue;
                    if (methodParams.Length == 1 && methodParams[0].ParameterType == argumentType)
                    {
                        info = mi;
                        break;
                    }
                }
                
                list.Add(new UnityEventListenerInfo
                {
                    CallState = (UnityEventCallState) _callState.GetValue(call),
                    Target = target,
                    Argument = argument,
                    Mode = mode,
                    Method = info,
                });
            }

            return list;
        }

        private static void FetchFields()
        {
            if (_initialized) return;

            var t = typeof(UnityEventBase);
            
            // m_PersistentCalls == PersistentCallGroup
            _persistentCallsField = t.GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic);

            // Type == PersistentCallGroup [Internal]
            t = _persistentCallsField.FieldType;
            
            // m_Calls == List<PersistentCall>
            _groupCallsField = t.GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
            
            // Type = PersistentCall [Internal]
            t = _groupCallsField.FieldType.GenericTypeArguments[0];
            
            _callTarget     = t.GetField("m_Target", BindingFlags.Instance | BindingFlags.NonPublic);
            _callMethod     = t.GetField("m_MethodName", BindingFlags.Instance | BindingFlags.NonPublic);
            _callMode       = t.GetField("m_Mode", BindingFlags.Instance | BindingFlags.NonPublic);
            _callState      = t.GetField("m_CallState", BindingFlags.Instance | BindingFlags.NonPublic);
            _callArgument   = t.GetField("m_Arguments", BindingFlags.Instance | BindingFlags.NonPublic);
            
            // Type = ArgumentCache [Internal]
            t = _callArgument.FieldType;
            _objectTypeName = t.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Instance | BindingFlags.NonPublic);

            _initialized = true;
        }

        private static object GetArgument(UnityEventBase e, object argumentCache, PersistentListenerMode mode, out Type argumentType)
        {
            argumentType = null;
            if (argumentCache == null) return null;
            string argumentName = null;

            switch (mode)
            {
                case PersistentListenerMode.EventDefined:
                    // If UnityEvent<> get its Type
                    argumentType = e.GetType().GenericTypeArguments.FirstOrDefault();
                    return null;
                case PersistentListenerMode.Void:
                    return null;
                case PersistentListenerMode.Object:
                    argumentName = "m_ObjectArgument";
                    break;
                case PersistentListenerMode.Int:
                    argumentName = "m_IntArgument";
                    break;
                case PersistentListenerMode.Float:
                    argumentName = "m_FloatArgument";
                    break;
                case PersistentListenerMode.String:
                    argumentName = "m_StringArgument";
                    break;
                case PersistentListenerMode.Bool:
                    argumentName = "m_BoolArgument";
                    break;
            }
            
            var memberInfo = argumentCache.GetType().GetField(argumentName, BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (memberInfo.FieldType == typeof(UnityEngine.Object))
            {
                // Unity Object is saved generically, get the type
                var assemblyTypeName = (string) _objectTypeName.GetValue(argumentCache);
                argumentType = ReflectionUtility.FindTypeExtensively(ref assemblyTypeName);
            }
            else
                argumentType = memberInfo.FieldType;

            return memberInfo.GetValue(argumentCache);;
        }
    }
}