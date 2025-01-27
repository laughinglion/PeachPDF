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

using System;

namespace PeachPDF.Html.Adapters.Entities
{
    /// <summary>
    ///     Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public record struct RPoint(double X, double Y)
    {
        /// <summary>
        ///     Represents a new instance of the <see cref="RPoint" /> class with member data left uninitialized.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public static readonly RPoint Empty = new RPoint();

        static RPoint()
        { }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="RPoint" /> is empty.
        /// </summary>
        /// <returns>
        ///     true if both <see cref="RPoint.X" /> and
        ///     <see
        ///         cref="RPoint.Y" />
        ///     are 0; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public bool IsEmpty
        {
            get
            {
                if (Math.Abs(X - 0.0) < 0.001)
                    return Math.Abs(Y - 0.0) < 0.001;
                else
                    return false;
            }
        }

        /// <summary>
        ///     Translates a given <see cref="RPoint" /> by a specified
        ///     <see
        ///         cref="T:System.Drawing.SizeF" />
        ///     .
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to add to the coordinates of
        ///     <paramref
        ///         name="pt" />
        ///     .
        /// </param>
        public static RPoint Add(RPoint pt, RSize sz)
        {
            return new RPoint(pt.X + sz.Width, pt.Y + sz.Height);
        }

        /// <summary>
        ///     Translates a <see cref="RPoint" /> by the negative of a specified size.
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to subtract from the coordinates of
        ///     <paramref
        ///         name="pt" />
        ///     .
        /// </param>
        public static RPoint Subtract(RPoint pt, RSize sz)
        {
            return new RPoint(pt.X - sz.Width, pt.Y - sz.Height);
        }
    }
}