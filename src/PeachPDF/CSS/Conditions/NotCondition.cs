﻿using System.IO;

namespace PeachPDF.CSS
{
    internal sealed class NotCondition : StylesheetNode, IConditionFunction
    {
        private IConditionFunction _content;

        public IConditionFunction Content
        {
            get => _content ?? new EmptyCondition();
            set
            {
                if (_content != null) RemoveChild(_content);

                _content = value;

                if (value != null) AppendChild(_content);
            }
        }

        public bool Check()
        {
            return !Content.Check();
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write("not ");
            Content.ToCss(writer, formatter);
        }
    }
}