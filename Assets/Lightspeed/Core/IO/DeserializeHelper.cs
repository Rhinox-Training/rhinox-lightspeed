using System;

namespace Rhinox.Lightspeed
{
    public static class DeserializeHelper
    {
        public static object TryConvert(string value)
        {
            if (string.IsNullOrEmpty(value))
                return (string.Empty);

            // NOTE: decimal gets parsed even if not a decimal (e.g. int)
            //decimal decimalValue = 0;
            //if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out decimalValue))
            //{
            //    return (decimalValue);
            //}

            double doubleValue = 0;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out doubleValue))
            {
                return (doubleValue);
            }

            float floatValue = 0;
            if (float.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out floatValue))
            {
                return (floatValue);
            }

            long longValue = 0;
            if (long.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out longValue))
                return (longValue);

            int intValue = 0;
            if (int.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out intValue))
                return (intValue);

            bool boolValue = false;
            if (bool.TryParse(value, out boolValue))
                return (boolValue);

            DateTime dateTimeValue = DateTime.MinValue;
            if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dateTimeValue))
                return (dateTimeValue);

            return (value);
        }
    }
}