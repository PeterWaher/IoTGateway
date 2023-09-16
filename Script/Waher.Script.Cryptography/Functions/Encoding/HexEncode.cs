using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Script.Cryptography.Functions.Encoding
{
	/// <summary>
	/// HexEncode(Data)
	/// </summary>
	public class HexEncode : FunctionMultiVariate
	{
		/// <summary>
		/// HexEncode(Data)
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HexEncode(ScriptNode Data, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Data }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// HexEncode(Data)
		/// </summary>
		/// <param name="Integer">Integer to encode</param>
		/// <param name="NrBytes">Number of bytes to use when encoding integer</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HexEncode(ScriptNode Integer, ScriptNode NrBytes, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Integer, NrBytes }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(HexEncode);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Data" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;

			if (c == 1)
			{
				if (!(Arguments[0].AssociatedObjectValue is byte[] Bin))
					throw new ScriptRuntimeException("Binary data expected.", this);

				return new StringValue(Hashes.BinaryToString(Bin));
			}
			else
			{
				object Obj = Arguments[0].AssociatedObjectValue;
				int NrBytes = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);
				if (NrBytes <= 0)
					throw new ScriptRuntimeException("Number of bytes must be positive.", this);

				string s;

				if (Obj is double d)
				{
					if (d >= int.MinValue && d <= int.MaxValue)
						s = ((int)d).ToString("x");
					else if (d >= long.MinValue && d <= long.MaxValue)
						s = ((long)d).ToString("x");
					else
						s = ((BigInteger)d).ToString("x");
				}
				else if (Obj is BigInteger i)
					s = i.ToString("x");
				else
					throw new ScriptRuntimeException("Expected integer as first argument.", this);

				int MaxLen = NrBytes << 1;
				c = s.Length;

				if (c > MaxLen)
					throw new ScriptRuntimeException("Integer too long to fit.", this);
				else if (c < MaxLen)
				{
					if (s[0] >= '8')
						s = new string('f', MaxLen - c) + s;
					else
						s = new string('0', MaxLen - c) + s;
				}

				return new StringValue(s);
			}
		}
	}
}
