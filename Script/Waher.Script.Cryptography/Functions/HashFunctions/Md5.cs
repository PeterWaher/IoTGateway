using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Script.Cryptography.Functions.HashFunctions
{
	/// <summary>
	/// Md5(Data)
	/// </summary>
	public class Md5 : FunctionOneScalarVariable
	{
		/// <summary>
		/// Md5(Data)
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Md5(ScriptNode Data, int Start, int Length, Expression Expression)
			: base(Data, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "md5"; }
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
			{
				string s = Argument is StringValue S ? S.Value : Expression.ToString(Argument.AssociatedObjectValue);
				Bin = System.Text.Encoding.UTF8.GetBytes(s);
			}

			return new ObjectValue(Hashes.ComputeMD5Hash(Bin));
		}

	}
}
