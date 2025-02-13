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

using PeachPDF.CSS;
using PeachPDF.Html.Adapters;
using PeachPDF.Html.Core.Dom;
using PeachPDF.Html.Core.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachPDF.Html.Core
{
    /// <summary>
    /// Holds parsed stylesheet css blocks arranged by media and classes.<br/>
    /// </summary>
    /// <remarks>
    /// To learn more about CSS blocks visit CSS spec: http://www.w3.org/TR/CSS21/syndata.html#block
    /// </remarks>
    public sealed class CssData
    {
        public List<Stylesheet> Stylesheets { get; } = [];

        /// <summary>
        /// Init.
        /// </summary>
        internal CssData()
        {
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the 
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="adapter">Platform adapter</param>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static async Task<CssData> Parse(RAdapter adapter, string stylesheet, bool combineWithDefault = true)
        {
            var parser = new CssParser(adapter, null);
            return await parser.ParseStyleSheet(stylesheet, combineWithDefault);
        }

        internal IEnumerable<IStyleRule> GetStyleRules(string media, CssBox box)
        {
            foreach (var stylesheet in Stylesheets)
            {
                foreach (var rule in GetStyleRules(stylesheet.StyleRules, box))
                {
                    yield return rule;
                }

                foreach (var mediaRule in stylesheet.MediaRules)
                {
                    foreach (var medium in mediaRule.Media)
                    {
                        if (medium.Type != media) continue;

                        foreach (var rule in GetStyleRules(mediaRule.Rules.OfType<IStyleRule>(), box))
                        {
                            yield return rule;
                        }
                    }
                }

            }
        }

        private static IEnumerable<IStyleRule> GetStyleRules(IEnumerable<IStyleRule> styleRules, CssBox box)
        {
            return styleRules.Where(rule => DoesSelectorMatch(rule.Selector, box));
        }

        private static bool DoesSelectorMatch(ISelector selector, CssBox box)
        {
            return selector switch
            {
                AllSelector => true,
                ListSelector listSelector => DoesSelectorMatch(listSelector, box),
                TypeSelector typeSelector => DoesSelectorMatch(typeSelector, box),
                ComplexSelector complexSelector => DoesSelectorMatch(complexSelector, box),
                CompoundSelector compoundSelector => DoesSelectorMatch(compoundSelector, box),
                PseudoElementSelector => false,
                PseudoClassSelector pseudoClassSelector => DoesSelectorMatch(pseudoClassSelector, box),
                AttrMatchSelector attrMatchSelector => DoesSelectorMatch(attrMatchSelector, box),
                ClassSelector classSelector => DoesSelectorMatch(classSelector, box),
                IdSelector idSelector => DoesSelectorMatch(idSelector, box),
                AttrAvailableSelector attrAvailableSelector => DoesSelectorMatch(attrAvailableSelector, box),
                AttrContainsSelector attrContainsSelector => DoesSelectorMatch(attrContainsSelector, box),
                AttrListSelector attrListSelector => DoesSelectorMatch(attrListSelector, box),
                _ => false
            };
        }
        private static bool DoesSelectorMatch(ListSelector listSelector, CssBox box)
        {
            return listSelector.Any(selector => DoesSelectorMatch(selector, box));
        }

        private static bool DoesSelectorMatch(CompoundSelector compoundSelector, CssBox box)
        {
            return compoundSelector.All(selector => DoesSelectorMatch(selector, box));
        }

        private static bool DoesSelectorMatch(TypeSelector typeSelector, CssBox box)
        {
            return box.HtmlTag is not null && typeSelector.Name.Equals(box.HtmlTag.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool DoesSelectorMatch(ClassSelector classSelector, CssBox box)
        {
            if (box.HtmlTag is not null && box.HtmlTag.Attributes.TryGetValue("class", out var classNames))
            {
                return classNames.Split(' ').Any(className =>
                    className.Equals(classSelector.Class, StringComparison.InvariantCultureIgnoreCase));
            }

            return false;
        }

        private static bool DoesSelectorMatch(IdSelector idSelector, CssBox box)
        {
            if (box.HtmlTag is not null && box.HtmlTag.Attributes.TryGetValue("id", out var id))
            {
                return id.Equals(idSelector.Id, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrAvailableSelector attrAvailableSelector, CssBox box)
        {
            if (box.HtmlTag is null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrAvailableSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrMatchSelector attrMatchSelector, CssBox box)
        {
            if (box.HtmlTag is null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrMatchSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return attribute.Value.Equals(attrMatchSelector.Value, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrListSelector attrListSelector, CssBox box)
        {
            if (box.HtmlTag is null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrListSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    var attributeValues = attribute.Value.Split(' ');

                    return attributeValues.Any(value => value.Equals(attrListSelector.Value, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrContainsSelector attrContainsSelector, CssBox box)
        {
            if (box.HtmlTag is null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrContainsSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return attribute.Value.Contains(attrContainsSelector.Value, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(PseudoClassSelector pseudoClassSelector, CssBox box)
        {
            return pseudoClassSelector.Class == "link" && box.IsClickable;
        }

        private static bool DoesSelectorMatch(ComplexSelector complexSelector, CssBox box)
        {
            var currentLevel = box;
            var selectorsInReverse = complexSelector.Reverse();

            var isLowestItem = true;
            var isMatch = false;

            foreach (var selector in selectorsInReverse)
            {
                if (selector.Selector is not null)
                {
                    if (isLowestItem)
                    {
                        isMatch = DoesSelectorMatch(selector.Selector, currentLevel);

                        if (!isMatch)
                        {
                            return false;
                        }

                        isLowestItem = false;
                        currentLevel = box.ParentBox;
                        continue;
                    }

                    if (currentLevel is null)
                    {
                        return false;
                    }

                    switch (selector.Delimiter)
                    {
                        case ">":

                            isMatch = DoesSelectorMatch(selector.Selector, currentLevel);

                            if (!isMatch)
                            {
                                return false;
                            }

                            break;
                        case " " or null:
                        {
                            do
                            {
                                isMatch = DoesSelectorMatch(selector.Selector, currentLevel);

                                if (!isMatch)
                                {
                                    currentLevel = currentLevel.ParentBox;
                                }

                            } while (!isMatch && currentLevel is not null);

                            if (!isMatch)
                            {
                                return false;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return isMatch;
        }

        public CssData Clone()
        {
            CssData cssData = new();
            cssData.Stylesheets.AddRange(Stylesheets);
            return cssData;
        }
    }
}