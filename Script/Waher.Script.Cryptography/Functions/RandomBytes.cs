using System;
using System.Numerics;
using System.Security.Cryptography;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Cryptography.Functions
{
	/// <summary>
	/// Generates an array of random bytes.
	/// </summary>
	public class RandomBytes : FunctionOneScalarVariable
	{
		/// <summary>
		/// Generates an array of random bytes.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RandomBytes(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "RandomBytes";

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			int N = (int)Argument;
			if (N != Argument || N < 0)
				throw new ScriptRuntimeException("Number of bytes must be non-negative.", this);

			byte[] Bin = new byte[N];

			lock (rnd)
			{
				rnd.GetBytes(Bin);
			}

			return new ObjectValue(Bin);
		}

		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

	}
}
