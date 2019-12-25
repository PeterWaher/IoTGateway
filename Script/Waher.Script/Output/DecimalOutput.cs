using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Decimal to expression strings.
	/// </summary>
	public class DecimalOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(decimal);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			return Expression.ToString((decimal)Value);
		}
	}
}
