using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Abstract base class for semantic conversion functions.
	/// </summary>
	public abstract class SemanticConversionFunction : FunctionOneVariable, IExtensionFunction
	{
		/// <summary>
		/// Abstract base class for semantic conversion functions.
		/// </summary>
		public SemanticConversionFunction()
			: base(null, 0, 0, null)
		{
		}

		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SemanticConversionFunction(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Minimum number of arguments
		/// </summary>
		public int MinArguments => 1;

		/// <summary>
		/// Maximum number of arguments
		/// </summary>
		public int MaxArguments => 1;

		/// <summary>
		/// Creates a function node.
		/// </summary>
		/// <param name="Arguments">Parsed arguments.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		/// <returns>Function script node.</returns>
		public ScriptNode CreateFunction(ScriptNode[] Arguments, int Start, int Length, Expression Expression)
		{
			return this.CreateFunction(Arguments[0], Start, Length, Expression);
		}

		/// <summary>
		/// Creates a function node.
		/// </summary>
		/// <param name="Argument">Parsed argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		/// <returns>Function script node.</returns>
		public abstract ScriptNode CreateFunction(ScriptNode Argument, int Start, int Length, Expression Expression);

		/// <summary>
		/// How well the extension function supports an URI.
		/// </summary>
		/// <param name="Uri">Uri for function.</param>
		/// <returns>Support grade.</returns>
		public virtual Grade Supports(string Uri)
		{
			return Uri == this.FunctionName ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Arg = this.Argument.Evaluate(Variables);
			return this.Convert(Arg.AssociatedObjectValue);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement Arg = await this.Argument.EvaluateAsync(Variables);
			return this.Convert(Arg.AssociatedObjectValue);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return this.Convert(Argument.AssociatedObjectValue);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.Convert(Argument.AssociatedObjectValue));
		}

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public abstract IElement Convert(object Value);
	}
}
