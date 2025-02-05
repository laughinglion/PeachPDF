using System.Collections.Generic;

namespace PeachPDF.CSS
{
    public interface IRuleList : IEnumerable<IRule>
    {
        IRule this[int index] { get; }
        int Length { get; }
    }
}