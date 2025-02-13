namespace PeachPDF.CSS
{
    public interface IProperty : IStylesheetNode
    {
        string Name { get; }
        string Value { get; }
        string Original { get; }
        bool IsImportant { get; }
    }
}