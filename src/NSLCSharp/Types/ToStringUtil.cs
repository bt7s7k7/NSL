using System;
using System.Globalization;
#if UNITY_2020
using UnityEngine;
#else
using System.Text.Json;
#endif

namespace NSL.Types
{
    public static class ToStringUtil
    {
        public static string ToString(object target)
        {
            var culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            if (target == null) return "null";

            string ret = null;
            if (target.GetType().GetMethod("ToString", Type.EmptyTypes).DeclaringType != typeof(object)) ret = target.ToString();
            if (ret == null)
            {
#if UNITY_2020
                var json = JsonUtility.ToJson(target);
#else
                var json = JsonSerializer.Serialize(target);
#endif
                ret = target.GetType().Name + json;
            }

            CultureInfo.CurrentCulture = culture;
            return ret;
        }
    }
}