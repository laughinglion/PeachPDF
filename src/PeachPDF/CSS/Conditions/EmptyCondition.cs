using System.IO;

namespace PeachPDF.CSS
{
    internal sealed class EmptyCondition : StylesheetNode, IConditionFunction
    {
        public bool Check()
        {
            return true;
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
        }
    }
}