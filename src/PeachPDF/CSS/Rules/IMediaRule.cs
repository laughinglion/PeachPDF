namespace PeachPDF.CSS
{
    public interface IMediaRule : IConditionRule
    {
        MediaList Media { get; }
    }
}