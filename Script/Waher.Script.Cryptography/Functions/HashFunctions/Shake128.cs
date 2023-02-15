using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security.SHA3;

namespace Waher.Script.Cryptography.Functions.HashFunctions
{
	/// <summary>
	/// Shake128(Bits,Data)
	/// </summary>
	public class Shake128 : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Shake128(Bits,Data)
		/// </summary>
		/// <param name="Bits">Number of bits of the hash digest.</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Shake128(ScriptNode Bits, ScriptNode Data, int Start, int Length, Expression Expression)
			: base(Bits, Data, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Shake128);

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			double n = Expression.ToDouble(Argument1.AssociatedObjectValue);
			int N = (int)n;

			if (N != n || N <= 0)
				throw new ScriptRuntimeException("Number of bits must be a positive integer.", this);

			if (!(Argument2.AssociatedObjectValue is byte[] Bin))
				throw new ScriptRuntimeException("Binary data expected in second argument.", this);

			SHAKE128 H = new SHAKE128(N);
			
			return new ObjectValue(H.ComputeVariable(Bin));
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateScalar(Argument1, Argument2, Variables));
		}

	}
}
