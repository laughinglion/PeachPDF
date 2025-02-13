using System.Collections.Generic;

namespace PeachPDF.CSS
{
    public interface IStylesheetNode : IStyleFormattable
    {
        IEnumerable<IStylesheetNode> Children { get; }
        StylesheetText StylesheetText { get; }
    }
}