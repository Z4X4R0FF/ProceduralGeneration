using System;
using Unity.Properties.Editor;
using Unity.Serialization;

namespace Unity.Build
{
    internal static class TypeConstructionHelper
    {
        public static T ConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName)
        {
            var type = Type.GetType(assemblyQualifiedTypeName);
            if (null == type && FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentTypeName))
            {
                type = Type.GetType(currentTypeName);
            }
            return TypeConstruction.Construct<T>(type);
        }

        public static bool TryConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName, out T value)
        {
            var type = Type.GetType(assemblyQualifiedTypeName);
            if (null == type && FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentTypeName))
            {
                type = Type.GetType(currentTypeName);
            }
            return TypeConstruction.TryConstruct(type, out value);
        }
    }
}
