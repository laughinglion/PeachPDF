namespace PeachPDF.CSS
{
    internal sealed class ContainerNameProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.StringConverter.OrDefault();

        internal ContainerNameProperty()
            : base(PropertyNames.ContainerName)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}