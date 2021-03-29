using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security.EllipticCurves;

namespace Waher.Script.Cryptography.Functions.Ecc
{
	/// <summary>
	/// Creates an Edwards448 Elliptic curve.
	/// </summary>
	public class Ed448 : FunctionMultiVariate
	{
		/// <summary>
		/// Creates an Edwards448 Elliptic curve.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ed448(int Start, int Length, Expression Expression)
			: base(new ScriptNode[0], argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates an Edwards448 Elliptic curve.
		/// </summary>
		/// <param name="PrivateKey">Private key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ed448(ScriptNode PrivateKey, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { PrivateKey }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates an Edwards448 Elliptic curve.
		/// </summary>
		/// <param name="PrivateKey">Private key.</param>
		/// <param name="HashKey">If the private key should be hashed before.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ed448(ScriptNode PrivateKey, ScriptNode HashKey, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { PrivateKey, HashKey }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ed448";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "PrivateKey" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;

			if (c == 0)
				return new ObjectValue(new Edwards448());

			object Obj = Arguments[0].AssociatedObjectValue;
			if (!(Obj is byte[] PrivateKey))
				PrivateKey = Convert.FromBase64String(Obj?.ToString());

			if (c == 1)
				return new ObjectValue(new Edwards448(PrivateKey));

			bool? HashKey = ToBoolean(Arguments[1]);
			if (!HashKey.HasValue)
				throw new ScriptRuntimeException("HashKey mst be a boolean value.", this);

			return new ObjectValue(new Edwards448(PrivateKey, HashKey.Value));
		}
	}
}
