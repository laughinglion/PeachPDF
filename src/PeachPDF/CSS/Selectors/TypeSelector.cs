namespace PeachPDF.CSS
{
    public sealed class TypeSelector : SelectorBase
    {
        private TypeSelector(string name) : base(Priority.OneTag, name)
        {
            Name = name;
        }

        public string Name { get; }

        public static TypeSelector Create(string name)
        {
            return new TypeSelector(name);
        }
    }
}