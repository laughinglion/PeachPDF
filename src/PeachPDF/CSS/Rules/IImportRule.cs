namespace PeachPDF.CSS
{
    public interface IImportRule : IRule
    {
        string Href { get; set; }
        MediaList Media { get; }
    }
}