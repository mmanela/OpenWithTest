using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace MattManela.OpenWithTest.OptionPages
{
    public class StringCollectionEditor : CollectionEditor
    {
        public StringCollectionEditor(Type type)
            : base(typeof(List<String>))
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(String);
        }

        protected override object CreateInstance(Type itemType)
        {
            string newString = String.Empty;
            return newString;
        }

        protected override string GetDisplayText(object value)
        {
            return value.ToString();
        }
    }
}