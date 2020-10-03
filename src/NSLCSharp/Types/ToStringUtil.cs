using System;
using System.Globalization;
using System.Text.Json;

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
            if (target.GetType().GetMethod("ToString", Type.EmptyTypes)!.DeclaringType != typeof(object)) ret = target.ToString();
            if (ret == null)
            {
                ret = target.GetType().Name + JsonSerializer.Serialize(target);
            }

            CultureInfo.CurrentCulture = culture;
            return ret;
        }
    }
}