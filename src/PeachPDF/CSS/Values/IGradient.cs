using System.Collections.Generic;

namespace PeachPDF.CSS
{
    public interface IGradient : IImageSource
    {
        IEnumerable<GradientStop> Stops { get; }
        bool IsRepeating { get; }
    }
}