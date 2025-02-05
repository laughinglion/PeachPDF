namespace PeachPDF.CSS
{
    public interface IPageRule : IRule
    {
        string SelectorText { get; set; }
        StyleDeclaration Style { get; }
    }
}