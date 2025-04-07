using System;
using System.Security.Cryptography;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWS.Functions
{
	/// <summary>
	/// Creates an RSASSA-PKCS1-v1_5 SHA-256 JSON Web Signature key.
	/// </summary>
	public class RS256 : FunctionMultiVariate
	{
		/// <summary>
		/// Creates an RSASSA-PKCS1-v1_5 SHA-256 JSON Web Signature key.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public RS256(int Start, int Length, Expression Expression)
			: base(Array.Empty<ScriptNode>(), argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates an RSASSA-PKCS1-v1_5 SHA-256 JSON Web Signature key.
		/// </summary>
		/// <param name="Rsa">RSA definition</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public RS256(ScriptNode Rsa, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Rsa }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(RS256);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Rsa" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments.Length == 0)
				return new ObjectValue(new RsaSsaPkcsSha256());

			object Obj = Arguments[0].AssociatedObjectValue;

			if (Obj is RSA Rsa)
				return new ObjectValue(new RsaSsaPkcsSha256(Rsa));
			else if (Obj is RSAParameters Parameters)
				return new ObjectValue(new RsaSsaPkcsSha256(Parameters));
			else if (Obj is double NrBits)
				return new ObjectValue(new RsaSsaPkcsSha256((int)NrBits));
			else
				throw new ScriptRuntimeException("Expected argument to be an RSA object, RSAParameters object, or a number of bits.", this);
		}
	}
}
