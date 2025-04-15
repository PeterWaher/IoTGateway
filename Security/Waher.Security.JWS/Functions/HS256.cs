using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWS.Functions
{
	/// <summary>
	/// Creates an HMAC SHA-256 JSON Web Signature key.
	/// </summary>
	public class HS256 : FunctionMultiVariate
	{
		/// <summary>
		/// Creates an HMAC SHA-256 JSON Web Signature key.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public HS256(int Start, int Length, Expression Expression)
			: base(Array.Empty<ScriptNode>(), argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates an HMAC SHA-256 JSON Web Signature key.
		/// </summary>
		/// <param name="Secret">Secret</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public HS256(ScriptNode Secret, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Secret }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(HS256);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Secret" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments.Length == 0)
				return new ObjectValue(new HmacSha256());

			object Obj = Arguments[0].AssociatedObjectValue;

			if (Obj is byte[] Bin)
				return new ObjectValue(new HmacSha256(Bin));
			else if (Obj is string s)
				return new ObjectValue(new HmacSha256(Convert.FromBase64String(s)));
			else
				throw new ScriptRuntimeException("Expected secret to be a binary array, or a base64-encoded string.", this);
		}
    }
}
