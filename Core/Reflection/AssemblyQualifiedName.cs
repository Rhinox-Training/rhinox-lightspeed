using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rhinox.Lightspeed.Reflection
{
    public class AssemblyQualifiedName
    {
        public class TypeInfo
        {
            public string AssemblyName;
            public string FullTypeName;

            public IReadOnlyCollection<TypeInfo> GenericArguments => _arguments;
            private List<TypeInfo> _arguments;
            private readonly string _suffix;
            public bool IsGeneric => GenericArguments != null && GenericArguments.Count > 0;

            public TypeInfo(string assemblyQualifiedString)
            {
                AssemblyQualifiedName.TryParse(assemblyQualifiedString, out FullTypeName, out AssemblyName,
                    out _suffix);
                
                _arguments = new List<TypeInfo>();
            }

            public bool AddGenericArgument(TypeInfo typeInfo)
            {
                if (typeInfo == null)
                    return false;
                return _arguments.AddUnique(typeInfo);
            }

            public void Validate()
            {
                string searchTypeName = FullTypeName;
                string suffix = $"`{_arguments.Count}";
                if (_arguments.Count > 0 && !searchTypeName.EndsWith(suffix))
                    searchTypeName += suffix;
                Type t = ReflectionUtility.FindTypeExtensively(ref AssemblyName, ref searchTypeName);
                if (searchTypeName.EndsWith(suffix))
                    FullTypeName = searchTypeName.Substring(0, searchTypeName.Length - suffix.Length);
                else
                    FullTypeName = searchTypeName;
                foreach (var child in _arguments)
                {
                    child.Validate();
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(FullTypeName);
                if (_arguments.Count > 0)
                {
                    sb.Append($"`{_arguments.Count}[");
                    for (int i = 0; i < _arguments.Count; ++i)
                    {
                        var arg = _arguments[i];
                        sb.Append("[");
                        sb.Append(arg.ToString());
                        sb.Append("]");
                        if (i < _arguments.Count - 1)
                            sb.Append(", ");
                    }

                    sb.Append("]");
                }
                sb.Append(", ");
                sb.Append(AssemblyName);
                sb.Append(_suffix);
                return sb.ToString();
            }
        }

        public TypeInfo Info { get; private set; }

        public AssemblyQualifiedName(string assemblyQualifiedName)
        {
            Parse(assemblyQualifiedName);
        }

        public AssemblyQualifiedName(Type t)
            : this(t.AssemblyQualifiedName)
        {
        }

        private class ElementHandler
        {
            private readonly StringBuilder _builder;
            private List<ElementHandler> _children;

            public ElementHandler Parent { get; private set; }
            public int ChildCount => _children.Count;
            public IReadOnlyCollection<ElementHandler> Children => _children;

            public ElementHandler()
            {
                _builder = new StringBuilder();
                _children = new List<ElementHandler>();
            }

            public void Append(char c)
            {
                _builder.Append(c);
            }

            public void Append(string s)
            {
                _builder.Append(s);
            }

            public void AddChild(ElementHandler handler)
            {
                if (handler == this || handler == null)
                    throw new ArgumentException(nameof(handler));

                handler.Parent = this;
                _children.AddUnique(handler);
            }

            public override string ToString()
            {
                return _builder.ToString();
            }
        }

        private bool Parse(string assemblyQualifiedName)
        {
            ElementHandler rootHandler = new ElementHandler();
            ElementHandler currentHandler = rootHandler;
            for (int i = 0; i < assemblyQualifiedName.Length; ++i)
            {
                char currentChar = assemblyQualifiedName[i];
                if (currentChar == '`')
                {
                    var childHandler = new ElementHandler();
                    currentHandler.AddChild(childHandler);
                    currentHandler = childHandler;

                    int indexOfEnd = assemblyQualifiedName.IndexOf("[[", i, StringComparison.InvariantCulture);
                    i = indexOfEnd + 1;
                }
                else if (currentChar == ']' && assemblyQualifiedName[i - 1] != '[')
                {
                    char nextChar = assemblyQualifiedName[i + 1];
                    if (nextChar == ']')
                    {
                        // Popping back up a level
                        currentHandler = currentHandler.Parent;
                        i = i + 1;
                    }
                    else if (nextChar == ',')
                    {
                        var childHandler = new ElementHandler();
                        currentHandler.Parent.AddChild(childHandler);
                        currentHandler = childHandler;

                        i = assemblyQualifiedName.IndexOf('[', i + 1);
                    }
                }
                else
                {
                    currentHandler.Append(currentChar);
                }
            }

            var entries = new Stack<ElementHandler>();
            var dict = new Dictionary<ElementHandler, TypeInfo>();
            entries.Push(rootHandler);
            while (entries.Count > 0)
            {
                var current = entries.Pop();

                var typeInfo = new TypeInfo(current.ToString());
                dict.Add(current, typeInfo);

                if (current.Parent != null && dict.ContainsKey(current.Parent))
                {
                    dict[current.Parent].AddGenericArgument(typeInfo);
                }

                if (current.ChildCount > 0)
                {
                    foreach (var child in current.Children)
                        entries.Push(child);
                }
            }

            Info = dict[rootHandler];
            
            return true;
        }

        public void ValidateAndUpdate()
        {
            Info.Validate();
        }

        public override string ToString()
        {
            return Info.ToString();
        }

        private static Regex _qualifiedNameRegex = new Regex(@"([a-zA-Z]{0}[a-zA-Z0-9.]*)((?:\[\])*),\s([a-zA-Z0-9.]+)(,\sVersion=[0-9]\.[0-9]\.[0-9]\.[0-9],\sCulture=[a-zA-Z\-]+,\sPublicKeyToken=[a-zA-Z0-9]+)*"); 
        
        public static bool TryParse(string assemblyQualifiedName, out string typeName, out string assemblyName)
        {
            return TryParse(assemblyQualifiedName, out typeName, out assemblyName, out _);
        }

        private static bool TryParse(string assemblyQualifiedName, out string typeName, out string assemblyName, out string remainder)
        {
            var match = _qualifiedNameRegex.Match(assemblyQualifiedName);
            if (!match.Success)
            {
                typeName = null;
                assemblyName = null;
                remainder = null;
                return false;
            }
            typeName = match.Groups[1].Value + match.Groups[2].Value;
            assemblyName = match.Groups[3].Value;
            remainder = match.Groups[4].Value ?? "";
            return true;
        }

        public static string TrimVersion(string assemblyQualifiedName)
        {
            if (TryParse(assemblyQualifiedName, out string type, out string assembly, out string _))
                return $"{type}, {assembly}";
            return assemblyQualifiedName;
        }
    }
}