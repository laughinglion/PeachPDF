
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
using System.IO;

namespace PeachPDF.Html.Core.Utils
{
    /// <summary>
    /// Static class that contains argument-checking methods
    /// </summary>
    public static class ArgChecker
    {
        /// <summary>
        /// Validate given argument isn't <see cref="System.IntPtr.Zero"/>.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is <see cref="System.IntPtr.Zero"/></exception>
        public static void AssertArgNotNull(IntPtr arg, string argName)
        {
            if (arg == IntPtr.Zero)
            {
                throw new ArgumentException("IntPtr argument cannot be Zero", argName);
            }
        }

        /// <summary>
        /// Validate given argument isn't Null or empty.
        /// </summary>
        /// <param name="arg">argument to validate</param>
        /// <param name="argName">Name of the argument checked</param>
        /// <exception cref="System.ArgumentNullException">if <paramref name="arg"/> is Null or empty</exception>
        public static void AssertArgNotNullOrEmpty(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentNullException(argName);
            }
        }
    }
}