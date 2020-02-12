using System;
using System.Reflection;

namespace cworks.DbTest
{
    public static class PropertyInfoExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
                // ReSharper disable  RedundantCast
                return (object) type.GetGenericTypeDefinition() == (object) typeof(Nullable<>);
            return false;
        }
    }
}