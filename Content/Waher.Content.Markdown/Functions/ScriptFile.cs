using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// Executes script from a file.
	/// Corresponds to the SCRIPT meta-data tag in Markdown.
	/// </summary>
	public class ScriptFile : FunctionOneScalarVariable
	{
		/// <summary>
		/// Executes script from a file, if not executed before, or if file timestamp has changed.
		/// Corresponds to the INIT meta-data tag in Markdown.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ScriptFile(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ScriptFile);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "FileName" };

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
			string Source = this.Expression.Source;
			if (string.IsNullOrEmpty(Source))
				throw new ScriptRuntimeException("Script has no source.", this);

			Source = Path.Combine(Source, Argument);

			string Script = await Resources.ReadAllTextAsync(Source);
			Expression Exp = new Expression(Script, Source);

			return await Exp.Root.EvaluateAsync(Variables);
		}
	}
}
