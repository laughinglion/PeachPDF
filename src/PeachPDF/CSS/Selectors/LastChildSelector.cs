namespace PeachPDF.CSS
{
    public sealed class LastChildSelector : ChildSelector
    {
        public LastChildSelector()
            : base(PseudoClassNames.NthLastChild)
        {
        }
    }
}