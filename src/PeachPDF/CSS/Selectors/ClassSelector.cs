namespace PeachPDF.CSS
{
    public sealed class ClassSelector : SelectorBase
    {
        private ClassSelector(string name) : base(Priority.OneClass, $".{name}")
        {
            Class = name;
        }

        public string Class { get; }

        public static ClassSelector Create(string name)
        {
            return new ClassSelector(name);
        }
    }
}