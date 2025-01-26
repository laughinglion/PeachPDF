using PeachPDF.PdfSharpCore.Drawing;
using SixLabors.Fonts;
using System.Collections.Generic;

namespace PeachPDF.PdfSharpCore.Internal
{
    public class FontFamilyModel
    {
        public string Name { get; set; }

        public Dictionary<XFontStyle, FontDescription> FontFiles = new();

        public bool IsStyleAvailable(XFontStyle fontStyle)
        {
            return FontFiles.ContainsKey(fontStyle);
        }
    }
}
