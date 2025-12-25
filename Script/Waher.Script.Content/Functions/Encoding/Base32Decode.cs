using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Base32Decode(Base32)
	/// </summary>
	public class Base32Decode : FunctionOneScalarStringVariable
	{
		/// <summary>
		/// Base32Decode(Base32)
		/// </summary>
		/// <param name="Base32">Base32-encoded data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Base32Decode(ScriptNode Base32, int Start, int Length, Expression Expression)
			: base(Base32, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Base32Decode);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(Base32.Decode(Argument));
		}

	}
}
