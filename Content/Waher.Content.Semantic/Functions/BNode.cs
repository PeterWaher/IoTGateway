using System;
using System.Threading.Tasks;
using Waher.Content.Semantic.Model;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// BNode([x])
	/// </summary>
	public class BNode : FunctionMultiVariate
	{
		/// <summary>
		/// BNode()
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BNode(int Start, int Length, Expression Expression)
			: base(new ScriptNode[0], argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// BNode(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BNode(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Argument }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(BNode);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Label" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments.Length == 0)
				return new BlankNode(Guid.NewGuid().ToString());

			string Label = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;

			return new BlankNode(Label);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Arguments, Variables));
		}
	}
}
