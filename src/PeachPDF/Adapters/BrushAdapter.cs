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
using PeachPDF.PdfSharpCore.Drawing;

namespace PeachPDF.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter(XBrush brush) : RBrush
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public XBrush Brush => brush;

        public override void Dispose()
        { }
    }
}