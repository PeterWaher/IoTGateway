using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// ParseMarkdown(Markdown)
	/// </summary>
	public class ParseMarkdown : FunctionOneScalarStringVariable
    {
		/// <summary>
		/// ParseMarkdown(Markdown)
		/// </summary>
		/// <param name="Markdown">Markdown.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ParseMarkdown(ScriptNode Markdown, int Start, int Length, Expression Expression)
            : base(Markdown, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(ParseMarkdown);

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
			MarkdownSettings Settings = new MarkdownSettings()
			{
				Variables = Variables,
				ParseMetaData = MarkdownDocument.HeaderEndPosition(Argument).HasValue
			};

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Argument, Settings);

			return new ObjectValue(Doc);
        }

    }
}
