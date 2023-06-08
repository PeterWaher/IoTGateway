using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions
{
	/// <summary>
	/// Orders in ascending order
	/// </summary>
	public class Asc : FunctionOneVariable
	{
		/// <summary>
		/// Ascending argument
		/// </summary>
		/// <param name="Argument">Argument to order on.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Asc(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Asc);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.Argument.Evaluate(Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return Argument;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is true.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return this.Argument.EvaluateAsync(Variables);
		}
	}
}
