// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using PeachPDF.Html.Core;

namespace PeachPDF.Html.Adapters.Entities
{
    /// <summary>
    /// Even class for handling keyboard events in <see cref="HtmlContainerInt"/>.
    /// </summary>
    public sealed class RMouseEvent(bool leftButton)
    {
        /// <summary>
        /// Is the left mouse button participated in the event
        /// </summary>
        public bool LeftButton { get; } = leftButton;
    }
}