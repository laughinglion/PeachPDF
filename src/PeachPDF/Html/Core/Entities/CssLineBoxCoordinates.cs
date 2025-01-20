using PeachPDF.Html.Core.Dom;

namespace PeachPDF.Html.Core.Entities
{
    internal record CssLineBoxCoordinates
    {
        public required CssLineBox Line { get; set; }

        public required double CurrentX { get; set; }

        public required double CurrentY { get; set; }

        public required double MaxRight { get; set; }

        public required double MaxBottom { get; set; }
    }
}
