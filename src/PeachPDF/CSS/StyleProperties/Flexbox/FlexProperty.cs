﻿namespace PeachPDF.CSS
{
    internal sealed class FlexProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.FlexConverter;

        internal FlexProperty()
            : base(PropertyNames.Flex)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
