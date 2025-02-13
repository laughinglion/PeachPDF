namespace PeachPDF.CSS
{
    public struct GradientStop
    {
        public GradientStop(Color color, Length location)
        {
            Color = color;
            Location = location;
        }

        public Color Color { get; }
        public Length Location { get; }
    }
}