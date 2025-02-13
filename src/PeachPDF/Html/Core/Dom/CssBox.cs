﻿// "Therefore those skilled at the unorthodox
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

using PeachPDF.Html.Adapters;
using PeachPDF.Html.Adapters.Entities;
using PeachPDF.Html.Core.Entities;
using PeachPDF.Html.Core.Handlers;
using PeachPDF.Html.Core.Parse;
using PeachPDF.Html.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PeachPDF.Html.Core.Dom
{
    /// <summary>
    /// Represents a CSS Box of text or replaced elements.
    /// </summary>
    /// <remarks>
    /// The Box can contains other boxes, that's the way that the CSS Tree
    /// is composed.
    /// 
    /// To know more about boxes visit CSS spec:
    /// http://www.w3.org/TR/CSS21/box.html
    /// </remarks>
    internal class CssBox : CssBoxProperties, IDisposable
    {
        #region Fields and Consts

        private readonly uint _id = 0;

        private static uint _idCounter = 0;

        /// <summary>
        /// the parent css box of this css box in the hierarchy
        /// </summary>
        private CssBox _parentBox;

        /// <summary>
        /// the root container for the hierarchy
        /// </summary>
        protected HtmlContainerInt _htmlContainer;

        /// <summary>
        /// the inner text of the box
        /// </summary>
        private string _text;

        /// <summary>
        /// Do not use or alter this flag
        /// </summary>
        /// <remarks>
        /// Flag that indicates that CssTable algorithm already made fixes on it.
        /// </remarks>
        internal bool _tableFixed;

        protected bool _wordsSizeMeasured;
        private CssBox _listItemBox;

        /// <summary>
        /// handler for loading background image
        /// </summary>
        private ImageLoadHandler _imageLoadHandler;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parentBox">optional: the parent of this css box in html</param>
        /// <param name="tag">optional: the html tag associated with this css box</param>
        public CssBox(CssBox parentBox, HtmlTag tag)
        {
            if (parentBox != null)
            {
                _parentBox = parentBox;
                _parentBox.Boxes.Add(this);
            }

            _id = ++_idCounter;
            HtmlTag = tag;
        }

        /// <summary>
        /// Gets the HtmlContainer of the Box.
        /// WARNING: May be null.
        /// </summary>
        public HtmlContainerInt HtmlContainer
        {
            get { return _htmlContainer ??= _parentBox?.HtmlContainer; }
            set => _htmlContainer = value;
        }

        /// <summary>
        /// Gets or sets the parent box of this box
        /// </summary>
        public CssBox ParentBox
        {
            get => _parentBox;
            set
            {
                //Remove from last parent
                _parentBox?.Boxes.Remove(this);

                _parentBox = value;

                //Add to new parent
                if (value != null)
                    _parentBox.Boxes.Add(this);
            }
        }

        /// <summary>
        /// Gets the children boxes of this box
        /// </summary>
        public List<CssBox> Boxes { get; } = [];

        /// <summary>
        /// Is the box is of "br" element.
        /// </summary>
        public bool IsBrElement => HtmlTag != null && HtmlTag.Name.Equals("br", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// is the box "Display" is "Inline", is this is an inline box and not block.
        /// </summary>
        public bool IsInline => Display is CssConstants.Inline or CssConstants.InlineBlock && !IsBrElement;

        /// <summary>
        /// is the box "Display" is "Block", is this is a block box and not inline.
        /// </summary>
        public bool IsBlock => Display == CssConstants.Block;

        public bool IsFloated => Float is CssConstants.Left or CssConstants.Right;

        /// <summary>
        /// Is the css box clickable (by default only "a" element is clickable)
        /// </summary>
        public virtual bool IsClickable => HtmlTag is { Name: HtmlConstants.A } && !HtmlTag.HasAttribute("id") && !HtmlTag.HasAttribute("name");

        /// <summary>
        /// Gets a value indicating whether this instance or one of its parents has Position = fixed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fixed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsFixed
        {
            get
            {
                if (Position == CssConstants.Fixed)
                    return true;

                if (this.ParentBox == null)
                    return false;

                CssBox parent = this;

                while (!(parent.ParentBox == null || parent == parent.ParentBox))
                {
                    parent = parent.ParentBox;

                    if (parent.Position == CssConstants.Fixed)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Get the href link of the box (by default get "href" attribute)
        /// </summary>
        public virtual string HrefLink => GetAttribute(HtmlConstants.Href);

        /// <summary>
        /// Gets the containing block-box of this box. (The nearest parent box with display=block)
        /// </summary>
        public CssBox ContainingBlock
        {
            get
            {
                if (ParentBox == null)
                {
                    return this; //This is the initial containing block.
                }

                var box = ParentBox;
                while (!box.IsBlock &&
                       box.Display != CssConstants.ListItem &&
                       box.Display != CssConstants.Table &&
                       box.Display != CssConstants.TableCell &&
                       box.ParentBox != null)
                {
                    box = box.ParentBox;
                }

                //Comment this following line to treat always superior box as block
                if (box == null)
                    throw new Exception("There's no containing block on the chain");

                return box;
            }
        }

        /// <summary>
        /// Gets the HTMLTag that hosts this box
        /// </summary>
        public HtmlTag HtmlTag { get; }

        /// <summary>
        /// Gets if this box represents an image
        /// </summary>
        public bool IsImage => Words.Count == 1 && Words[0].IsImage;

        /// <summary>
        /// Tells if the box is empty or contains just blank spaces
        /// </summary>
        public bool IsSpaceOrEmpty
        {
            get
            {
                if ((Words.Count != 0 || Boxes.Count != 0) && (Words.Count != 1 || !Words[0].IsSpaces))
                {
                    foreach (CssRect word in Words)
                    {
                        if (!word.IsSpaces)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the inner text of the box
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = HtmlUtils.FixNewLines(value);
                Words.Clear();
            }
        }

        /// <summary>
        /// Gets the line-boxes of this box (if block box)
        /// </summary>
        internal List<CssLineBox> LineBoxes { get; } = [];

        /// <summary>
        /// Gets the rectangles where this box should be painted
        /// </summary>
        internal Dictionary<CssLineBox, RRect> Rectangles { get; } = [];

        /// <summary>
        /// Gets the BoxWords of text in the box
        /// </summary>
        internal List<CssRect> Words { get; } = [];

        /// <summary>
        /// Gets the first word of the box
        /// </summary>
        internal CssRect FirstWord => Words[0];

        /// <summary>
        /// Gets or sets the first linebox where content of this box appear
        /// </summary>
        internal CssLineBox FirstHostingLineBox { get; set; }

        /// <summary>
        /// Gets or sets the last linebox where content of this box appear
        /// </summary>
        internal CssLineBox LastHostingLineBox { get; set; }

        /// <summary>
        /// Create new css box for the given parent with the given html tag.<br/>
        /// </summary>
        /// <param name="tag">the html tag to define the box</param>
        /// <param name="parent">the box to add the new box to it as child</param>
        /// <returns>the new box</returns>
        public static CssBox CreateBox(HtmlTag tag, CssBox parent = null)
        {
            ArgumentNullException.ThrowIfNull(tag);

            return tag.Name switch
            {
                HtmlConstants.Img => new CssBoxImage(parent, tag),
                HtmlConstants.Iframe => new CssBoxFrame(parent, tag),
                HtmlConstants.Hr => new CssBoxHr(parent, tag),
                _ => new CssBox(parent, tag)
            };
        }

        /// <summary>
        /// Create new css box for the given parent with the given optional html tag and insert it either
        /// at the end or before the given optional box.<br/>
        /// If no html tag is given the box will be anonymous.<br/>
        /// If no before box is given the new box will be added at the end of parent boxes collection.<br/>
        /// If before box doesn't exists in parent box exception is thrown.<br/>
        /// </summary>
        /// <remarks>
        /// To learn more about anonymous inline boxes visit: http://www.w3.org/TR/CSS21/visuren.html#anonymous
        /// </remarks>
        /// <param name="parent">the box to add the new box to it as child</param>
        /// <param name="tag">optional: the html tag to define the box</param>
        /// <param name="before">optional: to insert as specific location in parent box</param>
        /// <returns>the new box</returns>
        public static CssBox CreateBox(CssBox parent, HtmlTag tag = null, CssBox before = null)
        {
            ArgumentNullException.ThrowIfNull(parent);

            var newBox = new CssBox(parent, tag);
            newBox.InheritStyle();
            if (before != null)
            {
                newBox.SetBeforeBox(before);
            }
            return newBox;
        }

        /// <summary>
        /// Create new css block box.
        /// </summary>
        /// <returns>the new block box</returns>
        public static CssBox CreateBlock()
        {
            return new CssBox(null, null)
            {
                Display = CssConstants.Block
            };
        }

        /// <summary>
        /// Create new css block box for the given parent with the given optional html tag and insert it either
        /// at the end or before the given optional box.<br/>
        /// If no html tag is given the box will be anonymous.<br/>
        /// If no before box is given the new box will be added at the end of parent boxes collection.<br/>
        /// If before box doesn't exists in parent box exception is thrown.<br/>
        /// </summary>
        /// <remarks>
        /// To learn more about anonymous block boxes visit CSS spec:
        /// http://www.w3.org/TR/CSS21/visuren.html#anonymous-block-level
        /// </remarks>
        /// <param name="parent">the box to add the new block box to it as child</param>
        /// <param name="tag">optional: the html tag to define the box</param>
        /// <param name="before">optional: to insert as specific location in parent box</param>
        /// <returns>the new block box</returns>
        public static CssBox CreateBlock(CssBox parent, HtmlTag tag = null, CssBox before = null)
        {
            ArgumentNullException.ThrowIfNull(parent);

            var newBox = CreateBox(parent, tag, before);
            newBox.Display = CssConstants.Block;
            return newBox;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.
        /// </summary>
        /// <param name="g">Device context to use</param>
        public async ValueTask PerformLayout(RGraphics g)
        {
            try
            {
                await PerformLayoutImp(g);
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Layout, "Exception in box layout", ex);
            }
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">Device context to use</param>
        public async ValueTask Paint(RGraphics g)
        {
#if DEBUG
            Console.WriteLine($"paint: {ToString()}");
#endif

            try
            {
                if (Display == CssConstants.None || Visibility != CssConstants.Visible) return;

                // use initial clip to draw blocks with Position = fixed. I.e. ignore page margins
                if (Position == CssConstants.Fixed)
                {
                    g.SuspendClipping();
                }

                // don't call paint if the rectangle of the box is not in visible rectangle
                var visible = Rectangles.Count == 0;
                if (!visible)
                {
                    var clip = g.GetClip();
                    var rect = ContainingBlock.ClientRectangle;
                    rect.X -= 2;
                    rect.Width += 2;
                    if (!IsFixed)
                    {
                        //rect.Offset(new RPoint(-HtmlContainer.Location.X, -HtmlContainer.Location.Y));
                        rect.Offset(HtmlContainer.ScrollOffset);
                    }
                    clip.Intersect(rect);

                    if (clip != RRect.Empty)
                        visible = true;
                }

                if (visible)
                    await PaintImp(g);

                // Restore clips
                if (Position == CssConstants.Fixed)
                {
                    g.ResumeClipping();
                }
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Paint, "Exception in box paint", ex);
            }
        }

        /// <summary>
        /// Set this box in 
        /// </summary>
        /// <param name="before"></param>
        public void SetBeforeBox(CssBox before)
        {
            int index = _parentBox.Boxes.IndexOf(before);
            if (index < 0)
                throw new Exception("before box doesn't exist on parent");

            _parentBox.Boxes.Remove(this);
            _parentBox.Boxes.Insert(index, this);
        }

        /// <summary>
        /// Move all child boxes from <paramref name="fromBox"/> to this box.
        /// </summary>
        /// <param name="fromBox">the box to move all its child boxes from</param>
        public void SetAllBoxes(CssBox fromBox)
        {
            foreach (var childBox in fromBox.Boxes)
                childBox._parentBox = this;

            Boxes.AddRange(fromBox.Boxes);
            fromBox.Boxes.Clear();
        }

        /// <summary>
        /// Splits the text into words and saves the result
        /// </summary>
        public void ParseToWords()
        {
            Words.Clear();

            var startIdx = 0;
            var preserveSpaces = WhiteSpace is CssConstants.Pre or CssConstants.PreWrap;
            var respectNewLines = preserveSpaces || WhiteSpace == CssConstants.PreLine;

            while (startIdx < _text.Length)
            {
                while (startIdx < _text.Length && _text[startIdx] == '\r')
                    startIdx++;

                if (startIdx < _text.Length)
                {
                    var endIdx = startIdx;
                    while (endIdx < _text.Length && char.IsWhiteSpace(_text[endIdx]) && _text[endIdx] != '\n')
                        endIdx++;

                    if (endIdx > startIdx)
                    {
                        if (preserveSpaces)
                            Words.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Substring(startIdx, endIdx - startIdx)), false, false));
                    }
                    else
                    {
                        endIdx = startIdx;
                        while (endIdx < _text.Length && !char.IsWhiteSpace(_text[endIdx]) && _text[endIdx] != '-' && WordBreak != CssConstants.BreakAll && !CommonUtils.IsAsianCharecter(_text[endIdx]))
                            endIdx++;

                        if (endIdx < _text.Length && (_text[endIdx] == '-' || WordBreak == CssConstants.BreakAll || CommonUtils.IsAsianCharecter(_text[endIdx])))
                            endIdx++;

                        if (endIdx > startIdx)
                        {
                            var hasSpaceBefore = !preserveSpaces && (startIdx > 0 && Words.Count == 0 && char.IsWhiteSpace(_text[startIdx - 1]));
                            var hasSpaceAfter = !preserveSpaces && (endIdx < _text.Length && char.IsWhiteSpace(_text[endIdx]));
                            Words.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Substring(startIdx, endIdx - startIdx)), hasSpaceBefore, hasSpaceAfter));
                        }
                    }

                    // create new-line word so it will effect the layout
                    if (endIdx < _text.Length && _text[endIdx] == '\n')
                    {
                        endIdx++;
                        if (respectNewLines)
                            Words.Add(new CssRectWord(this, "\n", false, false));
                    }

                    startIdx = endIdx;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            _imageLoadHandler?.Dispose();

            foreach (var childBox in Boxes)
            {
                childBox.Dispose();
            }
        }


        #region Private Methods

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.<br/>
        /// </summary>
        /// <param name="g">Device context to use</param>
        protected virtual async ValueTask PerformLayoutImp(RGraphics g)
        {
#if DEBUG
            Console.WriteLine($"layout start: {ToString()}");
#endif

            if (Display != CssConstants.None)
            {
                RectanglesReset();
                await MeasureWordsSize(g);
            }



            if (PageBreakBefore is CssConstants.Always)
            {
                var previousSibling = DomUtils.GetPreviousSibling(this);

                if (previousSibling is not null)
                {

                    var bottomRelativeToCurrentPage = previousSibling.ActualBottom;
                    var pageHeight = HtmlContainer.PageSize.Height;

                    while (bottomRelativeToCurrentPage > pageHeight)
                    {
                        bottomRelativeToCurrentPage -= pageHeight;
                    }

                    var pixelsToNextPage = pageHeight - bottomRelativeToCurrentPage;
                    previousSibling.ActualBottom += pixelsToNextPage + HtmlContainer.MarginTop;
                }
            }

            if (IsBlock || Display == CssConstants.ListItem || Display == CssConstants.Table || Display == CssConstants.InlineTable || Display == CssConstants.TableCell)
            {
                // Because their width and height are set by CssTable
                if (Display != CssConstants.TableCell && Display != CssConstants.Table)
                {
                    var width = ContainingBlock.ClientRight - ContainingBlock.ClientLeft - ActualMarginLeft - ActualMarginRight;

                    if (Words.Count > 0)
                    {
                        width = Words.Sum(x => x.FullWidth);
                    }

                    if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width))
                    {
                        width = CssValueParser.ParseLength(Width, width, this);
                    }

                    ActualRight = Location.X + width + ActualBoxSizeIncludedWidth;
                }

                if (Display != CssConstants.TableCell)
                {
                    if (Position is CssConstants.Static or CssConstants.Relative)
                    {
                        var prevSibling = DomUtils.GetPreviousSibling(this, false);

                        var left = ContainingBlock.ClientLeft;
                        var top = (prevSibling == null && ContainingBlock != null ? ContainingBlock.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (prevSibling != null ? prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth : 0);

                        Location = new RPoint(left + ActualMarginLeft, top);
                        ActualBottom = top;

                        CssLayoutEngine.FloatBox(this);
                    }

                    if (Position is CssConstants.Relative)
                    {
                        var left = Location.X + CssValueParser.ParseLength(Left, ActualWidth, this);
                        var top = Location.Y + CssValueParser.ParseLength(Top, ActualHeight, this);

                        Location = new RPoint(left, top);
                        ActualBottom = top;
                    }

                    if (Position is CssConstants.Absolute)
                    {
                        var nearestPositionedAncestor = DomUtils.GetNearestPositionedAncestor(this);

                        var left = nearestPositionedAncestor.Location.X +
                                   CssValueParser.ParseLength(Left, nearestPositionedAncestor.ActualWidth, this);

                        var top = nearestPositionedAncestor.Location.Y +
                                  CssValueParser.ParseLength(Top, nearestPositionedAncestor.ActualHeight, this);

                        Location = new RPoint(left, top);
                    }

                    if (Position is CssConstants.Fixed)
                    {
                        var left = CssValueParser.ParseLength(Left, HtmlContainer.ScrollOffset.X, this);
                        var top = CssValueParser.ParseLength(Top, HtmlContainer.ScrollOffset.Y, this);
                        Location = new RPoint(left, top);
                    }
                }

                //If we're talking about a table here...
                if (Display is CssConstants.Table or CssConstants.InlineTable)
                {
                    await CssLayoutEngineTable.PerformLayout(g, this);
                }
                else
                {
                    //If there's just inline boxes, create LineBoxes
                    if (DomUtils.ContainsInlinesOnly(this))
                    {
                        ActualBottom = Location.Y;
                        await CssLayoutEngine.CreateLineBoxes(g, this); //This will automatically set the bottom of this block

#if DEBUG
                        foreach (var lineBox in LineBoxes)
                        {
                            Console.WriteLine($"layout linebox: {lineBox} [h: {lineBox.LineBottom}]");
                        }
#endif

                    }
                    else if (Boxes.Count > 0)
                    {
                        foreach (var childBox in Boxes)
                        {
                            await childBox.PerformLayout(g);
                        }

                        ActualRight = CalculateActualRight();

                        if (Boxes.Any(b => !b.IsFloated))
                        {
                            ActualBottom = MarginBottomCollapse();
                        }
                    }
                }
            }
            else
            {
                var prevSibling = DomUtils.GetPreviousSibling(this, false);
                if (prevSibling != null)
                {
                    if (Location == RPoint.Empty)
                        Location = prevSibling.Location;
                    ActualBottom = prevSibling.ActualBottom;
                }
            }

            var height = ActualBoxSizingHeight;

            if (Words.Count > 0)
            {
                height = Math.Max(height, Words.Sum(w => w.Height));
            }

            if (CssValueParser.IsValidLength(MinHeight))
            {
                var minHeight = CssValueParser.ParseLength(MinHeight, ParentBox?.Size.Height ?? Size.Height, this) + ActualBoxSizeIncludedHeight;

                if (minHeight > height)
                {
                    height = minHeight;
                }
            }

            if (CssValueParser.IsValidLength(Height))
            {
                var cssHeight = CssValueParser.ParseLength(Height, ParentBox?.Size.Height ?? Size.Height, this) + ActualBoxSizeIncludedHeight;

                if (cssHeight > height)
                {
                    height = cssHeight;
                }
            }

            ActualBottom = Math.Max(ActualBottom, Location.Y + height);

            await CreateListItemBox(g);

#if DEBUG
            Console.WriteLine($"layout finish: {ToString()} [x: {Location.X}, y: {Location.Y}, b: {ActualBottom}, r: {ActualRight}, h: {Size.Height}, w: {Size.Width}]");
#endif
            if (IsFixed) return;

            var actualWidth = Math.Max(GetMinimumWidth() + GetWidthMarginDeep(this), Size.Width < 90999 ? ActualRight - HtmlContainer.Root.Location.X : 0);
            HtmlContainer.ActualSize = CommonUtils.Max(HtmlContainer.ActualSize, new RSize(actualWidth, ActualBottom - HtmlContainer.Root.Location.Y));
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g"></param>
        internal virtual async ValueTask MeasureWordsSize(RGraphics g)
        {
            if (_wordsSizeMeasured) return;

            if (BackgroundImage != CssConstants.None && _imageLoadHandler == null)
            {
                _imageLoadHandler = new ImageLoadHandler(HtmlContainer);
                await _imageLoadHandler.LoadImage(BackgroundImage);
            }

            MeasureWordSpacing(g);

            if (Words.Count > 0)
            {
                foreach (var boxWord in Words)
                {
                    boxWord.Width = boxWord.Text != "\n" ? g.MeasureString(boxWord.Text, ActualFont).Width : 0;
                    boxWord.Height = ActualFont.Height;
                }
            }

            _wordsSizeMeasured = true;
        }

        /// <summary>
        /// Get the parent of this css properties instance.
        /// </summary>
        /// <returns></returns>
        protected sealed override CssBoxProperties GetParent()
        {
            return _parentBox;
        }

        /// <summary>
        /// Gets the index of the box to be used on a (ordered) list
        /// </summary>
        /// <returns></returns>
        private int GetIndexForList()
        {
            bool reversed = !string.IsNullOrEmpty(ParentBox.GetAttribute("reversed"));
            if (!int.TryParse(ParentBox.GetAttribute("start"), out var index))
            {
                if (reversed)
                {
                    index = 0;
                    foreach (CssBox b in ParentBox.Boxes)
                    {
                        if (b.Display == CssConstants.ListItem)
                            index++;
                    }
                }
                else
                {
                    index = 1;
                }
            }

            foreach (CssBox b in ParentBox.Boxes)
            {
                if (b.Equals(this))
                    return index;

                if (b.Display == CssConstants.ListItem)
                    index += reversed ? -1 : 1;
            }

            return index;
        }

        /// <summary>
        /// Creates the <see cref="_listItemBox"/>
        /// </summary>
        /// <param name="g"></param>
        private async ValueTask CreateListItemBox(RGraphics g)
        {
            if (Display != CssConstants.ListItem || ListStyleType == CssConstants.None) return;

            if (_listItemBox == null)
            {
                _listItemBox = new CssBox(null, null);
                _listItemBox.InheritStyle(this);
                _listItemBox.Display = CssConstants.Inline;
                _listItemBox.HtmlContainer = HtmlContainer;

                if (ListStyleType.Equals(CssConstants.Disc, StringComparison.InvariantCultureIgnoreCase))
                {
                    _listItemBox.Text = "•";
                }
                else if (ListStyleType.Equals(CssConstants.Circle, StringComparison.InvariantCultureIgnoreCase))
                {
                    _listItemBox.Text = "o";
                }
                else if (ListStyleType.Equals(CssConstants.Square, StringComparison.InvariantCultureIgnoreCase))
                {
                    _listItemBox.Text = "\u25a0";
                }
                else if (ListStyleType.Equals(CssConstants.Decimal, StringComparison.InvariantCultureIgnoreCase))
                {
                    _listItemBox.Text = GetIndexForList().ToString(CultureInfo.InvariantCulture) + ".";
                }
                else if (ListStyleType.Equals(CssConstants.DecimalLeadingZero, StringComparison.InvariantCultureIgnoreCase))
                {
                    _listItemBox.Text = GetIndexForList().ToString("00", CultureInfo.InvariantCulture) + ".";
                }
                else
                {
                    _listItemBox.Text = CommonUtils.ConvertToAlphaNumber(GetIndexForList(), ListStyleType) + ".";
                }

                _listItemBox.ParseToWords();

                await _listItemBox.PerformLayoutImp(g);
                _listItemBox.Size = new RSize(_listItemBox.Words[0].Width, _listItemBox.Words[0].Height);
            }
            _listItemBox.Words[0].Left = Location.X - _listItemBox.Size.Width - 5;
            _listItemBox.Words[0].Top = Location.Y + ActualPaddingTop; // +FontAscent;
        }

        /// <summary>
        /// Searches for the first word occurrence inside the box, on the specified linebox
        /// </summary>
        /// <param name="b"></param>
        /// <param name="line"> </param>
        /// <returns></returns>
        internal static CssRect FirstWordOccurence(CssBox b, CssLineBox line)
        {
            switch (b.Words.Count)
            {
                case 0 when b.Boxes.Count == 0:
                    return null;
                case > 0:
                {
                    foreach (CssRect word in b.Words)
                    {
                        if (line.Words.Contains(word))
                        {
                            return word;
                        }
                    }
                    return null;
                }
                default:
                {
                    foreach (CssBox bb in b.Boxes)
                    {
                        CssRect w = FirstWordOccurence(bb, line);

                        if (w != null)
                        {
                            return w;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the specified Attribute, returns string.Empty if no attribute specified
        /// </summary>
        /// <param name="attribute">Attribute to retrieve</param>
        /// <returns>Attribute value or string.Empty if no attribute specified</returns>
        internal string GetAttribute(string attribute)
        {
            return GetAttribute(attribute, string.Empty);
        }

        /// <summary>
        /// Gets the value of the specified attribute of the source HTML tag.
        /// </summary>
        /// <param name="attribute">Attribute to retrieve</param>
        /// <param name="defaultValue">Value to return if attribute is not specified</param>
        /// <returns>Attribute value or defaultValue if no attribute specified</returns>
        internal string GetAttribute(string attribute, string defaultValue)
        {
            return HtmlTag != null ? HtmlTag.TryGetAttribute(attribute, defaultValue) : defaultValue;
        }

        /// <summary>
        /// Gets the minimum width that the box can be.<br/>
        /// The box can be as thin as the longest word plus padding.<br/>
        /// The check is deep thru box tree.<br/>
        /// </summary>
        /// <returns>the min width of the box</returns>
        internal double GetMinimumWidth()
        {
            double maxWidth = 0;
            CssRect maxWidthWord = null;
            GetMinimumWidth_LongestWord(this, ref maxWidth, ref maxWidthWord);

            double padding = 0f;
            if (maxWidthWord != null)
            {
                var box = maxWidthWord.OwnerBox;
                while (box != null)
                {
                    padding += box.ActualBorderRightWidth + box.ActualPaddingRight + box.ActualBorderLeftWidth + box.ActualPaddingLeft;
                    box = box != this ? box.ParentBox : null;
                }
            }

            return maxWidth + padding;
        }

        /// <summary>
        /// Gets the longest word (in width) inside the box, deeply.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="maxWidth"> </param>
        /// <param name="maxWidthWord"> </param>
        /// <returns></returns>
        private static void GetMinimumWidth_LongestWord(CssBox box, ref double maxWidth, ref CssRect maxWidthWord)
        {
            if (box.Words.Count > 0)
            {
                foreach (CssRect cssRect in box.Words)
                {
                    if (cssRect.Width > maxWidth)
                    {
                        maxWidth = cssRect.Width;
                        maxWidthWord = cssRect;
                    }
                }
            }
            else
            {
                foreach (CssBox childBox in box.Boxes)
                    GetMinimumWidth_LongestWord(childBox, ref maxWidth, ref maxWidthWord);
            }
        }

        /// <summary>
        /// Get the total margin value (left and right) from the given box to the given end box.<br/>
        /// </summary>
        /// <param name="box">the box to start calculation from.</param>
        /// <returns>the total margin</returns>
        private static double GetWidthMarginDeep(CssBox box)
        {
            double sum = 0f;
            if (box.Size.Width > 90999 || box.ParentBox is { Size.Width: > 90999 })
            {
                while (box != null)
                {
                    sum += box.ActualMarginLeft + box.ActualMarginRight;
                    box = box.ParentBox;
                }
            }
            return sum;
        }

        /// <summary>
        /// Gets the maximum bottom of the boxes inside the startBox
        /// </summary>
        /// <param name="startBox"></param>
        /// <param name="currentMaxBottom"></param>
        /// <returns></returns>
        internal static double GetMaximumBottom(CssBox startBox, double currentMaxBottom)
        {
            foreach (var line in startBox.Rectangles.Keys)
            {
                currentMaxBottom = Math.Max(currentMaxBottom, startBox.Rectangles[line].Bottom);
            }

            foreach (var b in startBox.Boxes)
            {
                currentMaxBottom = Math.Max(currentMaxBottom, GetMaximumBottom(b, currentMaxBottom));
            }

            return currentMaxBottom;
        }

        /// <summary>
        /// Get the <paramref name="minWidth"/> and <paramref name="maxWidth"/> width of the box content.<br/>
        /// </summary>
        /// <param name="minWidth">The minimum width the content must be so it won't overflow (largest word + padding).</param>
        /// <param name="maxWidth">The total width the content can take without line wrapping (with padding).</param>
        internal void GetMinMaxWidth(out double minWidth, out double maxWidth)
        {
            double min = 0f;
            double maxSum = 0f;
            double paddingSum = 0f;
            double marginSum = 0f;
            GetMinMaxSumWords(this, ref min, ref maxSum, ref paddingSum, ref marginSum);

            maxWidth = paddingSum + maxSum;
            minWidth = paddingSum + (min < 90999 ? min : 0);
        }

        /// <summary>
        /// Get the <paramref name="min"/> and <paramref name="maxSum"/> of the box words content and <paramref name="paddingSum"/>.<br/>
        /// </summary>
        /// <param name="box">the box to calculate for</param>
        /// <param name="min">the width that allows for each word to fit (width of the longest word)</param>
        /// <param name="maxSum">the max width a single line of words can take without wrapping</param>
        /// <param name="paddingSum">the total amount of padding the content has </param>
        /// <param name="marginSum"></param>
        /// <returns></returns>
        private static void GetMinMaxSumWords(CssBox box, ref double min, ref double maxSum, ref double paddingSum, ref double marginSum)
        {
            double? oldSum = null;

            // not inline (block) boxes start a new line so we need to reset the max sum
            if (box.Display != CssConstants.Inline && box.Display != CssConstants.TableCell && box.WhiteSpace != CssConstants.NoWrap)
            {
                oldSum = maxSum;
                maxSum = marginSum;
            }

            // add the padding 
            paddingSum += box.ActualBorderLeftWidth + box.ActualBorderRightWidth + box.ActualPaddingRight + box.ActualPaddingLeft;


            // for tables the padding also contains the spacing between cells
            if (box.Display == CssConstants.Table)
                paddingSum += CssLayoutEngineTable.GetTableSpacing(box);

            if (box.Words.Count > 0)
            {
                // calculate the min and max sum for all the words in the box
                foreach (CssRect word in box.Words)
                {
                    maxSum += word.FullWidth + (word.HasSpaceBefore ? word.OwnerBox.ActualWordSpacing : 0);
                    min = Math.Max(min, word.Width);
                }

                // remove the last word padding
                if (box.Words.Count > 0 && !box.Words[^1].HasSpaceAfter)
                    maxSum -= box.Words[^1].ActualWordSpacing;
            }
            else
            {
                // recursively on all the child boxes
                for (int i = 0; i < box.Boxes.Count; i++)
                {
                    CssBox childBox = box.Boxes[i];
                    marginSum += childBox.ActualMarginLeft + childBox.ActualMarginRight;

                    //maxSum += childBox.ActualMarginLeft + childBox.ActualMarginRight;
                    GetMinMaxSumWords(childBox, ref min, ref maxSum, ref paddingSum, ref marginSum);

                    marginSum -= childBox.ActualMarginLeft + childBox.ActualMarginRight;
                }
            }

            // max sum is max of all the lines in the box
            if (oldSum.HasValue)
            {
                maxSum = Math.Max(maxSum, oldSum.Value);
            }
        }

        /// <summary>
        /// Gets if this box has only inline siblings (including itself)
        /// </summary>
        /// <returns></returns>
        internal bool HasJustInlineSiblings()
        {
            return ParentBox != null && DomUtils.ContainsInlinesOnly(ParentBox);
        }

        /// <summary>
        /// Gets the rectangles where inline box will be drawn. See Remarks for more info.
        /// </summary>
        /// <returns>Rectangles where content should be placed</returns>
        /// <remarks>
        /// Inline boxes can be split across different LineBoxes, that's why this method
        /// Delivers a rectangle for each LineBox related to this box, if inline.
        /// </remarks>
        /// <summary>
        /// Inherits inheritable values from parent.
        /// </summary>
        internal new void InheritStyle(CssBox box = null, bool everything = false)
        {
            base.InheritStyle(box ?? ParentBox, everything);
        }

        /// <summary>
        /// Gets the result of collapsing the vertical margins of the two boxes
        /// </summary>
        /// <param name="prevSibling">the previous box under the same parent</param>
        /// <returns>Resulting top margin</returns>
        protected double MarginTopCollapse(CssBoxProperties prevSibling)
        {
            double value;
            if (prevSibling != null)
            {
                value = Math.Max(prevSibling.ActualMarginBottom, ActualMarginTop);
                CollapsedMarginTop = value;
            }
            else if (_parentBox != null && ActualPaddingTop < 0.1 && ActualPaddingBottom < 0.1 && _parentBox.ActualPaddingTop < 0.1 && _parentBox.ActualPaddingBottom < 0.1)
            {
                value = Math.Max(0, ActualMarginTop - Math.Max(_parentBox.ActualMarginTop, _parentBox.CollapsedMarginTop));
            }
            else
            {
                value = ActualMarginTop;
            }

            // fix for hr tag
            if (value < 0.1 && HtmlTag is { Name: "hr" })
            {
                value = GetEmHeight() * 1.1f;
            }

            return value;
        }

        public bool BreakPage()
        {
            var container = this.HtmlContainer;

            if (this.Size.Height >= container.PageSize.Height)
                return false;

            var remTop = (this.Location.Y - container.MarginTop) % container.PageSize.Height;
            var remBottom = (this.ActualBottom - container.MarginTop) % container.PageSize.Height;

            if (remTop > remBottom)
            {
                var diff = container.PageSize.Height - remTop;
                this.Location = new RPoint(this.Location.X, this.Location.Y + diff + 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate the actual right of the box by the actual right of the child boxes if this box actual right is not set.
        /// </summary>
        /// <returns>the calculated actual right value</returns>
        private double CalculateActualRight()
        {
            if (!(ActualRight > 90999)) return ActualRight;

            var maxRight = 0d;

            double additionalMarginRight;

            foreach (var box in Boxes)
            { 
                additionalMarginRight = box.BoxSizing switch
                {
                    CssConstants.ContentBox => 0,
                    CssConstants.BorderBox => box.ActualMarginRight,
                    _ => throw new HtmlRenderException("Unknown BoxSizing", HtmlRenderErrorType.Layout)
                };

                maxRight = Math.Max(maxRight, box.ActualRight + additionalMarginRight);
            }

            additionalMarginRight = BoxSizing switch
            {
                CssConstants.ContentBox => 0,
                CssConstants.BorderBox => ActualMarginRight,
                _ => throw new HtmlRenderException("Unknown BoxSizing", HtmlRenderErrorType.Layout)
            };

            return maxRight + ActualPaddingRight + additionalMarginRight + ActualBorderRightWidth;

        }

        /// <summary>
        /// Gets the result of collapsing the vertical margins of the two boxes
        /// </summary>
        /// <returns>Resulting bottom margin</returns>
        private double MarginBottomCollapse()
        {
            var lastNonFloatingBox = Boxes.Last(b => !b.IsFloated);

            double margin = 0;
            if (ParentBox == null || ParentBox.Boxes.IndexOf(this) != ParentBox.Boxes.Count - 1 ||
                !(_parentBox.ActualMarginBottom < 0.1))
                return Math.Max(ActualBottom,
                    lastNonFloatingBox.ActualBottom + margin + ActualPaddingBottom + ActualBorderBottomWidth);

            var lastChildBottomMargin = lastNonFloatingBox.ActualMarginBottom;
            margin = Height == "auto" ? Math.Max(ActualMarginBottom, lastChildBottomMargin) : lastChildBottomMargin;
            return Math.Max(ActualBottom, lastNonFloatingBox.ActualBottom + margin + ActualPaddingBottom + ActualBorderBottomWidth);
        }

        /// <summary>
        /// Deeply offsets the top of the box and its contents
        /// </summary>
        /// <param name="amount"></param>
        internal void OffsetTop(double amount)
        {
            List<CssLineBox> lines = [];
            foreach (CssLineBox line in Rectangles.Keys)
                lines.Add(line);

            foreach (CssLineBox line in lines)
            {
                RRect r = Rectangles[line];
                Rectangles[line] = new RRect(r.X, r.Y + amount, r.Width, r.Height);
            }

            foreach (CssRect word in Words)
            {
                word.Top += amount;
            }

            foreach (CssBox b in Boxes)
            {
                b.OffsetTop(amount);
            }

            _listItemBox?.OffsetTop(amount);

            Location = new RPoint(Location.X, Location.Y + amount);
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected virtual async ValueTask PaintImp(RGraphics g)
        {
            if (Display == CssConstants.None ||
                (Display == CssConstants.TableCell && EmptyCells == CssConstants.Hide && IsSpaceOrEmpty)) return;

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, this);

            var areas = Rectangles.Count == 0 ? new List<RRect>([Bounds]) : new List<RRect>(Rectangles.Values);
            var clip = g.GetClip();
            var rects = areas.ToArray();
            var offset = RPoint.Empty;

            if (!IsFixed)
            {
                offset = HtmlContainer.ScrollOffset;
            }

            for (var i = 0; i < rects.Length; i++)
            {
                var actualRect = rects[i];
                actualRect.Offset(offset);

                if (!IsRectVisible(actualRect, clip)) continue;

                PaintBackground(g, actualRect, i == 0);
                BordersDrawHandler.DrawBoxBorders(g, this, actualRect, i == 0, i == rects.Length - 1);
            }

            PaintWords(g, offset);

            for (var i = 0; i < rects.Length; i++)
            {
                var actualRect = rects[i];
                actualRect.Offset(offset);

                if (IsRectVisible(actualRect, clip))
                {
                    PaintDecoration(g, actualRect, i == 0, i == rects.Length - 1);
                }
            }

            // split paint to handle z-order
            foreach (var b in Boxes)
            {
                if (b.Position != CssConstants.Absolute && !b.IsFixed)
                    await b.Paint(g);
            }

            foreach (var b in Boxes)
            {
                if (b.Position == CssConstants.Absolute)
                    await b.Paint(g);
            }

            foreach (var b in Boxes)
            {
                if (b.IsFixed)
                    await b.Paint(g);
            }

            if (clipped)
                g.PopClip();

            if (_listItemBox is not null)
            {
                await _listItemBox.Paint(g);
            }
        }

        private static bool IsRectVisible(RRect rect, RRect clip)
        {
            rect.X -= 2;
            rect.Width += 2;
            clip.Intersect(rect);

            return clip != RRect.Empty;
        }

        /// <summary>
        /// Paints the background of the box
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rect">the bounding rectangle to draw in</param>
        /// <param name="isFirst">is it the first rectangle of the element</param>
        protected void PaintBackground(RGraphics g, RRect rect, bool isFirst)
        {
            if (rect is { Width: > 0, Height: > 0 })
            {
                RBrush brush = null;

                if (BackgroundGradient != CssConstants.None)
                {
                    brush = g.GetLinearGradientBrush(rect, ActualBackgroundColor, ActualBackgroundGradient, ActualBackgroundGradientAngle);
                }
                else if (RenderUtils.IsColorVisible(ActualBackgroundColor))
                {
                    brush = g.GetSolidBrush(ActualBackgroundColor);
                }

                if (brush != null)
                {
                    // TODO:a handle it correctly (tables background)
                    // if (isLast)
                    //  rectangle.Width -= ActualWordSpacing + CssUtils.GetWordEndWhitespace(ActualFont);

                    RGraphicsPath roundrect = null;
                    if (IsRounded)
                    {
                        roundrect = RenderUtils.GetRoundRect(g, rect, ActualCornerNw, ActualCornerNe, ActualCornerSe, ActualCornerSw);
                    }

                    Object prevMode = null;
                    if (HtmlContainer is { AvoidGeometryAntialias: false } && IsRounded)
                    {
                        prevMode = g.SetAntiAliasSmoothingMode();
                    }

                    if (roundrect != null)
                    {
                        g.DrawPath(brush, roundrect);
                    }
                    else
                    {
                        g.DrawRectangle(brush, Math.Ceiling(rect.X), Math.Ceiling(rect.Y), rect.Width, rect.Height);
                    }

                    g.ReturnPreviousSmoothingMode(prevMode);

                    roundrect?.Dispose();
                    brush.Dispose();
                }

                if (_imageLoadHandler is { Image: not null } && isFirst)
                {
                    BackgroundImageDrawHandler.DrawBackgroundImage(g, this, _imageLoadHandler, rect);
                }
            }
        }

        /// <summary>
        /// Paint all the words in the box.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="offset">the current scroll offset to offset the words</param>
        private void PaintWords(RGraphics g, RPoint offset)
        {
            if (Width.Length <= 0) return;

            var isRtl = Direction == CssConstants.Rtl;
            foreach (var word in Words)
            {
                if (word.IsLineBreak) continue;
                var clip = g.GetClip();
                var wordRect = word.Rectangle;
                wordRect.Offset(offset);
                clip.Intersect(wordRect);

                if (clip == RRect.Empty) continue;
                var wordPoint = new RPoint(word.Left + offset.X, word.Top + offset.Y);
                g.DrawString(word.Text, ActualFont, ActualColor, wordPoint, new RSize(word.Width, word.Height), isRtl);
            }
        }

        /// <summary>
        /// Paints the text decoration (underline/strike-through/over-line)
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rectangle"> </param>
        /// <param name="isFirst"> </param>
        /// <param name="isLast"> </param>
        protected void PaintDecoration(RGraphics g, RRect rectangle, bool isFirst, bool isLast)
        {
            var textDecorationLine = TextDecorationLine;
            var textDecorationStyle = TextDecorationStyle;
            var textDecorationColor = TextDecorationColor;

            var textDecorationParts = TextDecoration?.Split(' ') ?? [];
            

            if (textDecorationParts.Length > 0)
            {
                HashSet<string> lineValues =
                    [
                    CssConstants.Underline, CssConstants.Overline, CssConstants.LineThrough, CssConstants.Blink];

                HashSet<string> styleValues =
                [
                    CssConstants.Solid, CssConstants.Double, CssConstants.Dotted, CssConstants.Dashed, CssConstants.Wavy
                ];

                foreach (var textDecorationPart in textDecorationParts)
                {
                    if (string.IsNullOrEmpty(textDecorationLine) && lineValues.Contains(textDecorationPart))
                    {
                        textDecorationLine = textDecorationPart;
                    }

                    if (string.IsNullOrEmpty(textDecorationStyle) && styleValues.Contains(textDecorationPart))
                    {
                        textDecorationStyle = textDecorationPart;
                    }

                    if (string.IsNullOrEmpty(textDecorationColor) &&
                        _htmlContainer.CssParser.IsColorValid(textDecorationPart))
                    {
                        textDecorationColor = textDecorationPart;
                    }
                }
            }

            if (string.IsNullOrEmpty(textDecorationLine) || textDecorationLine == CssConstants.None)
                return;

            if (string.IsNullOrEmpty(textDecorationStyle))
            {
                textDecorationStyle = CssConstants.Solid;
            }

            if (!string.IsNullOrEmpty(textDecorationColor) && !HtmlContainer.CssParser.IsColorValid(textDecorationColor))
            {
                textDecorationColor = string.Empty;
            }

            var textDecorationActualColor = string.IsNullOrEmpty(textDecorationColor) ? ActualColor : 
                HtmlContainer.CssParser.ParseColor(textDecorationColor);

            double y = textDecorationLine switch
            {
                CssConstants.Underline => Math.Round(rectangle.Top + ActualFont.UnderlineOffset),
                CssConstants.LineThrough => rectangle.Top + rectangle.Height / 2f,
                CssConstants.Overline => rectangle.Top,
                _ => 0f
            };

            y -= ActualPaddingBottom - ActualBorderBottomWidth;

            double x1 = rectangle.X;
            if (isFirst)
                x1 += ActualPaddingLeft + ActualBorderLeftWidth;

            double x2 = rectangle.Right;
            if (isLast)
                x2 -= ActualPaddingRight + ActualBorderRightWidth;

            var dashStyle = textDecorationStyle switch
            {
                CssConstants.Solid => RDashStyle.Solid,
                CssConstants.Double => RDashStyle.Solid,
                CssConstants.Dotted => RDashStyle.Dot,
                CssConstants.Dashed => RDashStyle.Dash,
                CssConstants.Wavy => RDashStyle.Solid,
                _ => RDashStyle.Solid
            };

            var pen = g.GetPen(textDecorationActualColor);
            pen.Width = 1;
            pen.DashStyle = dashStyle;
            g.DrawLine(pen, x1, y, x2, y);
        }

        /// <summary>
        /// Offsets the rectangle of the specified linebox by the specified gap,
        /// and goes deep for rectangles of children in that linebox.
        /// </summary>
        /// <param name="lineBox"></param>
        /// <param name="gap"></param>
        internal void OffsetRectangle(CssLineBox lineBox, double gap)
        {
            if (!Rectangles.TryGetValue(lineBox, out var r)) return;
            Rectangles[lineBox] = new RRect(r.X, r.Y + gap, r.Width, r.Height);
        }

        /// <summary>
        /// Resets the <see cref="Rectangles"/> array
        /// </summary>
        internal void RectanglesReset()
        {
            Rectangles.Clear();
        }

        protected override RFont GetCachedFont(string fontFamily, double fsize, RFontStyle st)
        {
            var families = fontFamily.Split(',');

            if (families.Length == 1)
            {
                return HtmlContainer.Adapter.GetFont(fontFamily, fsize, st);
            }

            RFont selectedFont = null;

            foreach (var family in families)
            {
                var selectedFamily = family.TrimStart('"').TrimEnd('"');

                selectedFont = HtmlContainer.Adapter.GetFont(selectedFamily, fsize, st);

                if (selectedFont is not null)
                {
                    break;
                }
            }

            return selectedFont;
        }

        protected override RColor GetActualColor(string colorStr)
        {
            return HtmlContainer.CssParser.ParseColor(colorStr);
        }

        protected override RPoint GetActualLocation(string X, string Y)
        {
            var left = CssValueParser.ParseLength(X, this.HtmlContainer.PageSize.Width, this, null);
            var top = CssValueParser.ParseLength(Y, this.HtmlContainer.PageSize.Height, this, null);
            return new RPoint(left, top);
        }

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var tag = HtmlTag != null ? $"<{HtmlTag.Name}#{_id}>" : $"anon#{_id}";

            if (HtmlTag?.Attributes?.ContainsKey("class") ?? false)
            {
                tag = $"{tag}, Class: {HtmlTag.Attributes["class"]}";
            }

            if (HtmlTag?.Attributes?.ContainsKey("id") ?? false)
            {
                tag = $"{tag}, Id: {HtmlTag.Attributes["id"]}";
            }

            if (HtmlTag?.Attributes?.ContainsKey("src") ?? false)
            {
                tag = $"{tag}, Src: {HtmlTag.Attributes["src"]}";
            }

            if (Text is not null)
            {
                tag = $"{tag} Text: {Text}";
            }

            if (IsBlock)
            {
                return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} Block {FontSize}, Children:{Boxes.Count}";
            }
            else if (Display == CssConstants.None)
            {
                return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} None";
            }
            else
            {
                return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} {Display}: {Text}";
            }
        }

        #endregion
    }
}