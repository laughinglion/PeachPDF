namespace PeachPDF.CSS
{
    public interface IConditionFunction : IStylesheetNode
    {
        bool Check();
    }
}