using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MattManela.OpenWithTest.OptionPages
{
    public class StringCollectionConvertor : TypeConverter
    {
        private const string Seperator = ",";

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str)) return Enumerable.Empty<string>();

            return new List<string>(str.Split(new[] {Seperator}, StringSplitOptions.RemoveEmptyEntries));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof (string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof (string) && typeof (IEnumerable<string>).IsAssignableFrom(value.GetType()))
            {
                var collection = value as IEnumerable<string>;
                if (collection == null) return "";
                return string.Join(Seperator, collection);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}