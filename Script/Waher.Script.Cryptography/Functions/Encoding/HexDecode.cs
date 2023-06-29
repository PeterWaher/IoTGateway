using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Script.Cryptography.Functions.Encoding
{
	/// <summary>
	/// HexDecode(Hex)
	/// </summary>
	public class HexDecode : FunctionOneScalarVariable
	{
		/// <summary>
		/// HexDecode(Hex)
		/// </summary>
		/// <param name="Hex">Hex-encoded data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HexDecode(ScriptNode Hex, int Start, int Length, Expression Expression)
			: base(Hex, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(HexDecode);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(Hashes.StringToBinary(Argument));
		}

	}
}
