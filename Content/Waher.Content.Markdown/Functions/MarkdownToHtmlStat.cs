using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// MarkdownToHtmlStat(s)
	/// </summary>
	public class MarkdownToHtmlStat : FunctionOneScalarVariable
	{
		/// <summary>
		/// MarkdownToHtmlStat(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MarkdownToHtmlStat(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(MarkdownToHtmlStat);

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
			MarkdownDocument Doc;

			Argument = "BodyOnly: 1\r\n\r\n" + Argument;

			if (Variables.TryGetVariable(" MarkdownSettings ", out Variable v) && v.ValueObject is MarkdownSettings ParentSettings)
				Doc = await MarkdownDocument.CreateAsync(Argument, ParentSettings);
			else
				Doc = await MarkdownDocument.CreateAsync(Argument);

			string Html = await Doc.GenerateHTML();
			Markdown.MarkdownStatistics Statistics = Doc.GetStatistics();

			return new ObjectVector(new IElement[]
			{
				new StringValue(Html),
				new ObjectValue(Statistics)
			});
		}

	}
}
