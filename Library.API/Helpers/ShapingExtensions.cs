using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Library.API.Helpers
{
    public static class ShapingExtensions
    {
        public static ExpandoObject ShapeData<T>(this T source, string fields)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrEmpty(fields))
            {
                var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsSplit = fields.Split(",");

                foreach (var field in fieldsSplit)
                {
                    var propertyName = field.Trim();
                    var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                        throw new Exception("There is no such property");

                    propertyInfoList.Add(propertyInfo);
                }
            }

            var shapedObj = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(source);

                ((IDictionary<string, object>)shapedObj).Add(propertyInfo.Name, propertyValue);
            }
            
            return shapedObj;
        }
    }
}
