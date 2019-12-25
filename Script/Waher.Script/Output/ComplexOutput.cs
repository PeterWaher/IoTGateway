using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Complex to expression strings.
	/// </summary>
	public class ComplexOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(Complex);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			return Expression.ToString((Complex)Value);
		}
	}
}
