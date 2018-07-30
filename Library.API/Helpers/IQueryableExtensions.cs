using Library.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (mappingDictionary == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrEmpty(orderBy))
                return source;

            var orderBySplit = orderBy.Split(",");

            foreach (var orderByClause in orderBySplit.Reverse())
            {
                var trimmed = orderByClause.Trim();

                var orderDesc = trimmed.EndsWith(" desc");

                var indexOfFirstSpace = trimmed.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmed : trimmed.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                    throw new ArgumentException("Key mapping is missing");

                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                    throw new ArgumentNullException("sdgffgsdgf");

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                        orderDesc = !orderDesc;

                    source = source.OrderBy(destinationProperty + (orderDesc ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}
