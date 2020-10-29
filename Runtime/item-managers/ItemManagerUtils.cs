using BeatThat.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace BeatThat.ItemManagers
{
    /// <summary>
    /// Interface to expose a managed collection of items.
    /// </summary>
    public static class ItemManagerUtils
    {
        public delegate bool MapOne<FromType, ToType>(FromType item, out ToType result);

        public static bool ExtractWithCheckForComponentSibling<FromType, ToType>(FromType item, out ToType result)
            where ToType : class
        {
            if (item == null)
            {
                result = null;
                return false;
            }
            result = item as ToType;
            if (result != null)
            {
                return true;
            }
            var c = item as Component;
            if (c == null)
            {
                result = null;
                return false;
            }
            result = c.GetComponent<ToType>();
            return result != null;
        }

        public static int GetItems<FromType, ToType>(
            ICollection<FromType> sourceItems,
            ICollection<ToType> resultItems,
            MapOne<FromType, ToType> extractValue)
        {
            int n = 0;
            ToType tmp;
            foreach (var i in sourceItems)
            {
                if(extractValue(i, out tmp)) {
                    resultItems.Add(tmp);
                    n++;
                }
            }
            return n;
        }

        public static IEnumerable<ToType> GetItems<FromType, ToType>(
            IEnumerable<FromType> items,
            MapOne<FromType, ToType> extractValue)
        {
            ToType tmp;
            foreach (var i in items)
            {
                if (extractValue(i, out tmp))
                {
                    yield return tmp;
                }
            }
        }
    }
}

