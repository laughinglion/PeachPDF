﻿namespace PeachPDF.CSS
{
    internal sealed class SrcProperty : Property
    {
        public SrcProperty()
            : base(PropertyNames.Src)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}