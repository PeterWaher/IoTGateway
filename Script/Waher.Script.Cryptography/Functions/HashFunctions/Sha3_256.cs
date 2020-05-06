using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;
using Waher.Security.SHA3;

namespace Waher.Script.Cryptography.Functions.HashFunctions
{
	/// <summary>
	/// Sha3_256(Data)
	/// </summary>
	public class Sha3_256 : FunctionOneScalarVariable
	{
		/// <summary>
		/// Sha3_256(Data)
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sha3_256(ScriptNode Data, int Start, int Length, Expression Expression)
			: base(Data, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "sha3_256"; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			if (!(Argument.AssociatedObjectValue is byte[] Bin))
				throw new ScriptRuntimeException("Binary data expected.", this);

			SHA3_256 H = new SHA3_256();

			return new ObjectValue(H.ComputeVariable(Bin));
		}

	}
}
