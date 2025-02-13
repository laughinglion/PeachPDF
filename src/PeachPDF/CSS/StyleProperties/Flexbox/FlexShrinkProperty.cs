﻿namespace PeachPDF.CSS
{
    internal sealed class FlexShrinkProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.FlexGrowShrinkConverter;

        internal FlexShrinkProperty()
            : base(PropertyNames.FlexShrink)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
