namespace PeachPDF.CSS
{
    public interface ISupportsRule : IConditionRule
    {
        IConditionFunction Condition { get; }
    }
}