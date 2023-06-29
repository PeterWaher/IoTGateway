using System.Numerics;
using System.Threading.Tasks;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Scalar;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a boolean literal.
	/// </summary>
	public class ToBooleanLiteral : FunctionOneScalarVariable, IExtensionFunction
	{
		/// <summary>
		/// Converts a value to a boolean literal.
		/// </summary>
		public ToBooleanLiteral()
			: base(new ConstantElement(ObjectValue.Null, 0, 0, null), 0, 0, null)
		{
		}

		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToBooleanLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
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
			return new ToBooleanLiteral(Arguments[0], Start, Length, Expression);
		}

		/// <summary>
		/// How well the extension function supports an URI.
		/// </summary>
		/// <param name="Uri">Uri for function.</param>
		/// <returns>Support grade.</returns>
		public Grade Supports(string Uri)
		{
			return Uri == BooleanLiteral.TypeUri ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ToBooleanLiteral);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			return new BooleanLiteral(Argument != 0);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument, Variables Variables)
		{
			return new BooleanLiteral(Argument != Complex.Zero);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(bool Argument, Variables Variables)
		{
			return new BooleanLiteral(Argument);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			bool? Value = Boolean.ToBoolean(Argument.ToLower());

			if (Value.HasValue)
				return new BooleanLiteral(Value.Value);
			else
				throw new ScriptException("Not a boolean value.");
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			return this.EvaluateScalar(Argument.ToString(), Variables);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateScalar(Argument, Variables));
		}
	}
}
