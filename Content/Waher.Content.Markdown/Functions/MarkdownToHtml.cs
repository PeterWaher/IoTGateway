using System;
using Waher.Content.Html.Elements;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// MarkdownToHtml(s)
	/// </summary>
	public class MarkdownToHtml : FunctionOneScalarVariable
	{
		/// <summary>
		/// MarkdownToHtml(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MarkdownToHtml(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "markdowntohtml"; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			MarkdownDocument Doc;

			Argument = "BodyOnly: 1\r\n\r\n" + Argument;

			if (Variables.TryGetVariable(" MarkdownSettings ", out Variable v) && v.ValueObject is MarkdownSettings ParentSettings)
				Doc = new MarkdownDocument(Argument, ParentSettings);
			else
				Doc = new MarkdownDocument(Argument);

			string Html = Doc.GenerateHTML();
			return new StringValue(Html);
		}

	}
}
