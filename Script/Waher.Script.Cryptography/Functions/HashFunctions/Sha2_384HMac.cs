using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Script.Cryptography.Functions.HashFunctions
{
	/// <summary>
	/// Sha2_384HMac(Data)
	/// </summary>
	public class Sha2_384HMac : FunctionTwoVariables
	{
		/// <summary>
		/// Sha2_384HMac(Data)
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Key">Binary key</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sha2_384HMac(ScriptNode Data, ScriptNode Key, int Start, int Length, Expression Expression)
			: base(Data, Key, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "sha2_384hmac"; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			if (!(Argument1.AssociatedObjectValue is byte[] Data))
				throw new ScriptRuntimeException("Binary data expected.", this);

			if (!(Argument2.AssociatedObjectValue is byte[] Key))
				throw new ScriptRuntimeException("Binary key expected.", this);

			return new ObjectValue(Hashes.ComputeHMACSHA384Hash(Key, Data));
		}

	}
}
