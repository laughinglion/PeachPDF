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

using ExCSS;
using PeachPDF.Html.Adapters;
using PeachPDF.Html.Adapters.Entities;
using PeachPDF.Html.Core.Utils;

namespace PeachPDF.Html.Core.Parse
{
    /// <summary>
    /// Parser to parse CSS stylesheet source string into CSS objects.
    /// </summary>
    internal sealed class CssParser
    {
        #region Fields and Consts

        /// <summary>
        /// 
        /// </summary>
        private readonly RAdapter _adapter;

        /// <summary>
        /// Utility for value parsing.
        /// </summary>
        private readonly CssValueParser _valueParser;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public CssParser(RAdapter adapter)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            _valueParser = new CssValueParser(adapter);
            _adapter = adapter;
        }

        public CssData DefaultCssData => _adapter.DefaultCssData.Clone();

        /// <summary>
        /// Parse the given stylesheet source to CSS blocks dictionary.<br/>
        /// The CSS blocks are organized into two level buckets of media type and class name.<br/>
        /// Root media type are found under 'all' bucket.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the 
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="stylesheet">raw css stylesheet to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the CSS data with parsed CSS objects (never null)</returns>
        public CssData ParseStyleSheet(string stylesheet, bool combineWithDefault)
        {
            var cssData = combineWithDefault ? DefaultCssData : new CssData();
            
            if (!string.IsNullOrEmpty(stylesheet))
            {
                ParseStyleSheet(cssData, stylesheet);
            }

            return cssData;
        }

        public Stylesheet ParseStyleSheet(string stylesheet)
        {
            StylesheetParser parser = new();
            return parser.Parse(stylesheet);
        }

        /// <summary>
        /// Parse the given stylesheet source to CSS blocks dictionary.<br/>
        /// The CSS blocks are organized into two level buckets of media type and class name.<br/>
        /// Root media type are found under 'all' bucket.<br/>
        /// The parsed css blocks are added to the given css data, merged if class name already exists.
        /// </summary>
        /// <param name="cssData">the CSS data to fill with parsed CSS objects</param>
        /// <param name="stylesheet">raw css stylesheet to parse</param>
        public void ParseStyleSheet(CssData cssData, string stylesheet)
        {
            if (!string.IsNullOrEmpty(stylesheet))
            {
                ParseExCssStyle(cssData, stylesheet);
            }
        }

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, red, rgb(255,0,0), rgb(100%, 0, 0) 
        /// </summary>
        /// <param name="colorStr">color string value to parse</param>
        /// <returns>color value</returns>
        public RColor ParseColor(string colorStr)
        {
            return _valueParser.GetActualColor(colorStr);
        }

        public bool IsColorValid(string colorValue)
        {
            return _valueParser.IsColorValid(colorValue);
        }

        private static void ParseExCssStyle(CssData data, string stylesheetText)
        {
            StylesheetParser parser = new();
            var stylesheet = parser.Parse(stylesheetText);
            data.Stylesheets.Add(stylesheet);
        }
    }
}