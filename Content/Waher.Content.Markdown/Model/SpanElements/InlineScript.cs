using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Script;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineScript : MarkdownElement
	{
        private Expression expression;
        private Variables variables;

        /// <summary>
        /// Inline source code.
        /// </summary>
        /// <param name="Document">Markdown document.</param>
        /// <param name="Expression">Expression.</param>
        /// <param name="Variables">Collection of variables to use when executing the script.</param>
        public InlineScript(MarkdownDocument Document, Expression Expression, Variables Variables)
			: base(Document)
		{
            this.expression = Expression;
            this.variables = Variables;

        }

		/// <summary>
		/// Expression
		/// </summary>
		public Expression Expresion
		{
			get { return this.expression; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
            object Result = this.expression.Evaluate(this.variables);
            if (Result == null)
                return;

            Output.Append(MarkdownDocument.HtmlValueEncode(Result.ToString()));
        }

        /// <summary>
        /// Generates plain text for the markdown element.
        /// </summary>
        /// <param name="Output">Plain text will be output here.</param>
        public override void GeneratePlainText(StringBuilder Output)
		{
            object Result = this.expression.Evaluate(this.variables);
            if (Result == null)
                return;

            Output.Append(Result.ToString());
        }

        /// <summary>
        /// Generates XAML for the markdown element.
        /// </summary>
        /// <param name="Output">XAML will be output here.</param>
        /// <param name="Settings">XAML settings.</param>
        /// <param name="TextAlignment">Alignment of text in element.</param>
        public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
            object Result = this.expression.Evaluate(this.variables);
            if (Result == null)
                return;

            Output.WriteValue(Result.ToString());
        }

        /// <summary>
        /// If the element is an inline span element.
        /// </summary>
        internal override bool InlineSpanElement
		{
			get { return true; }
		}

	}
}
