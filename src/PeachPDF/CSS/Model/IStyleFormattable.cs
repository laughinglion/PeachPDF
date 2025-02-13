using System.IO;

namespace PeachPDF.CSS
{
    public interface IStyleFormattable
    {
        void ToCss(TextWriter writer, IStyleFormatter formatter);
    }
}