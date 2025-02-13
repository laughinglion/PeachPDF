﻿namespace PeachPDF.CSS
{
    internal sealed class WordBreakProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.WordBreakConverter;

        public WordBreakProperty()
            : base(PropertyNames.WordBreak)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}