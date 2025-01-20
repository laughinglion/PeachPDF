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

using PeachPDF.Html.Core.Entities;
using PeachPDF.Html.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PeachPDF.Html.Core.Handlers
{
    /// <summary>
    /// Handler for loading a stylesheet data.
    /// </summary>
    internal static class StylesheetLoadHandler
    {
        /// <summary>
        /// Load stylesheet data from the given source.<br/>
        /// The source can be local file or web URI.<br/>
        /// If the stylesheet is downloaded from URI we will try to correct local URIs to absolute.<br/>
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the source of the element to load the stylesheet by</param>
        /// <param name="attributes">the attributes of the link element</param>
        /// <returns>The loaded stylesheet</returns>>
        public static async Task<string> LoadStylesheet(HtmlContainerInt htmlContainer, string src, Dictionary<string, string> attributes)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            try
            {
                return await LoadStylesheet(htmlContainer, src);
            }
            catch (Exception ex)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Exception in handling stylesheet source", ex);
            }

            return string.Empty;
        }

        /// <summary>
        /// Load stylesheet string from given source (file path or uri).
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the file path or uri to load the stylesheet from</param>
        /// <returns>the stylesheet string</returns>
        private static async Task<string> LoadStylesheet(HtmlContainerInt htmlContainer, string src)
        {
            var baseElement = DomUtils.GetBoxByTagName(htmlContainer.Root, "base");
            var baseUrl = "";

            if (baseElement is not null)
            {
                baseUrl = baseElement.HtmlTag.TryGetAttribute("href", "");
            }

            var baseUri = string.IsNullOrWhiteSpace(baseUrl) ? null : new Uri(baseUrl);
            var href = baseUri is null ? src : new Uri(baseUri, src).AbsoluteUri;

            var uri = CommonUtils.TryGetUri(href);
            
            Stream stream = null;

            if (uri.IsFile)
            {
                var fileInfo = CommonUtils.TryGetFileInfo(uri.AbsoluteUri);

                if (fileInfo.Exists)
                {
                    stream = fileInfo.OpenRead();
                }
            }
            else
            {
                stream = await htmlContainer.Adapter.GetResourceStream(uri);


            }

            if (stream is null)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "No stylesheet found by path: " + src);
                return string.Empty;
            }

            using var sr = new StreamReader(stream);
            return await sr.ReadToEndAsync();
        }
    }
}