﻿using System.IO;

namespace PeachPDF.CSS
{
    public sealed class UnknownSelector : StylesheetNode, ISelector
    {
        public Priority Specificity => Priority.Zero;

        public string Text => this.ToCss();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(StylesheetText?.Text);
        }
    }
}