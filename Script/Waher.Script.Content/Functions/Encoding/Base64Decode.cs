using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Base64Decode(Base64)
	/// </summary>
	public class Base64Decode : FunctionOneScalarVariable
	{
		/// <summary>
		/// Base64Decode(Base64)
		/// </summary>
		/// <param name="Base64">Base64-encoded data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Base64Decode(ScriptNode Base64, int Start, int Length, Expression Expression)
			: base(Base64, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "base64decode"; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(Convert.FromBase64String(Argument));
		}

	}
}
