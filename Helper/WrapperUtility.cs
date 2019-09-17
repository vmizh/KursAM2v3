using System.Collections.Generic;
using System.Linq;

namespace Helper
{
    public static class WrapperUtility
    {
        public static void AutoWrapper<TIn, TOut>(TIn input, TOut output, ICollection<string> excludedProperties)
            where TIn : class
            where TOut : class
        {
            if ((input == null) || (output == null)) return;
            var inType = input.GetType();
            var outType = output.GetType();
            var props = inType.GetProperties().Where(_ => _.GetAccessors()[0].IsVirtual == false).ToList();
            foreach (var info in props)
            {
                var outfo = (info.CanRead)
                    ? outType.GetProperty(info.Name, info.PropertyType)
                    : null;
                if (outfo == null || !outfo.CanWrite || (outfo.PropertyType != info.PropertyType)) continue;
                if (excludedProperties == null)
                {
                    outfo.SetValue(output, info.GetValue(input, null), null);
                    continue;
                }
                if (!excludedProperties.Contains(info.Name))
                    outfo.SetValue(output, info.GetValue(input, null), null);
            }
        }
    }
}