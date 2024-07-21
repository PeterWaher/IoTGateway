using System.Threading.Tasks;
using Waher.Content.Html.Elements;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
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
		public override string FunctionName => nameof(MarkdownToHtml);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			return new StringValue(await ToHtml(Argument, Variables));
		}

		/// <summary>
		/// Converts a Markdown snippet to a HTML snippet.
		/// </summary>
		/// <param name="Markdown">Markdown</param>
		/// <returns>HTML</returns>
		public static  Task<string> ToHtml(string Markdown)
		{
			return ToHtml(Markdown, null);
		}

		/// <summary>
		/// Converts a Markdown snippet to a HTML snippet.
		/// </summary>
		/// <param name="Markdown">Markdown</param>
		/// <param name="Variables">Optional collection of variables.</param>
		/// <returns>HTML</returns>
		public static async Task<string> ToHtml(string Markdown, Variables Variables)
		{
			MarkdownDocument Doc;

			Markdown = "BodyOnly: 1\r\n\r\n" + Markdown;

			if (Variables is null)
				Doc = await MarkdownDocument.CreateAsync(Markdown);
			else if (Variables.TryGetVariable(" MarkdownSettings ", out Variable v) &&
				v.ValueObject is MarkdownSettings Settings)
			{
				Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			}
			else
			{
				Settings = new MarkdownSettings()
				{
					Variables = Variables
				};

				Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			}

			return await Doc.GenerateHTML();
		}

	}
}
