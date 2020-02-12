using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace cworks.DbTest
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Returns the first column of values as an array of primitives
        /// </summary>
        /// <param name="dt"></param>
        /// <typeparam name="T">string, int, float, etc</typeparam>
        /// <returns></returns>
        public static T[] ToScalarArray<T>(this DataTable dt)
        {
            var items = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add((T) Convert.ChangeType(row[0],typeof(T)));
            }

            return items.ToArray();
        }
        public static T[] ToArray<T>(this DataTable dt) where T : new()
        {
            var models = new List<T>();
            var props = typeof(T).GetProperties();
            foreach (DataRow row in dt.Rows)
            {
                var model = new T();
            
                MapRowToModel(row, dt, model, props);
                models.Add(model);
            }

            return models.ToArray();
        }

        public static T GetField<T>(this DataRow row, string name)
        {
            if (row[name] == DBNull.Value)
            {
                return default(T);
            }

            var isNullable = typeof(T).IsNullableType();
                
                
            if (typeof(T).IsEnum )
            {
                var intValue =Convert.ToInt32(row[name]);
                return (T) (object) intValue;
            }
            // ReSharper disable once PossibleNullReferenceException
            if (isNullable && Nullable.GetUnderlyingType(typeof(T)).IsEnum)
            {
                var enumType = Nullable.GetUnderlyingType(typeof(T));
                var intValue =Convert.ToInt32(row[name]);
                var enumValue = Enum.ToObject( enumType ?? throw new ArgumentException(), intValue);
                return (T) enumValue;
            }

            return (T) row[name];
        }
    
        private static void MapRowToModel(DataRow row, DataTable dt, object model, PropertyInfo[] props)
        {
            foreach (DataColumn col in dt.Columns)
            {
                try
                {
                    var pi = GetPropertyInfo(props, col);
                    if (pi == null) continue;
                    var propertyValue = ToPropertyValue(row[col], pi.PropertyType);
                    if (propertyValue != null)
                    {
                        pi.SetValue(model,propertyValue);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static object ToPropertyValue(object o, Type type)
        {
            if (o is null) return null;

            if (type.IsNullableType())
            {
                type = Nullable.GetUnderlyingType(type);

            }

            if (type.IsEnum)
            {
                if (o is string)
                {
                    return Enum.Parse(type, o.ToString());
                }

                return Enum.ToObject(type, o);
            }
            return Convert.ChangeType(o, type);
        }

        private static PropertyInfo GetPropertyInfo(PropertyInfo[] props, DataColumn col)
        {
            var pi = props.FirstOrDefault(i => i.Name.Equals(col.ColumnName, StringComparison.InvariantCultureIgnoreCase));
            return pi;
        }

        public static bool HasModelProperties<T>(this DataTable dt)
        {
            return !dt.GetMissingModelProperties<T>().Any();
        }
    
        public static PropertyInfo[] GetMissingModelProperties<T>(this DataTable dt)
        {

            var missingProperties = new List<PropertyInfo>();
            var props = typeof(T).GetProperties();
            var columns = dt.Columns.Cast<DataColumn>().ToArray();
            foreach (var pi in props.Where(i=>i.GetCustomAttribute<NotMappedAttribute>()==null))
            {
                var col = columns.FirstOrDefault(i => i.ColumnName.Equals(pi.Name, StringComparison.InvariantCultureIgnoreCase));
                if (col == null) missingProperties.Add(pi);
            }

            return missingProperties.ToArray();
        }
    }
}