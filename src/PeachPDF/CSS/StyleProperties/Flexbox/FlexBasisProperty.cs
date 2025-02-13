﻿namespace PeachPDF.CSS
{
    internal sealed class FlexBasisProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.FlexBasisConverter;

        internal FlexBasisProperty()
            : base(PropertyNames.FlexBasis)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}