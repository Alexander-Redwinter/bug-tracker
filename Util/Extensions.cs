using System;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.Util
{
    public static class Extensions
    {
        #region IDictionary Extensions
        //from https://dejanstojanovic.net/aspnet/2018/november/tracking-data-changes-with-entity-framework-core/
        public static IDictionary<TKey, TValue> NullIfEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || !dictionary.Any())
            {
                return null;
            }
            return dictionary;
        }
        #endregion

        #region IEnumerable Extensions
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
            return source;
        }

        #endregion


    }
}
