// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using PeachPDF.Html.Adapters;
using PeachPDF.Html.Adapters.Entities;
using PeachPDF.PdfSharpCore.Drawing;
using PeachPDF.PdfSharpCore.Pdf;
using PeachPDF.PdfSharpCore.Utils;
using PeachPDF.Utilities;
using SixLabors.Fonts;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PeachPDF.Adapters
{
    /// <summary>
    /// Adapter for PdfSharp library platform.
    /// </summary>
    internal sealed class PdfSharpAdapter : RAdapter
    {
        /// <summary>
        /// Init color resolve.
        /// </summary>
        private PdfSharpAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            var fonts = FontResolver.SupportedFonts;
            
            var fontCollection = new FontCollection();
            fontCollection.AddSystemFonts();

            foreach(var fontPath in fonts)
            {
                var font = fontCollection.Add(fontPath);
                AddFontFamily(new FontFamilyAdapter(new XFontFamily(font.Name)));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static PdfSharpAdapter Instance { get; } = new();

        public override string GetCssMediaType(IEnumerable<string> mediaTypesAvailable)
        {
            return mediaTypesAvailable.Contains("print") ? "print" : "all";
        }

        protected override RColor GetColorInt(string colorName)
        {
            return System.Enum.TryParse(typeof(KnownColor), colorName, true, out var knownColor)
                ? Utils.Convert(Color.FromKnownColor((KnownColor)knownColor))
                : RColor.Empty;
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new XPen(Utils.Convert(color)));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            XBrush solidBrush;
            if (color == RColor.White)
                solidBrush = XBrushes.White;
            else if (color == RColor.Black)
                solidBrush = XBrushes.Black;
            else if (color.A < 1)
                solidBrush = XBrushes.Transparent;
            else
                solidBrush = new XSolidBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush);
        }

        protected override RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            var mode = angle switch
            {
                < 45 => XLinearGradientMode.ForwardDiagonal,
                < 90 => XLinearGradientMode.Vertical,
                < 135 => XLinearGradientMode.BackwardDiagonal,
                _ => XLinearGradientMode.Horizontal
            };

            return new BrushAdapter(new XLinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), mode));
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((XImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(XImage.FromStream(() => memoryStream));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            var xFont = new XFont(family, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            var xFont = new XFont(((FontFamilyAdapter)family).FontFamily.Name, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }
    }
}